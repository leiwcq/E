namespace E.Interface
{
    public interface IResponseStatus
    {
        string ErrorCode { get; set; }

        string ErrorMessage { get; set; }

        string StackTrace { get; set; }

        bool IsSuccess { get; }
    }
}