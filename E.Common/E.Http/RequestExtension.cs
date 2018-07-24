using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using E.Serializer;

namespace E.Http
{
    public static class RequestExtension
    {
        static RequestExtension()
        {
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public static async Task PostGetNon(this string requestString, string url, string method = "POST",
            Encoding encoding = null, int timeoutSeconds = 20,
            WebProxy proxy = null, string contentType = "application/x-www-form-urlencoded",
            string requestAccept = "*/*")
        {
            await PostGetAsync(requestString, url, method, encoding, timeoutSeconds, proxy, contentType,
                requestAccept, false);
        }

        public static async Task PostGetNonAsync(this string requestString, string url, string method = "POST",
            Encoding encoding = null, int timeoutSeconds = 20,
            WebProxy proxy = null, string contentType = "application/x-www-form-urlencoded",
            string requestAccept = "*/*")
        {
            await PostGetAsync(requestString, url, method, encoding, timeoutSeconds, proxy, contentType, requestAccept,
                false);
        }

        public static async Task<string> PostGet(this string requestString, string url, string method = "POST",
            Encoding encoding = null, int timeoutSeconds = 20,
            WebProxy proxy = null, string contentType = "application/x-www-form-urlencoded",
            string requestAccept = "*/*")
        {

            return await PostGetAsync(requestString, url, method, encoding, timeoutSeconds, proxy, contentType,
                requestAccept);
        }

        public static async Task<string> PostGetAsync(this string requestString, string url, string method = "POST",
            Encoding encoding = null, int timeoutSeconds = 20,
            WebProxy proxy = null, string contentType = "application/x-www-form-urlencoded",
            string requestAccept = "*/*", bool isReturn = true)
        {
            GC.Collect();
            if (encoding == null)
                encoding = Encoding.UTF8;
            try
            {
                using (var communicat = new CustomRequest {WebRequest = (HttpWebRequest) WebRequest.Create(url)})
                {
                    var request = communicat.WebRequest;
                    if (request == null) return string.Empty;

                    if (proxy != null) request.Proxy = proxy;
                    if (timeoutSeconds > 0)
                    {
                        request.Timeout = 0x3e8 * timeoutSeconds;
                        request.ReadWriteTimeout = 0x3e8 * 5;
                    }

                    request.Method = method;
                    request.ContentType = contentType;
                    //request.AllowAutoRedirect = false;
                    request.Accept = requestAccept ?? "*/*";
                    request.Headers.Add("Accept-Language", "zh-cn");
                    request.Headers.Add("Accept-Encoding", "gzip, deflate");
                    request.Headers.Add("Pragma", "no-cache");
                    request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                    //request.Host = new Uri(url).Host;
                    request.KeepAlive = false;
                    //设置
                    //request.ServicePoint.Expect100Continue = false;
                    //request.ServicePoint.UseNagleAlgorithm = false;
                    //request.ServicePoint.ConnectionLimit = 512;
                    request.AllowWriteStreamBuffering = false;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    if (ServicePointManager.DefaultConnectionLimit <= 512)
                        ServicePointManager.DefaultConnectionLimit = 1024;
                    //--

                    if (method.Equals("post", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var bytes = encoding.GetBytes(requestString);
                        request.ContentLength = bytes.Length;
                        using (var requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(bytes, 0, bytes.Length);
                            requestStream.Close();
                        }
                    }

                    ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                    var response = (HttpWebResponse) await request.GetResponseAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (!isReturn)
                            return string.Empty;

                        var responseStream = response.GetResponseStream();

                        if (responseStream != null)
                        {
                            Stream stream;
                            if (!string.IsNullOrWhiteSpace(response.ContentEncoding) &&
                                response.ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase))
                            {
                                stream = new GZipStream(responseStream, CompressionMode.Decompress);
                            }
                            else
                            {
                                stream = responseStream;
                            }
                            //var stream = response.ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase)
                            //    ? new GZipStream(responseStream, CompressionMode.Decompress)
                            //    : responseStream;
                            var reader = new StreamReader(stream, encoding);
                            var responseData = reader.ReadToEnd();
                            stream.Dispose();
                            reader.Dispose();
                            responseStream.Dispose();
                            response.Dispose();

                            return responseData;
                        }
                    }

                    response.Dispose();

                    throw new WebException(
                        $"StatusCode:{response.StatusCode} StatusDescription:{response.StatusDescription}");
                }

                //return response.StatusDescription;
            }
            catch (WebException e)
            {
                //Trace.WriteLine("This program is expected to throw WebException on successful run." +
                //                                   "\n\nException Message :" + e.Message);
                if (e?.Status == WebExceptionStatus.ProtocolError)
                {
                    var x = e.Response as HttpWebResponse;
                    var responseStream = x?.GetResponseStream();
                    var error = e.Message;
                    if (responseStream != null)
                    {
                        var reader = new StreamReader(responseStream, encoding);
                        var errorMessage = reader.ReadToEnd();
                        error += errorMessage;
                    }
                    else
                    {
                        error += $"发生错误{WebExceptionStatus.ProtocolError}";
                    }
                    return error;
                }
                return e.ToString();

                //throw;
            }
            catch (Exception e)
            {
                return e.ToString();
                //throw;
            }
            return string.Empty;
        }

        public static async Task<T> PostGet<T>(this string requestString, string url, string method = "POST",
            Encoding encoding = null, int timeoutSeconds = 20,
            WebProxy proxy = null, string contentType = "application/x-www-form-urlencoded",
            string requestAccept = "*/*")
        {

            var result = ResultFormat<T>(await PostGetAsync(requestString, url, method, encoding, timeoutSeconds, proxy,
                contentType, requestAccept));
            return result == null ? default(T) : result;
        }

        public static async Task<T> PostGetAsync<T>(this string requestString, string url, string method = "POST",
            Encoding encoding = null, int timeoutSeconds = 20,
            WebProxy proxy = null, string contentType = "application/x-www-form-urlencoded",
            string requestAccept = "*/*")
        {
            var result = ResultFormat<T>(await PostGetAsync(requestString, url, method, encoding, timeoutSeconds,
                proxy, contentType, requestAccept));
            return result;
        }

        private static T ResultFormat<T>(string responseData) 
        {
            if (!string.IsNullOrEmpty(responseData) && responseData.Length > 10)
            {
                var result = responseData.FromJson<T>();
                return result;
            }
            return default(T);
        }

        #region SSL/TLS设置

        /// <summary>
        /// 解决Error"基础连接已经关闭: 未能为SSL/TLS 安全通道建立信任关系。"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        #endregion
    }
}