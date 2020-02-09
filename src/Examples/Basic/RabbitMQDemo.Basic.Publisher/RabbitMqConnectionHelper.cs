using System;
using System.Net.Sockets;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace RabbitMQDemo.Publisher
{
    public class RabbitMqConnectionHelper
    {
        private Policy _waitAndRetryPolicy;
        private IConnectionFactory _connectionFactory;
        public IConnection Connection { get; private set; }
        public bool IsConnected => Connection != null && Connection.IsOpen;

        public RabbitMqConnectionHelper(int retryCount)
        {
            _waitAndRetryPolicy = Policy
                .Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    Console.WriteLine($"Could not connect to RabbitMQ. Retrying in {time} seconds. Error: ({ex.Message})");
                });


            _connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
            };
        }
        public bool TryConnect()
        {
            _waitAndRetryPolicy.Execute(() =>
            {
                Console.WriteLine("Initializing connection.");
                Connection = _connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                Connection.ConnectionShutdown += OnConnectionShutdown;
                Connection.ConnectionBlocked += OnConnectionBlocked;
                Connection.CallbackException += OnCallbackException;

                return true;
            }

            return false;
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs eventArgs)
        {
            Console.WriteLine($"An CallbackException occurred in RabbitMQ connection. Trying to re-connect...");
            TryConnect();
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs eventArgs)
        {
            Console.WriteLine($"The connection to RabbitMQ has been blocked. Reason: {eventArgs.Reason}. Trying to re-connect...");
            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            Console.WriteLine($"The connection to RabbitMQ has been shutdown. Cause: {eventArgs.Cause.ToString()}. Initiator: {eventArgs.Initiator.ToString()}");
            TryConnect();
        }
    }
}