using MareSynchronosServices;
using MareSynchronosShared.Data;
using MareSynchronosShared.Services;
using MareSynchronosShared.Utils.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder(args);
        var host = hostBuilder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            using var dbContext = services.GetRequiredService<MareDbContext>();

            var options = host.Services.GetService<IConfigurationService<ServicesConfiguration>>();
            var optionsServer = host.Services.GetService<IConfigurationService<ServerConfiguration>>();
            var logger = host.Services.GetService<ILogger<Program>>();
            logger.LogInformation("Loaded MareSynchronos Services Configuration (IsMain: {isMain})", options.IsMain);
            logger.LogInformation(options.ToString());
            logger.LogInformation("Loaded MareSynchronos Server Configuration (IsMain: {isMain})", optionsServer.IsMain);
            logger.LogInformation(optionsServer.ToString());
        }

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseContentRoot(AppContext.BaseDirectory);
                webBuilder.ConfigureLogging((ctx, builder) =>
                {
                    builder.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                    builder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                });
                webBuilder.UseStartup<Startup>();
            });
}