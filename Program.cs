using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace sample_application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Add --openssl-legacy-provider to env vars as this project requires old OpenSSL alogrithms
            var nodeOptions = Environment.GetEnvironmentVariable("NODE_OPTIONS");
            if (nodeOptions == null)
            {
                Environment.SetEnvironmentVariable("NODE_OPTIONS", "--openssl-legacy-provider");
            } else
            {
                Environment.SetEnvironmentVariable("NODE_OPTIONS", $"{nodeOptions} --openssl-legacy-provider");
            }
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
