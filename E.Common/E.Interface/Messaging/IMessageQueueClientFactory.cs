using System;

namespace E.Interface.Messaging
{
    public interface IMessageQueueClientFactory
        : IDisposable
    {
        IMessageQueueClient CreateMessageQueueClient();
    }
}