using RabbitMQ.Client;
using System;

namespace SharpRabbit
{
    public interface IRabbitConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();
    }
}