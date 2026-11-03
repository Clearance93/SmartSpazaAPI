using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartData;

namespace SmartSpazaAPI.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _ScopeFactory;

        public DatabaseHealthCheck(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
                                                             CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _ScopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

                return canConnect
                    ? HealthCheckResult.Healthy("The database is reachable.")
                    : HealthCheckResult.Unhealthy("The database is not reachable.");
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy("The database is not reachable.", exception);
            }
        }
    }
}
