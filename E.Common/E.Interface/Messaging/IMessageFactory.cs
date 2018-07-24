namespace E.Interface.Messaging
{
    public interface IMessageFactory : IMessageQueueClientFactory
    {
        IMessageProducer CreateMessageProducer();
    }
}