using System;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQDemo.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionHelper = new RabbitMqConnectionHelper(3);

            try
            {
                if (!connectionHelper.TryConnect())
                {
                    Console.WriteLine("No RabbitMQ connections are available to perform this action");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An irrecoverable error has ocurred while trying to connect to RabbitMQ. Exception: {ex.Message}.");
                Environment.Exit(0);
            }

            try
            {
                using (var channel = connectionHelper.Connection.CreateModel())
                {
                    while (true)
                    {
                        channel.QueueDeclare(
                            queue: "tests",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        Console.WriteLine("Type your message");

                        var teste = Console.ReadLine();

                        string message =
                            $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} - " +
                            $"Message content: {teste}";
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: "tests",
                            basicProperties: null,
                            body: body);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An irrecoverable error has ocurred while trying to connect to RabbitMQ. Exception: {ex.Message}.");
                Environment.Exit(0);
            }
        }
    }
}