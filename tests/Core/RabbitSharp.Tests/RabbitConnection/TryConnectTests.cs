using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using Xunit;
using Xunit.Abstractions;

namespace SharpRabbit.Tests.RabbitConnection
{
    public class TryConnectTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SharpRabbit.RabbitConnection> _logger;

        public TryConnectTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = _testOutputHelper.BuildLoggerFor<SharpRabbit.RabbitConnection>();
        }

        [Fact]
        public void TryConnect_NoExceptionsToHandle_CallCreatesConnectionOnce()
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

            connection.TryConnect();

            connectionFactoryMock.Verify(m => m.CreateConnection(), Times.Once);
        }

        [Fact]
        public void TryConnect_LessThanFourExceptionsToHandle_CallCreatesConnectionAtLeastOnce()
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();

            var callingTimes = 0;
            connectionFactoryMock
                .Setup(c => c.CreateConnection())
                .Callback(() =>
                {
                    callingTimes++;
                })
                .Returns(() =>
                {
                    if (callingTimes < 4)
                        throw new BrokerUnreachableException(new Exception());

                    connectionMock
                        .Setup(c => c.IsOpen)
                        .Returns(true);

                    return connectionMock.Object;
                });

            var connection = new SharpRabbit.RabbitConnection(
                    _logger,
                    connectionFactoryMock.Object);

            var blockReason = It.IsAny<string>();

            connection.TryConnect();

            Assert.True(connection.IsConnected);
        }

        [Fact]
        public void TryConnect_MoreThanThreeExceptionsToHandle_ConnectionUnsuccessful()
        {
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();

            connectionFactoryMock
                .Setup(c => c.CreateConnection())
                .Callback(() =>
                {
                    throw new BrokerUnreachableException(new Exception());
                });

            var connection = new SharpRabbit.RabbitConnection(
                    _logger,
                    connectionFactoryMock.Object);

            Assert.False(connection.IsConnected);
        }
    }
}
