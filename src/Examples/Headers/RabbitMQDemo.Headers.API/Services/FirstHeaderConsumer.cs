using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharpRabbit;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQDemo.Headers.API.Services
{
    public class FirstHeaderConsumer : BackgroundService
    {
        private const string Queue = "firstQueue";
        private readonly ILogger<FirstHeaderConsumer> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRabbitConnection _rabbitConnection;

        public FirstHeaderConsumer(ILoggerFactory loggerFactory,
            IRabbitConnection rabbitConnection)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<FirstHeaderConsumer>();
            _rabbitConnection = rabbitConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var channel = _rabbitConnection.CreateModel())
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

            _rabbitConnection.Dispose();
        }
    }
}