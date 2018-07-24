using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace E.Interface.Web
{
    [Obsolete("Use IStreamWriterAsync")]
    public interface IStreamWriter
    {
        void WriteTo(Stream responseStream);
    }

    public interface IStreamWriterAsync
    {
        Task WriteToAsync(Stream responseStream, CancellationToken token = default(CancellationToken));
    }
}