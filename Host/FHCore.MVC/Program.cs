using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FHCore.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
            .UseUrls("http://*:6606")
            .Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingcontext,config)=>{
                var dict = new Dictionary<string, string>
                {
                    {"test", "DEV_1111111-1111"},
                    {"test1", "PROD_2222222-2222"}
                };
                config.AddInMemoryCollection(dict);
                config.AddCommandLine(args);
            })
            .UseStartup<Startup>();
    }
}
