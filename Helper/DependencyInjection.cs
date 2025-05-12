using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Cryptography;

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

        public static IServiceCollection AddReqResLogging(this IServiceCollection services)
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

        public static IServiceCollection AddSsoConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var ssoConfig = configuration.GetSection("SsoConfig").Get<SsoConfig>();

            var publicKeyBytes = Convert.FromBase64String(ssoConfig.PublicKey);
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out int _);
            var rsaSecurityKey = new RsaSecurityKey(rsa);

            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
                {
                    o.MetadataAddress = ssoConfig.MetadataAddress;
                    o.Authority = ssoConfig.Authority;
                    o.RequireHttpsMetadata = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidIssuer = ssoConfig.ValidIssuer,
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        IssuerSigningKey = rsaSecurityKey,
                        ValidateLifetime = true,
                    };
                });

            return services;
        }

        public static IServiceCollection AddProviderHttpClient(this IServiceCollection services, IConfiguration configuration, string clientName, string configSectionName)
        {
            var config = configuration.GetSection(configSectionName).Get<BaseHttpClientConfig>();
            services.AddHttpClient(clientName, o =>
            {
                o.BaseAddress = new Uri(config.BaseUrl);
                o.Timeout = config.Timeout;
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            }).SetHandlerLifetime(TimeSpan.FromMinutes(10));

            return services;
        }
    }
}
