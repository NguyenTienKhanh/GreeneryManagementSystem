using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GMS_Lite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            //var host = CreateWebHostBuilder(args).Build();

            //using (var scope = host.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    try
            //    {
            //        var serviceProvider = services.GetRequiredService<IServiceProvider>();
            //        var configuration = services.GetRequiredService<IConfiguration>();
            //        Seed.CreateRoles(serviceProvider, configuration).Wait();

            //    }
            //    catch (Exception exception)
            //    {
            //        var logger = services.GetRequiredService<ILogger<Program>>();
            //        logger.LogError(exception, "An error occurred while creating roles");
            //    }
            //}

            //host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            //var config = new ConfigurationBuilder()
            //            .SetBasePath(Directory.GetCurrentDirectory())
            //            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
            //            .Build();
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    //config.AddJsonFile("config.json", optional: true, reloadOnChange: false);
                    config.AddCommandLine(args);
                })
                .UseStartup<Startup>();
            return host;
        }

    }
}
