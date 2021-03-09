using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using PrinterService.Consumers;
using PrinterService.Daemons;

namespace PrinterService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            services.AddMassTransit(c =>
            {
                c.AddConsumer<PrintLabelCommandHandler>();
  
                c.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    IRabbitMqHost host = cfg.Host(new Uri(Configuration["RabbitMqHostName"]),
                        hostConfigurator =>
                        {
                            hostConfigurator.Username(Configuration["RabbitMqUsername"]);
                            hostConfigurator.Password(Configuration["RabbitMqPassword"]);
                        });
                    
                    services.AddSingleton<IPublishEndpoint>(p => p.GetRequiredService<IBusControl>());
                    services.AddSingleton<ISendEndpointProvider>(p => p.GetRequiredService<IBusControl>());
                    services.AddSingleton<IBus>(p => p.GetRequiredService<IBusControl>());

                    cfg.ReceiveEndpoint("print-label",
                        ep =>
                        {
                            ep.BindMessageExchanges = false;
                            ep.ExchangeType = ExchangeType.Direct;
                            ep.Bind("BoxService:PrintLabelCommand", x =>
                            {});
                            ep.PrefetchCount = Convert.ToUInt16(Configuration["PrefetchCount"]);
                            ep.UseConcurrencyLimit(Convert.ToUInt16(Configuration["UseConcurrencyLimit"])); 
                            ep.ConfigureConsumer<PrintLabelCommandHandler>(provider);

                            ep.UseMessageRetry(r =>
                                r.Incremental(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)));
                        }
                    );
                }));

                services.AddHostedService<RabbitMqBusService>();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHealthChecks("/healthcheck");
        }
    }
}