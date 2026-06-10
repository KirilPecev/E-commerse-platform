using ECommercePlatform.Identity;

using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Seeding;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsEnvironment("Testing"))
            {
                // DbContext
                services
                    .AddDbContext<IdentityDbContext>(options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("IdentityDb"),
                            sqlOptions => sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName)));
            }

            services
                .AddIdentity<User, Role>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddTokenAuthentication(configuration);

            services.AddTransient<IJwtTokenGenerator, JwtTokenGeneratorService>();

            return services;
        }

        public static async Task<IApplicationBuilder> Initialize(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // DbContext may not be registered in the "Testing" environment, skip if not available
            var dbContext = serviceProvider.GetService<IdentityDbContext>();
            if (dbContext != null && environment.EnvironmentName != "Testing")
            {
                const int maxAttempts = 10;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        await dbContext.Database.MigrateAsync();
                        logger?.LogInformation("Applied Identity DB migrations successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Attempt {Attempt} to apply Identity DB migrations failed.", attempt);

                        if (attempt == maxAttempts)
                        {
                            logger?.LogError(ex, "Exceeded retry attempts while applying Identity DB migrations.");
                            throw;
                        }

                        int delaySeconds = Math.Min(30, attempt * 2);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
            }

            RoleManager<Role> roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            await RoleSeeder.SeedRolesAsync(roleManager);

            return app;
        }
    }
}
