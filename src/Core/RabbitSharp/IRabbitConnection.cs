using RabbitMQ.Client;
using System;

namespace SharpRabbit
{
    public interface IRabbitConnection : IDisposable
    {
        void TryConnect();
        IModel CreateModel();
    }
}