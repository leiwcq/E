using System;

namespace E.Interface.Messaging
{
    public interface IMessageProducer
        : IDisposable
    {
        void Publish<T>(T messageBody);
        void Publish<T>(IMessage<T> message);
    }

}
