using RabbitMQ.Client;
using System;

namespace MateusViegas.Net.RabbitMQ.Core
{
    public interface IRabbitConnection : IDisposable
    {
        bool TryConnect();
        IModel CreateModel();
    }
}