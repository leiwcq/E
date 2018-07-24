using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using E.Serializer;

namespace E.Http
{
    public static class AsyncHttp
    {
        static AsyncHttp()
        {
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static readonly CookieContainer _cookieContainer = new CookieContainer();

        public static CookieContainer CookieContainer => _cookieContainer;

        public static string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";

        public static AsyncHttpClient Create(string referer = null)
        {
            var client = new AsyncHttpClient();
            client
                .Header("Connection", "keep-alive")
                .Header("Keep-Alive", "600")
                .UserAgent(UserAgent)
                .Header("Accept-Encoding", "gzip,deflate,br")
                .Header("Accept-Language", "zh-CN,zh;q=0.8,en;q=0.6,zh-TW;q=0.4")
                .AutomaticDecompression(DecompressionMethods.Deflate | DecompressionMethods.GZip)
                .AllowAutoRedirect(false)
                .Cookies(_cookieContainer)
                .ExpectContinue(false);//.Timeout(35000)
            if (!string.IsNullOrWhiteSpace(referer))
            {
                client.Referer(referer);
                client.Header("Origin", referer);
            }
            return client;
        }

        public static string GetString(string url)
        {
            var result = Retry(() =>
            {
                var client = Create();
                client.Accept("application/json, text/plain, */*");
                var response = client.Url(url).Get().Result;
                var str = response.GetString();
                return str;
            });
            return result;
        }

        public static string GetStringOnce(string url)
        {
            string result;
            var stream = GetResponseStream("GET", url);
            using (stream)
            {
                var sr = new StreamReader(stream);
                result = sr.ReadToEnd();
            }
            return result;
        }

        public static string PostMutipart(string url, List<FormDataItem> dataList)
        {
            string result;
            var boundary = "----WebKitFormBoundary" + Guid.NewGuid().ToString("N").Substring(0, 16);
            var stream = Retry(() =>
            {
                if (!(WebRequest.Create(url) is HttpWebRequest request)) return null;
                request.Method = "POST";
                request.Accept = "*/*";
                request.KeepAlive = true;
                request = PretendWechat(request);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                var sw = new StreamWriter(request.GetRequestStream());
                foreach (var item in dataList)
                {
                    if (item.IsFile)
                    {
                        sw.Write(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; {2}=\"{3}\"\r\nContent-Type: {4}\r\n\r\n", boundary, item.Name, item.Name, item.FileName, MimeMapping.GetMimeMapping(item.FileName)));
                        sw.Flush();
                        sw.BaseStream.Write(item.Content, 0, item.ContentLength);
                        sw.Write("\r\n");
                    }
                    else
                    {
                        sw.Write(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", boundary, item.Name, item.Value));
                    }
                    sw.Flush();
                }
                sw.Write("--" + boundary + "--\r\n");
                sw.Flush();
                sw.Close();
                var response = request.GetResponse() as HttpWebResponse;
                return response?.GetResponseStream();
            });
            using (stream)
            {
                var sr = new StreamReader(stream);
                result = sr.ReadToEnd();
            }
            return result;
        }

        public static string PostFormString(string url, string body)
        {
            string result;
            var stream = Retry(() => GetResponseStream("POST", url, "application/x-www-form-urlencoded", body));
            using (stream)
            {
                var sr = new StreamReader(stream);
                result = sr.ReadToEnd();
            }
            return result;
        }

        public static T PostJson<T>(string url, object body)
        {
            string result;
            var stream = Retry(() => GetResponseStream("POST", url, "application/json;charset=UTF-8", body.ToJsonWithFront()));
            using (stream)
            {
                var sr = new StreamReader(stream);
                result = sr.ReadToEnd();
            }
            return result.FromJson<T>();
        }


        public static Stream GetResponseStream(string method, string url, string contentType = null, object body = null)
        {
            if (!(WebRequest.Create(url) is HttpWebRequest request)) return null;
            request.Method = method;
            request = PretendWechat(request);
            if (contentType != null)
            {
                request.ContentType = contentType;
            }
            if (body != null)
            {
                var sw = new StreamWriter(request.GetRequestStream());
                sw.Write(body);
                sw.Flush();
                sw.Close();
            }
            var response = request.GetResponse() as HttpWebResponse;
            var stream = response?.GetResponseStream();
            return stream;
        }

        public static byte[] GetImage(string url)
        {
            byte[] result;
            var stream = Retry(() => GetResponseStream("GET", url));
            using (stream)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    result = ms.ToArray();
                }
            }
            return result;
        }

        public static HttpWebRequest PretendWechat(HttpWebRequest request,string referer=null)
        {
            request.Proxy = null;
            request.KeepAlive = true;
            request.UserAgent = UserAgent;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Headers.Add("Accept-Encoding", "gzip,deflate,br");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en;q=0.6,zh-TW;q=0.4");
            request.CookieContainer = _cookieContainer;
            request.AllowAutoRedirect = false;
            request.Timeout = 35000;
            if (!string.IsNullOrWhiteSpace(referer))
            {
                request.Referer = referer;
                request.Headers.Add("Origin", referer);
            }
            return request;
        }


        /// <summary>
        /// 三次重试机制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private static T Retry<T>(Func<T> func)
        {
            var err = 0;
            while (err < 3)
            {
                try
                {
                    return func();
                }
                catch (WebException)
                {
                    err++;
                    Thread.Sleep(5000);
                    if (err > 2)
                    {
                        throw;
                    }
                }
            }
            return func();
        }
    }

    public static class MimeMapping
    {
        private static readonly Dictionary<string, string> _mimeMappingTable;

        private static void AddMimeMapping(string extension, string mimeType)
        {
            _mimeMappingTable.Add(extension, mimeType);
        }

        public static string GetMimeMapping(string fileName)
        {
            string text = null;
            var num = fileName.LastIndexOf('.');
            if (0 < num && num > fileName.LastIndexOf('\\'))
            {
                text = _mimeMappingTable[fileName.Substring(num)];
            }
            return text ?? _mimeMappingTable[".*"];
        }

        static MimeMapping()
        {
            _mimeMappingTable = new Dictionary<string, string>(190, StringComparer.CurrentCultureIgnoreCase);
            AddMimeMapping(".323", "text/h323");
            AddMimeMapping(".asx", "video/x-ms-asf");
            AddMimeMapping(".acx", "application/internet-property-stream");
            AddMimeMapping(".ai", "application/postscript");
            AddMimeMapping(".aif", "audio/x-aiff");
            AddMimeMapping(".aiff", "audio/aiff");
            AddMimeMapping(".axs", "application/olescript");
            AddMimeMapping(".aifc", "audio/aiff");
            AddMimeMapping(".asr", "video/x-ms-asf");
            AddMimeMapping(".avi", "video/x-msvideo");
            AddMimeMapping(".asf", "video/x-ms-asf");
            AddMimeMapping(".au", "audio/basic");
            AddMimeMapping(".application", "application/x-ms-application");
            AddMimeMapping(".bin", "application/octet-stream");
            AddMimeMapping(".bas", "text/plain");
            AddMimeMapping(".bcpio", "application/x-bcpio");
            AddMimeMapping(".bmp", "image/bmp");
            AddMimeMapping(".cdf", "application/x-cdf");
            AddMimeMapping(".cat", "application/vndms-pkiseccat");
            AddMimeMapping(".crt", "application/x-x509-ca-cert");
            AddMimeMapping(".c", "text/plain");
            AddMimeMapping(".css", "text/css");
            AddMimeMapping(".cer", "application/x-x509-ca-cert");
            AddMimeMapping(".crl", "application/pkix-crl");
            AddMimeMapping(".cmx", "image/x-cmx");
            AddMimeMapping(".csh", "application/x-csh");
            AddMimeMapping(".cod", "image/cis-cod");
            AddMimeMapping(".cpio", "application/x-cpio");
            AddMimeMapping(".clp", "application/x-msclip");
            AddMimeMapping(".crd", "application/x-mscardfile");
            AddMimeMapping(".deploy", "application/octet-stream");
            AddMimeMapping(".dll", "application/x-msdownload");
            AddMimeMapping(".dot", "application/msword");
            AddMimeMapping(".doc", "application/msword");
            AddMimeMapping(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            AddMimeMapping(".dvi", "application/x-dvi");
            AddMimeMapping(".dir", "application/x-director");
            AddMimeMapping(".dxr", "application/x-director");
            AddMimeMapping(".der", "application/x-x509-ca-cert");
            AddMimeMapping(".dib", "image/bmp");
            AddMimeMapping(".dcr", "application/x-director");
            AddMimeMapping(".disco", "text/xml");
            AddMimeMapping(".exe", "application/octet-stream");
            AddMimeMapping(".etx", "text/x-setext");
            AddMimeMapping(".evy", "application/envoy");
            AddMimeMapping(".eml", "message/rfc822");
            AddMimeMapping(".eps", "application/postscript");
            AddMimeMapping(".flr", "x-world/x-vrml");
            AddMimeMapping(".fif", "application/fractals");
            AddMimeMapping(".gtar", "application/x-gtar");
            AddMimeMapping(".gif", "image/gif");
            AddMimeMapping(".gz", "application/x-gzip");
            AddMimeMapping(".hta", "application/hta");
            AddMimeMapping(".htc", "text/x-component");
            AddMimeMapping(".htt", "text/webviewhtml");
            AddMimeMapping(".h", "text/plain");
            AddMimeMapping(".hdf", "application/x-hdf");
            AddMimeMapping(".hlp", "application/winhlp");
            AddMimeMapping(".html", "text/html");
            AddMimeMapping(".htm", "text/html");
            AddMimeMapping(".hqx", "application/mac-binhex40");
            AddMimeMapping(".isp", "application/x-internet-signup");
            AddMimeMapping(".iii", "application/x-iphone");
            AddMimeMapping(".ief", "image/ief");
            AddMimeMapping(".ivf", "video/x-ivf");
            AddMimeMapping(".ins", "application/x-internet-signup");
            AddMimeMapping(".ico", "image/x-icon");
            AddMimeMapping(".jpg", "image/jpeg");
            AddMimeMapping(".jfif", "image/pjpeg");
            AddMimeMapping(".jpe", "image/jpeg");
            AddMimeMapping(".jpeg", "image/jpeg");
            AddMimeMapping(".js", "application/x-javascript");
            AddMimeMapping(".lsx", "video/x-la-asf");
            AddMimeMapping(".latex", "application/x-latex");
            AddMimeMapping(".lsf", "video/x-la-asf");
            AddMimeMapping(".manifest", "application/x-ms-manifest");
            AddMimeMapping(".mhtml", "message/rfc822");
            AddMimeMapping(".mny", "application/x-msmoney");
            AddMimeMapping(".mht", "message/rfc822");
            AddMimeMapping(".mid", "audio/mid");
            AddMimeMapping(".mpv2", "video/mpeg");
            AddMimeMapping(".man", "application/x-troff-man");
            AddMimeMapping(".mvb", "application/x-msmediaview");
            AddMimeMapping(".mpeg", "video/mpeg");
            AddMimeMapping(".m3u", "audio/x-mpegurl");
            AddMimeMapping(".mdb", "application/x-msaccess");
            AddMimeMapping(".mpp", "application/vnd.ms-project");
            AddMimeMapping(".m1v", "video/mpeg");
            AddMimeMapping(".mpa", "video/mpeg");
            AddMimeMapping(".me", "application/x-troff-me");
            AddMimeMapping(".m13", "application/x-msmediaview");
            AddMimeMapping(".movie", "video/x-sgi-movie");
            AddMimeMapping(".m14", "application/x-msmediaview");
            AddMimeMapping(".mpe", "video/mpeg");
            AddMimeMapping(".mp2", "video/mpeg");
            AddMimeMapping(".mov", "video/quicktime");
            AddMimeMapping(".mp3", "audio/mpeg");
            AddMimeMapping(".mpg", "video/mpeg");
            AddMimeMapping(".ms", "application/x-troff-ms");
            AddMimeMapping(".nc", "application/x-netcdf");
            AddMimeMapping(".nws", "message/rfc822");
            AddMimeMapping(".oda", "application/oda");
            AddMimeMapping(".ods", "application/oleobject");
            AddMimeMapping(".pmc", "application/x-perfmon");
            AddMimeMapping(".p7r", "application/x-pkcs7-certreqresp");
            AddMimeMapping(".p7b", "application/x-pkcs7-certificates");
            AddMimeMapping(".p7s", "application/pkcs7-signature");
            AddMimeMapping(".pmw", "application/x-perfmon");
            AddMimeMapping(".ps", "application/postscript");
            AddMimeMapping(".p7c", "application/pkcs7-mime");
            AddMimeMapping(".pbm", "image/x-portable-bitmap");
            AddMimeMapping(".ppm", "image/x-portable-pixmap");
            AddMimeMapping(".pub", "application/x-mspublisher");
            AddMimeMapping(".pnm", "image/x-portable-anymap");
            AddMimeMapping(".png", "image/png");
            AddMimeMapping(".pml", "application/x-perfmon");
            AddMimeMapping(".p10", "application/pkcs10");
            AddMimeMapping(".pfx", "application/x-pkcs12");
            AddMimeMapping(".p12", "application/x-pkcs12");
            AddMimeMapping(".pdf", "application/pdf");
            AddMimeMapping(".pps", "application/vnd.ms-powerpoint");
            AddMimeMapping(".p7m", "application/pkcs7-mime");
            AddMimeMapping(".pko", "application/vndms-pkipko");
            AddMimeMapping(".ppt", "application/vnd.ms-powerpoint");
            AddMimeMapping(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            AddMimeMapping(".pmr", "application/x-perfmon");
            AddMimeMapping(".pma", "application/x-perfmon");
            AddMimeMapping(".pot", "application/vnd.ms-powerpoint");
            AddMimeMapping(".prf", "application/pics-rules");
            AddMimeMapping(".pgm", "image/x-portable-graymap");
            AddMimeMapping(".qt", "video/quicktime");
            AddMimeMapping(".ra", "audio/x-pn-realaudio");
            AddMimeMapping(".rgb", "image/x-rgb");
            AddMimeMapping(".ram", "audio/x-pn-realaudio");
            AddMimeMapping(".rmi", "audio/mid");
            AddMimeMapping(".ras", "image/x-cmu-raster");
            AddMimeMapping(".roff", "application/x-troff");
            AddMimeMapping(".rtf", "application/rtf");
            AddMimeMapping(".rtx", "text/richtext");
            AddMimeMapping(".sv4crc", "application/x-sv4crc");
            AddMimeMapping(".spc", "application/x-pkcs7-certificates");
            AddMimeMapping(".setreg", "application/set-registration-initiation");
            AddMimeMapping(".snd", "audio/basic");
            AddMimeMapping(".stl", "application/vndms-pkistl");
            AddMimeMapping(".setpay", "application/set-payment-initiation");
            AddMimeMapping(".stm", "text/html");
            AddMimeMapping(".shar", "application/x-shar");
            AddMimeMapping(".sh", "application/x-sh");
            AddMimeMapping(".sit", "application/x-stuffit");
            AddMimeMapping(".spl", "application/futuresplash");
            AddMimeMapping(".sct", "text/scriptlet");
            AddMimeMapping(".scd", "application/x-msschedule");
            AddMimeMapping(".sst", "application/vndms-pkicertstore");
            AddMimeMapping(".src", "application/x-wais-source");
            AddMimeMapping(".sv4cpio", "application/x-sv4cpio");
            AddMimeMapping(".tex", "application/x-tex");
            AddMimeMapping(".tgz", "application/x-compressed");
            AddMimeMapping(".t", "application/x-troff");
            AddMimeMapping(".tar", "application/x-tar");
            AddMimeMapping(".tr", "application/x-troff");
            AddMimeMapping(".tif", "image/tiff");
            AddMimeMapping(".txt", "text/plain");
            AddMimeMapping(".texinfo", "application/x-texinfo");
            AddMimeMapping(".trm", "application/x-msterminal");
            AddMimeMapping(".tiff", "image/tiff");
            AddMimeMapping(".tcl", "application/x-tcl");
            AddMimeMapping(".texi", "application/x-texinfo");
            AddMimeMapping(".tsv", "text/tab-separated-values");
            AddMimeMapping(".ustar", "application/x-ustar");
            AddMimeMapping(".uls", "text/iuls");
            AddMimeMapping(".vcf", "text/x-vcard");
            AddMimeMapping(".wps", "application/vnd.ms-works");
            AddMimeMapping(".wav", "audio/wav");
            AddMimeMapping(".wrz", "x-world/x-vrml");
            AddMimeMapping(".wri", "application/x-mswrite");
            AddMimeMapping(".wks", "application/vnd.ms-works");
            AddMimeMapping(".wmf", "application/x-msmetafile");
            AddMimeMapping(".wcm", "application/vnd.ms-works");
            AddMimeMapping(".wrl", "x-world/x-vrml");
            AddMimeMapping(".wdb", "application/vnd.ms-works");
            AddMimeMapping(".wsdl", "text/xml");
            AddMimeMapping(".xap", "application/x-silverlight-app");
            AddMimeMapping(".xml", "text/xml");
            AddMimeMapping(".xlm", "application/vnd.ms-excel");
            AddMimeMapping(".xaf", "x-world/x-vrml");
            AddMimeMapping(".xla", "application/vnd.ms-excel");
            AddMimeMapping(".xls", "application/vnd.ms-excel");
            AddMimeMapping(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            AddMimeMapping(".xof", "x-world/x-vrml");
            AddMimeMapping(".xlt", "application/vnd.ms-excel");
            AddMimeMapping(".xlc", "application/vnd.ms-excel");
            AddMimeMapping(".xsl", "text/xml");
            AddMimeMapping(".xbm", "image/x-xbitmap");
            AddMimeMapping(".xlw", "application/vnd.ms-excel");
            AddMimeMapping(".xpm", "image/x-xpixmap");
            AddMimeMapping(".xwd", "image/x-xwindowdump");
            AddMimeMapping(".xsd", "text/xml");
            AddMimeMapping(".z", "application/x-compress");
            AddMimeMapping(".zip", "application/x-zip-compressed");
            AddMimeMapping(".*", "application/octet-stream");
        }
    }
}
