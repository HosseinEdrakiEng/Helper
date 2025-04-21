using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Helper
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiVersion(this IServiceCollection services)
        {
            services.AddApiVersioning(setup =>
            {
                setup.DefaultApiVersion = new ApiVersion(1, 0);
                setup.AssumeDefaultVersionWhenUnspecified = true;
                setup.ReportApiVersions = false;
                setup.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            return services;
        }

        public static IServiceCollection AddExceptionMiddleware(this IServiceCollection services)
        {
            services.AddScoped<ExceptionHandlingMiddleware>();

            return services;
        }

        public static IServiceCollection AddSerilogger(this IServiceCollection services, IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                       .ReadFrom.Configuration(configuration)
                       .CreateLogger();

            services.AddSerilog(logger);

            return services;
        }

        public static IServiceCollection AddHttpLogging(this IServiceCollection services)
        {
            services.AddHttpLogging((logger) =>
            {
                logger.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
                logger.CombineLogs = true;
            });

            return services;
        }

        public static IServiceCollection AddQueuedHostedWorker(this IServiceCollection services)
        {
            services.AddHostedService<QueuedHostedService>();
            return services;
        }

        public static IServiceCollection BackgroundTaskQueueService(this IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTaskQueue>(_ =>
            {
                return new BackgroundTaskQueue(100);
            });

            return services;
        }
    }
}
