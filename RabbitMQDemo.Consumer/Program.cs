using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQDemo.Consumer
{
    public class Program
    {
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "tangaras.cmflex.com.br"
            };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "TestesASPNETCore",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (sender, eventArgs) =>
                {
                    var message = Encoding.UTF8.GetString(eventArgs.Body);

                    Console.WriteLine(Environment.NewLine + "[Nova mensagem recebida] " + message);

                    channel.BasicAck(eventArgs.DeliveryTag, false);
                };

                channel.BasicConsume(queue: "TestesASPNETCore",
                     autoAck: false,
                     consumer: consumer);

                Console.WriteLine("Aguardando mensagens para processamento");
                Console.WriteLine("Pressione uma tecla para encerrar...");
                Console.ReadKey();
            }
        }
    }

    internal class RabbitMQConfigurations
    {
        public RabbitMQConfigurations()
        {
        }

        public string HostName { get; internal set; }
        public int Port { get; internal set; }
        public string UserName { get; internal set; }
        public string Password { get; internal set; }
    }
}
