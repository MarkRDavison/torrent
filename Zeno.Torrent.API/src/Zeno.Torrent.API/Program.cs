using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zeno.Torrent.API.Core.Configuration;

namespace Zeno.Torrent.API {
    public class Program {

        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(urls: Environment.GetEnvironmentVariable("ZENO_TORRENT_DAEMON_URL") ?? "http://0.0.0.0:10000");
                })
                .ConfigureAppConfiguration((hostingContext, configurationBuilder) => {
                    configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configurationBuilder.AddEnvironmentVariables();
                });
    }
}
