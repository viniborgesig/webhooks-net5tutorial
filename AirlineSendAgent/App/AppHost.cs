using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using AirlineSendAgent.Client;
using AirlineSendAgent.Data;
using AirlineSendAgent.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AirlineSendAgent.App
{
    public class AppHost : IAppHost
    {
        private readonly SendAgentDbContext _context;
        private readonly IWebhookClient _webhookClient;

        public AppHost(SendAgentDbContext context, IWebhookClient webhookClient)
        {
            _context = context;
            _webhookClient = webhookClient;
        }

        public void Run()
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672
            };

            using (var connection = connectionFactory.CreateConnection(clientProvidedName: "AirlineSendAgent"))
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "trigger",
                                        type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: "trigger",
                                  routingKey: "");

                var consumer = new EventingBasicConsumer(channel);

                Console.WriteLine("Escutando o RabbitMQ...");

                consumer.Received += async (ModuleHandle, EventArgs) =>
                {
                    Console.WriteLine("Evento foi disparado...");

                    var body = EventArgs.Body;
                    var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
                    var notificationMessageDto = JsonSerializer.Deserialize<NotificationMessageDto>(notificationMessage);

                    var webhookToSend = new FlightDetailChangePayloadDto
                    {
                        WebhookURI = string.Empty,
                        WebhookType = notificationMessageDto.WebhookType,
                        FlightCode = notificationMessageDto.FlightCode,
                        Publisher = string.Empty,
                        Secret = string.Empty,
                        OldPrice = notificationMessageDto.OldPrice,
                        NewPrice = notificationMessageDto.NewPrice
                    };

                    foreach (var webhookSubscription in _context.WebhookSubscriptions
                        .Where(ws => ws.WebhookType.Equals(notificationMessageDto.WebhookType)))
                    {
                        webhookToSend.WebhookURI = webhookSubscription.WebhookURI;
                        webhookToSend.Secret = webhookSubscription.Secret;
                        webhookToSend.Publisher = webhookSubscription.WebhookPublisher;

                        await _webhookClient.SendWebhookNotification(webhookToSend);
                    }
                };

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.ReadLine();
            }
        }
    }
}