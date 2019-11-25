using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


namespace NetCore.Grpc.Server
{
    public class Program
    {
        internal static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", optional: false)
            .Build();

        
        public static void Main(string[] args)
        {
            var listeningPort = Configuration.GetValue<int>("ListeningPort");
            
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            options.Listen(IPEndPoint.Parse($"0.0.0.0:{listeningPort}"),
                                l =>
                                {
                                    l.Protocols = HttpProtocols.Http2;
                                    l.UseHttps();
                                });
                        })
                        .UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}