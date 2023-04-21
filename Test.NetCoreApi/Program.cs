using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace Test.NetCoreApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceExtensions.SetAssemblyName(Assembly.GetExecutingAssembly().GetName().Name);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
                                          .AddJsonFile("hosting.json")
                                          .Build();

                    var url = configuration["server.urls"];
                    webBuilder.UseUrls(new string[] { url }).UseStartup<Startup>();
                    //webBuilder.UseStartup<Startup>();
                });
    }
}
