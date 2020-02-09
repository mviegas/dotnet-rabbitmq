using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SharpRabbit
{
    public static class Registration
    {
        public static IServiceCollection AddSharpRabbit(this IServiceCollection owner, string connectionString)
        {
            owner.AddSingleton<IRabbitConnection>(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                return new RabbitConnection(
                    loggerFactory,
                    connectionFactory: new ConnectionFactory()
                    {
                        Endpoint = new AmqpTcpEndpoint(connectionString)
                    });
            });

            return owner;
        }
    }
}
