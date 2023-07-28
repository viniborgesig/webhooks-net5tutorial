using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using AirlineSendAgent.App;
using AirlineSendAgent.Client;
using AirlineSendAgent.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AirlineSendAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IAppHost, AppHost>();
                    services.AddSingleton<IWebhookClient, WebhookClient>();

                    services.AddDbContext<SendAgentDbContext>(opt => opt.UseSqlServer
                        (context.Configuration.GetConnectionString("AirlineConnection")));

                    services.AddHttpClient();
                }).Build();

            host.Services.GetService<IAppHost>().Run();
        }
    }
}
