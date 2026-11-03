using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartEnums;

namespace SmartUtility
{
    public class RoleSeeder
    {
        public static async Task SeedRoleAsync(RoleManager<IdentityRole> rolemanager, ILogger? logger = null)
        {
            var roles = new[]
            {
                RoleNames.Supplier,
                RoleNames.SpazaOwner,
                RoleNames.Driver
            };

            foreach (var role in roles)
            {
                logger?.LogInformation($"Checking if role '{role}' exists...");

                if (!await rolemanager.RoleExistsAsync(role))
                {
                    logger?.LogInformation($"Creating role '{role}' ....");
                    var result = await rolemanager.CreateAsync(new IdentityRole(role));

                    if (result.Succeeded)
                    {
                        logger?.LogInformation($"Role '{role}' created successfully.");
                    }
                    else
                    {
                        logger?.LogError($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger?.LogInformation($"Role '{role}' already exists.");
                }
            }
        }
    }
}
