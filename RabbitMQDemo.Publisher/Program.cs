using System;
using System.Net;
using System.Net.Http;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQDemo.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string vhost = AskForVHost();

            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = vhost
            };

            EnsureThatVHostExists(vhost, connectionFactory);

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                while (true)
                {
                    Console.WriteLine("Type your message");
                    var teste = Console.ReadLine();

                    channel.QueueDeclare(queue: "TestesASPNETCore",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message =
                        $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} - " +
                        $"Conteúdo da Mensagem: {teste}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "TestesASPNETCore",
                                         basicProperties: null,
                                         body: body);
                }
            }
        }

        private static string AskForVHost()
        {
            Console.WriteLine("Call VHOST");
            var vhost = Console.ReadLine();
            return vhost;
        }

        private static void EnsureThatVHostExists(string vhost, ConnectionFactory factory)
        {
            var credentials = new NetworkCredential() { UserName = factory.UserName, Password = factory.Password };
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                var url = $"http://{factory.HostName}:15672/api/vhosts/{vhost}";

                var content = new StringContent("", Encoding.UTF8, "application/json");
                var result = client.PutAsync(url, content).Result;

                if ((int)result.StatusCode >= 300)
                    throw new Exception(result.ToString());
            }
        }
    }
}
