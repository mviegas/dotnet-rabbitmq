using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQDemo.API.Services
{
    public class FirstHeaderConsumer : BackgroundService
    {
        private const string Queue = "firstQueue";
        private readonly ILogger<FirstHeaderConsumer> _logger;

        public FirstHeaderConsumer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FirstHeaderConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionFactory = new ConnectionFactory();

            using var connection = connectionFactory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, eventArgs) =>
                {
                    _logger.LogInformation("Message Received");

                    var message = Encoding.UTF8.GetString(eventArgs.Body);

                    _logger.LogInformation(Environment.NewLine + "[New message received] from first header" + message);

                    channel.BasicAck(eventArgs.DeliveryTag, false);
                };

                channel.QueueDeclare(
                    queue: Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.BasicConsume(
                    queue: Queue,
                    autoAck: false,
                    consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken).ConfigureAwait(false);
                }
            }

            connection.Dispose();
        }
    }
}