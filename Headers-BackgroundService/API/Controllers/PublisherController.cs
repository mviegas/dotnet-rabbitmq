// using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitMQDemo.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PublisherController : ControllerBase
    {
        private readonly ILogger<PublisherController> _logger;
        private readonly IConnectionFactory _connectionFactory;

        public PublisherController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PublisherController>();
            _connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Up and running! Try to POST something at this endpoint and watch the console ;)");
        }

        [HttpPost]
        public IActionResult Post(Message message)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                const string Exchange = "myExchange";

                channel.ExchangeDeclare(
                    exchange: Exchange,
                    type: "headers",
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                var basicProperties = channel.CreateBasicProperties();

                if (!basicProperties.IsHeadersPresent())
                    basicProperties.Headers = new Dictionary<string, object>();

                basicProperties.Headers.Add("Key", message.Key);

                var body = Encoding.UTF8.GetBytes(message.Value);

                channel.QueueDeclare(
                    queue: message.Key == "Key" ? "Key" : "Key2",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                channel.QueueBind(
                    queue: message.Key == "Key" ? "Key" : "Key2",
                    exchange: "myExchange",
                    routingKey: string.Empty,
                    arguments: new Dictionary<string, object>() { { "Key", message.Key } });

                channel.BasicPublish(
                    exchange: Exchange,
                    routingKey: string.Empty,
                    basicProperties: basicProperties,
                    body: body);
            }

            return Ok();
        }

    }

    public class Message
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
