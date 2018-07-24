using System;
using System.Net;

namespace E.Http
{
    internal class CustomRequest: IDisposable
    {
        internal HttpWebRequest WebRequest { get; set; }

        public void Dispose()
        {
            try
            {
                WebRequest?.Abort();
                WebRequest = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}