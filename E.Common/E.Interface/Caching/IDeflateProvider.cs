using System.IO;

namespace E.Interface.Caching
{
    public interface IDeflateProvider
    {
        byte[] Deflate(string text);
        byte[] Deflate(byte[] bytes);

        string Inflate(byte[] gzBuffer);
        byte[] InflateBytes(byte[] gzBuffer);

        Stream DeflateStream(Stream outputStream);
        Stream InflateStream(Stream inputStream);
    }
}
