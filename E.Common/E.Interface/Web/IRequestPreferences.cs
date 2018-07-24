namespace E.Interface.Web
{
    public interface IRequestPreferences
    {
        bool AcceptsGzip { get; }

        bool AcceptsDeflate { get; }
    }
}