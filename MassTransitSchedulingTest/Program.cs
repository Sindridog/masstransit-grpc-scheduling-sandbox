using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using Serilog.Events;
using System;

namespace MassTransitSchedulingTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(
                    new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Error)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .CreateLogger(),
                    dispose: true));

                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                });

                services.AddMassTransit(x =>
                {
                    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("test", false));
                    x.AddPublishMessageScheduler();
                    x.AddQuartzConsumers();
                    x.AddConsumer<MessageConsumer>();
                    x.UsingGrpc((context, cfg) =>
                    {
                        cfg.Host(h =>
                        {
                            Uri uri = new Uri("http://localhost:11001");
                            h.Host = uri.Host;
                            h.Port = uri.Port;
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                });

                services.Configure<MassTransitHostOptions>(opt =>
                {
                    opt.WaitUntilStarted = true;
                });

                services.AddQuartzHostedService(options =>
                {
                    options.StartDelay = TimeSpan.FromSeconds(5);
                    options.WaitForJobsToComplete = true;
                });

                services.AddHostedService<SchedulerInitService>();
            });
    }
}
