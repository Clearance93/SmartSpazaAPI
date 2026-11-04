using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SmartConfiguration;
using SmartData;
using SmartEnums;
using SmartInterfaces.IService;
using SmartModels;
using SmartSpazaAPI.Configuration;
using SmartSpazaAPI.Filters;
using SmartSpazaAPI.HealthChecks;
using SmartSpazaAPI.Middleware;
using SmartUtility;
using SmartUtility.Sealed;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;

    // API payloads are small; cap request bodies to reduce memory-exhaustion abuse.
    options.Limits.MaxRequestBodySize = 1024 * 1024;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

var configuration = builder.Configuration;

// Strongly-typed, fail-fast configuration.
builder.Services.AddOptions<JwtOptions>()
    .Bind(configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => builder.Environment.IsDevelopment() ||
                         (!string.IsNullOrWhiteSpace(options.Key) &&
                          Encoding.UTF8.GetByteCount(options.Key) >= 32),
              "Jwt:Key must be configured and at least 32 bytes in non-Development environments.")
    .ValidateOnStart();

var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");

var jwtKey = jwtOptions.Key;

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be configured and at least 32 bytes.");
}

var connectionString = configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("The 'ConnectionString' connection string is not configured.");

var requireEmailConfirmation = configuration.GetValue("Auth:RequireEmailConfirmation", true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = false;
});

SqlMapper.AddTypeHandler(new TimeOnlyHandler());
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                });

const string corsPolicyName = "AppCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var allowWildcard = allowedOrigins.Contains("*");

        // The API authenticates with bearer tokens (no cookies), so allowing any origin is
        // safe and lets the public marketing site post registrations from any domain.
        if (allowWildcard || allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowCredentials();
        }

        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

var knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    foreach (var proxy in knownProxies)
    {
        if (IPAddress.TryParse(proxy, out var address))
        {
            options.KnownProxies.Add(address);
        }
    }
});

builder.Services.AddApplicationDependencies(configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromSeconds(30),
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SupplierOnly", policy => policy.RequireRole(RoleNames.Supplier))
    .AddPolicy("SpazaOwnerOnly", policy => policy.RequireRole(RoleNames.SpazaOwner))
    .AddPolicy("DriverOnly", policy => policy.RequireRole(RoleNames.Driver))
    .AddPolicy("BusinessUsers", policy => policy.RequireRole(RoleNames.BusinessUsers));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 30,
                QueueLimit = 0
            }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 600,
                QueueLimit = 0
            }));
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var swaggerEnabled = configuration.GetValue("Swagger:Enabled", builder.Environment.IsDevelopment());

if (swaggerEnabled)
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SmartSpaza API",
            Version = "v1",
            Description = "SmartSpaza last-mile delivery and inventory API."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the access token returned by POST /api/Auth/login."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });
    });
}

var app = builder.Build();

if (requireEmailConfirmation)
{
    using var validationScope = app.Services.CreateScope();
    validationScope.ServiceProvider.GetRequiredService<IConfirmationEmailSender>().ValidateConfiguration();
}

var seedDemoData = app.Environment.IsDevelopment();

await ApplyMigrationsAndSeedAsync(app, seedDemoData);

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

var requireHttps = configuration.GetValue("Https:RequireHttps", !app.Environment.IsDevelopment());

if (requireHttps)
{
    app.UseMiddleware<HttpsRequiredMiddleware>();
}
else if (!app.Environment.IsDevelopment())
{
    app.Logger.LogWarning("HTTPS enforcement is disabled (Https:RequireHttps=false). Use this only for a local-only demo.");
}

app.UseMiddleware<ExceptionConfigurations>();

// API payloads are per-user; never let a CDN, proxy or browser cache them.
// (The HTTPS deployment sits behind Netlify's edge proxy, which would otherwise
// be free to cache one shop owner's catalogue and serve it to another.)
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
    }

    await next();
});

// Serve the hosted Angular SPA (files copied into wwwroot by the UI build).
// .apk is not in the default MIME table, so the Android scanner download would 404
// without this mapping.
var staticFileContentTypes = new FileExtensionContentTypeProvider();
staticFileContentTypes.Mappings[".apk"] = "application/vnd.android.package-archive";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticFileContentTypes
});

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartSpaza API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "SmartSpaza API";
        options.DisplayRequestDuration();
        options.EnablePersistAuthorization();
    });
}

app.UseCors(corsPolicyName);

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Client-side routes (/login, /scanner, /dashboard, …) fall back to the Angular index.
// Unknown API paths and missing files keep returning 404 instead of HTML.
app.MapFallback(async context =>
{
    var path = context.Request.Path;

    if (path.StartsWithSegments("/api") ||
        path.StartsWithSegments("/health") ||
        Path.HasExtension(path.Value))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var webRoot = app.Environment.WebRootPath;
    var indexPath = string.IsNullOrWhiteSpace(webRoot) ? null : Path.Combine(webRoot, "index.html");

    if (indexPath is null || !File.Exists(indexPath))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

static async Task ApplyMigrationsAndSeedAsync(WebApplication app, bool seedDemoData)
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    logger.LogInformation("Applying pending migrations (if any ...)");
    await db.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied successfully.");

    logger.LogInformation("Starting role seeding ...");
    await RoleSeeder.SeedRoleAsync(roleManager, logger);
    logger.LogInformation("Role seeding completed successfully.");

    if (seedDemoData)
    {
        logger.LogInformation("Starting demo user seeding ...");
        await UserSeeder.SeedDemoUserAsync(userManager, roleManager, db, logger);
        logger.LogInformation("Demo user seeding completed successfully.");

        logger.LogInformation("Starting product seeding ...");
        await ProductSeeder.SeedProductsAsync(db, logger);
        logger.LogInformation("Product seeding completed successfully.");
    }
    else
    {
        logger.LogInformation("Demo data seeding is disabled for this environment.");
    }
}
