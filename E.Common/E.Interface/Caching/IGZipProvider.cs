using System.IO;

namespace E.Interface.Caching
{
    public interface IGZipProvider
    {
        byte[] GZip(string text);
        byte[] GZip(byte[] bytes);

        string GUnzip(byte[] gzBuffer);
        byte[] GUnzipBytes(byte[] gzBuffer);

        Stream GZipStream(Stream outputStream);
        Stream GUnzipStream(Stream gzStream);
    }
}