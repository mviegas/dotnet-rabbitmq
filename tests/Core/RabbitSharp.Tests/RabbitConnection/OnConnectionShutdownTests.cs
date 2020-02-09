using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace SharpRabbit.Tests.RabbitConnection
{
    public class OnConnectionShutdownTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SharpRabbit.RabbitConnection> _logger;

        public OnConnectionShutdownTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = _testOutputHelper.BuildLoggerFor<SharpRabbit.RabbitConnection>();
        }

        [Fact]
        public void OnConnectionShutdown_InitiatorIsApplication_DoNotReconnect()
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();

            connectionFactoryMock
                .Setup(c => c.CreateConnection())
                .Returns(connectionMock.Object);

            var connection = new SharpRabbit.RabbitConnection(
                    _logger,
                    connectionFactoryMock.Object);

            var sender = It.IsAny<object>();
            var replyText = It.IsAny<string>();
            var replyCode = It.IsAny<ushort>();

            connection.OnConnectionShutdown(sender, new ShutdownEventArgs(ShutdownInitiator.Application, replyCode, replyText));

            connectionFactoryMock.Verify(m => m.CreateConnection(), Times.Never);
        }

        [Fact]
        public void OnConnectionShutdown_InitiatorIsPeer_Reconnect()
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();

            connectionFactoryMock
                .Setup(c => c.CreateConnection())
                .Returns(connectionMock.Object);

            var connection = new SharpRabbit.RabbitConnection(
                    _logger,
                    connectionFactoryMock.Object);

            var sender = It.IsAny<object>();
            var replyText = It.IsAny<string>();
            var replyCode = It.IsAny<ushort>();

            connection.OnConnectionShutdown(sender, new ShutdownEventArgs(ShutdownInitiator.Peer, replyCode, replyText));

            connectionFactoryMock.Verify(m => m.CreateConnection(), Times.Once);
        }
    }
}
