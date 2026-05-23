using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Caching;
using CatalogService.Infrastructure.Messaging;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Seeding;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Identity;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsEnvironment("Testing"))
            {
                // DbContext
                services
                .AddDbContext<CatalogDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("CatalogDb"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName)));
            }

            services.AddScoped<ICatalogDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());
            services.AddScoped<MessageDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());

            // Domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Integration event publisher (writes to outbox)
            services.AddScoped<IEventPublisher, OutboxEventPublisher>();

            // Outbox message sender + background processor
            services.AddScoped<IOutboxMessageSender, MassTransitOutboxMessageSender>();
            services.AddHostedService<OutboxMessageProcessor>();

            // MassTransit + RabbitMQ
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"] ?? throw new InvalidOperationException("RabbitMQ:Host configuration is missing.");
                    var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? throw new InvalidOperationException("RabbitMQ:Username configuration is missing.");
                    var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password configuration is missing.");

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUsername);
                        h.Password(rabbitMqPassword);
                    });

                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15)));

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddTokenAuthentication(configuration);

            services.AddStackExchangeRedisCache(options =>
            {
                string? host = configuration["Redis:Host"];
                string? port = configuration["Redis:Port"];
                string? instanceName = configuration["Redis:InstanceName"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
                {
                    throw new InvalidOperationException("Redis connection string is missing in configuration.");
                }

                options.Configuration = $"{host}:{port}";
                options.InstanceName = instanceName;
            });

            services.AddScoped<IProductCache, RedisProductCache>();

            return services;
        }

        public static async Task<IApplicationBuilder> Initialize(this IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var logger = serviceProvider.GetService<ILogger<Program>>();

            // DbContext may not be registered in the "Testing" environment, skip if not available
            var dbContext = serviceProvider.GetService<CatalogDbContext>();
            if (dbContext != null)
            {
                const int maxAttempts = 10;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        await dbContext.Database.MigrateAsync();
                        logger?.LogInformation("Applied Catalog DB migrations successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Attempt {Attempt} to apply Catalog DB migrations failed.", attempt);

                        if (attempt == maxAttempts)
                        {
                            logger?.LogError(ex, "Exceeded retry attempts while applying Catalog DB migrations.");
                            throw;
                        }

                        // exponential backoff with a cap
                        int delaySeconds = Math.Min(30, attempt * 2);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }

                await CategoriesSeeder.SeedCategoriesAsync(dbContext);
            }

            return app;
        }
    }
}
