using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Identity;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Messaging.Consumers;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsEnvironment("Testing"))
            {
                // DbContext
                services
                .AddDbContext<OrdersDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("OrdersDb"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(OrdersDbContext).Assembly.FullName)));
            }

            services.AddScoped<IOrdersDbContext>(sp => sp.GetRequiredService<OrdersDbContext>());
            services.AddScoped<MessageDbContext>(sp => sp.GetRequiredService<OrdersDbContext>());

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
                x.AddConsumers(typeof(PaymentCompletedEventConsumer).Assembly);

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

            return services;
        }

        public static async Task<IApplicationBuilder> Initialize(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var logger = serviceProvider.GetService<ILogger<Program>>();

            var dbContext = serviceProvider.GetService<OrdersDbContext>();
            if (dbContext != null && environment.EnvironmentName != "Testing")
            {
                const int maxAttempts = 10;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        await dbContext.Database.MigrateAsync();
                        logger?.LogInformation("Applied Orders DB migrations successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Attempt {Attempt} to apply Orders DB migrations failed.", attempt);
                        if (attempt == maxAttempts)
                        {
                            logger?.LogError(ex, "Exceeded retry attempts while applying Orders DB migrations.");
                            throw;
                        }

                        int delaySeconds = Math.Min(30, attempt * 2);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
            }

            return app;
        }
    }
}
