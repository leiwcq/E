namespace E.Interface.Web
{
    public interface IHttpError : IHttpResult
    {
        string Message { get; }
        string ErrorCode { get; }
        string StackTrace { get; }
    }
}