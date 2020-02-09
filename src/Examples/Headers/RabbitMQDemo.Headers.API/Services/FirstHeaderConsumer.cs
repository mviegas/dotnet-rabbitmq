using MateusViegas.Net.RabbitMQ.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQDemo.API.Services
{
    public class FirstHeaderConsumer : BackgroundService
    {
        private const string Queue = "firstQueue";
        private readonly ILogger<FirstHeaderConsumer> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public FirstHeaderConsumer(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<FirstHeaderConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = new RabbitConnection(
                _loggerFactory,
                new ConnectionFactory()
                {
                });

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