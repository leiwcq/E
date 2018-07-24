namespace E.Interface
{
    public interface IEncryptedClient : IReplyClient, IHasSessionId, IHasVersion
    {
        string ServerPublicKeyXml { get; }
        IJsonServiceClient Client { get; }

        TResponse Send<TResponse>(string httpMethod, object request);
        TResponse Send<TResponse>(string httpMethod, IReturn<TResponse> request);
    }
}