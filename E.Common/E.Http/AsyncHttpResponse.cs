using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace E.Http
{
    public class AsyncHttpResponse : IDisposable
    {
        protected byte[] Data { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        /// <summary>
        /// 过期属性
        /// </summary>
        //[Obsolete]
        public AsyncCookieContainer Cookies { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Encoding Encoding { get; private set; }
        public Exception Exception { get; private set; }

        internal AsyncHttpResponse(HttpResponseMessage rsp, string encoding)
        {
            StatusCode = rsp.StatusCode;
            Encoding = Encoding.GetEncoding(encoding ?? "UTF-8");
            Headers = new Dictionary<string, string>();
            Cookies = null;
            Exception = null;
            Init(rsp);
        }

        internal AsyncHttpResponse(Exception exp, string encoding)
        {
            StatusCode = 0;
            Encoding = Encoding.GetEncoding(encoding ?? "UTF-8");
            Headers = new Dictionary<string, string>();
            Cookies = null;
            Exception = exp;
        }

        protected async void Init(HttpResponseMessage rsp)
        {
            if (rsp.StatusCode == HttpStatusCode.OK)
            {
                Data = await rsp.Content.ReadAsByteArrayAsync();
            }
            if (rsp.Headers != null)
            {
                foreach (var kv in rsp.Headers)
                {
                    if ("Set-Cookie".Equals(kv.Key))
                    {
                        if (Cookies == null)
                        {
                            Cookies = new AsyncCookieContainer();
                        }
                        foreach (var cookieStr in kv.Value)
                        {
                            Cookies.Add(cookieStr);
                        }
                    }
                    Headers[kv.Key] = string.Join(";", kv.Value);
                }
            }
            StatusCode = rsp.StatusCode;
            RequestMessage = rsp.RequestMessage;
        }

        public HttpRequestMessage RequestMessage { get; private set; }

        //public async Task<IRandomAccessStream> GetRandomStream()
        //{
        //    if (Data == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        var buffer = this.GetBuffer();
        //        InMemoryRandomAccessStream inStream = new InMemoryRandomAccessStream();
        //        DataWriter datawriter = new DataWriter(inStream.GetOutputStreamAt(0));
        //        datawriter.WriteBuffer(buffer, 0, buffer.Length);
        //        await datawriter.StoreAsync();
        //        return inStream;
        //    }

        //}

        //public IBuffer GetBuffer()
        //{
        //    if (Data == null)
        //        return null;
        //    else
        //        return WindowsRuntimeBufferExtensions.AsBuffer(Data);
        //}

        public byte[] GetBytes()
        {
            return Data;
        }

        public string GetString()
        {
            if (Data == null)
            {
                return null;
            }
            return Encoding.GetString(Data);
        }

        public void Dispose()
        {
            Data = null;
            Headers = null;
            Cookies = null;
            Encoding = null;
            Exception = null;
        }
    }
}