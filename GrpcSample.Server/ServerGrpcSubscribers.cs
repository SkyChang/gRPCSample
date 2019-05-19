﻿using Duplex;
using GrpcSample.Server.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcSample.Server
{
    public class ServerGrpcSubscribers
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SubscribersModel> Subscribers = new ConcurrentDictionary<string, SubscribersModel>();

        public ServerGrpcSubscribers(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServerGrpcSubscribers>();
        }

        public async Task BroadcastMessageAsync(MyMessage message)
        {
            await BroadcastMessages(message);
        }


        public void AddSubscriber(SubscribersModel subscriber)
        {
            bool added = Subscribers.TryAdd(subscriber.Name, subscriber);
            _logger.LogInformation($"New subscriber added: {subscriber.Name}");
            if (!added)
            {
                _logger.LogInformation($"could not add subscriber: {subscriber.Name}");
            }
        }

        public void RemoveSubscriber(SubscribersModel subscriber)
        {
            try
            {
                Subscribers.TryRemove(subscriber.Name, out SubscribersModel item);
                _logger.LogInformation($"Force Remove: {item.Name} - no longer works");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not remove {subscriber.Name}");
            }
        }

        private async Task BroadcastMessages(MyMessage message)
        {
            foreach (var subscriber in Subscribers.Values)
            {
                var item = await SendMessageToSubscriber(subscriber, message);
                if (item != null)
                {
                    RemoveSubscriber(item);
                };
            }
        }

        private async Task<SubscribersModel> SendMessageToSubscriber(SubscribersModel subscriber, MyMessage message)
        {
            try
            {
                _logger.LogInformation($"Broadcasting: {message.Name} - {message.Message}");
                await subscriber.Subscriber.WriteAsync(message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send");
                return subscriber;
            }
        }
    }
}
