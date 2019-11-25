using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
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
            var certPath = Path.Combine(Environment.CurrentDirectory, "Certs", "server.pfx");
            
            var cert = new X509Certificate2(certPath, "1111");
            
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            options.Listen(IPEndPoint.Parse($"0.0.0.0:{listeningPort}"),
                                l =>
                                {
                                    l.UseHttps(cert);
                                });
                        })
                        .UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}