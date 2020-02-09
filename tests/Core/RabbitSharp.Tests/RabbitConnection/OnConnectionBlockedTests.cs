using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace SharpRabbit.Tests.RabbitConnection
{
    public class OnConnectionBlockedTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SharpRabbit.RabbitConnection> _logger;

        public OnConnectionBlockedTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = _testOutputHelper.BuildLoggerFor<SharpRabbit.RabbitConnection>();
        }

        [Fact]
        public void OnConnectionBlocked_NoMatterTheReason_TryReconnect()
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();

            connectionFactoryMock
                .Setup(c => c.CreateConnection())
                .Returns(connectionMock.Object);

            var connection = new SharpRabbit.RabbitConnection(
                    _logger,
                    connectionFactoryMock.Object);

            var blockReason = It.IsAny<string>();

            connection.OnConnectionBlocked(new object(), new RabbitMQ.Client.Events.ConnectionBlockedEventArgs(blockReason));

            connectionFactoryMock.Verify(m => m.CreateConnection(), Times.Once);
        }
    }
}
