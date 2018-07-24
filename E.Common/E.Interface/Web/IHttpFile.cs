using System.IO;

namespace E.Interface.Web
{
    public interface IHttpFile
    {
        string Name { get; }
        string FileName { get; }
        long ContentLength { get; }
        string ContentType { get; }
        Stream InputStream { get; }
    }
}