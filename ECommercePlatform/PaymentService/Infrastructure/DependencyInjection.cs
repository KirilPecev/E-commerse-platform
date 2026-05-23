using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Identity;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Gateways;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Messaging.Consumers;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsEnvironment("Testing"))
            {
                // DbContext
                services
                .AddDbContext<PaymentDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("PaymentDb"),
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName)));
            }

            services.AddScoped<IPaymentDbContext>(sp => sp.GetRequiredService<PaymentDbContext>());
            services.AddScoped<MessageDbContext>(sp => sp.GetRequiredService<PaymentDbContext>());

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
                x.AddConsumers(typeof(OrderFinalizedIntegrationEventConsumer).Assembly);

                x.SetEndpointNameFormatter(
                    new KebabCaseEndpointNameFormatter("payment", false));

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

            services.AddTransient<IPaymentGateway, CardPaymentGateway>();

            return services;
        }

        public static async Task<IApplicationBuilder> Initialize(this IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var logger = serviceProvider.GetService<ILogger<Program>>();

            var dbContext = serviceProvider.GetService<PaymentDbContext>();
            if (dbContext != null)
            {
                const int maxAttempts = 10;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        await dbContext.Database.MigrateAsync();
                        logger?.LogInformation("Applied Payment DB migrations successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Attempt {Attempt} to apply Payment DB migrations failed.", attempt);
                        if (attempt == maxAttempts)
                        {
                            logger?.LogError(ex, "Exceeded retry attempts while applying Payment DB migrations.");
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
