using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = ConfigureServices();

//var minioService = services.GetRequiredService<IMinioClientFactory>();
//var minioClient = minioService.CreateClient();
//var isExist = await minioClient.ListBucketsAsync();

//var serviceminio = services.GetService<IMinioService>();
////await serviceminio.PutObjectAsync(default);

//var byteContent = await serviceminio.GetObjectAsync(default);

Console.ReadLine(); 

static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();
    services.AddHttpClient();

    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
        .AddJsonFile("appsettings.json", false)
        .Build();

    //services.AddScoped<IMinioService, MinioService>();

    //services.AddOptions();
    //services.Configure<Helper.MinioConfig>(configuration.GetSection("MinioConfig"));
    //services.AddMinioConfiguration(configuration);

    return services.BuildServiceProvider();
}