using System.IO;

namespace E.Interface.IO
{
    public interface IVirtualFile : IVirtualNode
    {
        IVirtualPathProvider VirtualPathProvider { get; }

        string Extension { get; }

        string GetFileHash();

        Stream OpenRead();
        StreamReader OpenText();
        string ReadAllText();

        long Length { get; }

        /// <summary>
        /// Refresh file stats for this node if supported
        /// </summary>
        void Refresh();
    }
}
