using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AirlineSendAgent.App
{
    public class AppHost : IAppHost
    {
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
                };

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.ReadLine();
            }
        }
    }
}