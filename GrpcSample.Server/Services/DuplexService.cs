using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duplex;
using Grpc.Core;
using GrpcSample.Server.Models;
using Microsoft.Extensions.Logging;

namespace GrpcSample.Server
{
    public class DuplexService : Messaging.MessagingBase
    {
        private readonly ILogger _logger;
        private readonly ServerGrpcSubscribers _serverGrpcSubscribers;

        public DuplexService(ILoggerFactory loggerFactory, ServerGrpcSubscribers serverGrpcSubscribers)
        {
            _logger = loggerFactory.CreateLogger<DuplexService>();
            _serverGrpcSubscribers = serverGrpcSubscribers;
        }

        public override async Task SendData(
            IAsyncStreamReader<MyMessage> requestStream, 
            IServerStreamWriter<MyMessage> responseStream, 
            ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            _logger.LogInformation($"Connection id: {httpContext.Connection.Id}");

            if (!await requestStream.MoveNext())
            {
                return;
            }

            var user = requestStream.Current.Name;
            _logger.LogInformation($"{user} connected");
            var subscriber = new SubscribersModel
            {
                Subscriber = responseStream,
                Name = user
            };

            _serverGrpcSubscribers.AddSubscriber(subscriber);

            do
            {
                await _serverGrpcSubscribers.BroadcastMessageAsync(requestStream.Current);
            } while (await requestStream.MoveNext());

            _serverGrpcSubscribers.RemoveSubscriber(subscriber);
            _logger.LogInformation($"{user} disconnected");
        }

        public void Dispose()
        {
            _logger.LogInformation("Cleaning up");
        }
    }
}
