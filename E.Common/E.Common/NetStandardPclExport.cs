using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace E.Common
{
    public class NetStandardPclExport : PclExport
    {
        public static NetStandardPclExport Provider = new NetStandardPclExport();

        static readonly Action<HttpWebRequest, string> SetUserAgentDelegate =
            (Action<HttpWebRequest, string>) typeof(HttpWebRequest)
                .GetProperty("UserAgent")
                ?.GetSetMethod(true)?.CreateDelegate(typeof(Action<HttpWebRequest, string>));

        static readonly Action<HttpWebRequest, bool> SetAllowAutoRedirectDelegate =
            (Action<HttpWebRequest, bool>) typeof(HttpWebRequest)
                .GetProperty("AllowAutoRedirect")
                ?.GetSetMethod(true)?.CreateDelegate(typeof(Action<HttpWebRequest, bool>));

        static readonly Action<HttpWebRequest, bool> SetKeepAliveDelegate =
            (Action<HttpWebRequest, bool>) typeof(HttpWebRequest)
                .GetProperty("KeepAlive")
                ?.GetSetMethod(true)?.CreateDelegate(typeof(Action<HttpWebRequest, bool>));

        static readonly Action<HttpWebRequest, long> SetContentLengthDelegate =
            (Action<HttpWebRequest, long>) typeof(HttpWebRequest)
                .GetProperty("ContentLength")
                ?.GetSetMethod(true)?.CreateDelegate(typeof(Action<HttpWebRequest, long>));

        private readonly bool _allowToChangeRestrictedHeaders;

        public NetStandardPclExport()
        {
            PlatformName = Platforms.NetStandard;
            DirSep = Path.DirectorySeparatorChar;
            var req = HttpWebRequest.Create("http://e.net");
            try
            {
                req.Headers[HttpRequestHeader.UserAgent] = "E";
                _allowToChangeRestrictedHeaders = true;
            }
            catch (ArgumentException)
            {
                _allowToChangeRestrictedHeaders = false;
            }
        }

        public override string ReadAllText(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return reader.ReadToEnd();
            }
        }

        public override bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public override bool DirectoryExists(string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        public override void CreateDirectory(string dirPath)
        {
            Directory.CreateDirectory(dirPath);
        }

        public override string[] GetFileNames(string dirPath, string searchPattern = null)
        {
            if (!Directory.Exists(dirPath))
                return TypeConstants.EmptyStringArray;

            return searchPattern != null
                ? Directory.GetFiles(dirPath, searchPattern)
                : Directory.GetFiles(dirPath);
        }

        public override string[] GetDirectoryNames(string dirPath, string searchPattern = null)
        {
            if (!Directory.Exists(dirPath))
                return TypeConstants.EmptyStringArray;

            return searchPattern != null
                ? Directory.GetDirectories(dirPath, searchPattern)
                : Directory.GetDirectories(dirPath);
        }

        public override string MapAbsolutePath(string relativePath, string appendPartialPathModifier)
        {
            if (relativePath.StartsWith("~"))
            {
                var assemblyDirectoryPath = AppContext.BaseDirectory;

                // Escape the assembly bin directory to the hostname directory
                var hostDirectoryPath = appendPartialPathModifier != null
                    ? assemblyDirectoryPath + appendPartialPathModifier
                    : assemblyDirectoryPath;

                return Path.GetFullPath(relativePath.Replace("~", hostDirectoryPath));
            }
            return relativePath;
        }

        public static PclExport Configure()
        {
            Configure(Provider);
            return Provider;
        }

        public override string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name);

        public override void WriteLine(string line) => Console.WriteLine(line);

        public override void WriteLine(string format, params object[] args) => Console.WriteLine(format, args);

        public override void AddCompression(WebRequest webReq)
        {
            try
            {
                var httpReq = (HttpWebRequest) webReq;
                httpReq.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
                httpReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            catch (Exception ex)
            {
                Tracer.Instance.WriteError(ex);
            }
        }

        public override void AddHeader(WebRequest webReq, string name, string value)
        {
            webReq.Headers[name] = value;
        }

        public override Assembly[] GetAllAssemblies()
        {
            return new Assembly[0];
        }

        public override string GetAssemblyCodeBase(Assembly assembly)
        {
            return typeof(PclExport).Assembly.CodeBase;
        }

        public override string GetAssemblyPath(Type source)
        {
            var codeBase = GetAssemblyCodeBase(source.GetTypeInfo().Assembly);
            if (codeBase == null)
                return null;

            var assemblyUri = new Uri(codeBase);
            return assemblyUri.LocalPath;
        }

        public override string GetAsciiString(byte[] bytes, int index, int count)
        {
            return System.Text.Encoding.ASCII.GetString(bytes, index, count);
        }

        public override byte[] GetAsciiBytes(string str)
        {
            return System.Text.Encoding.ASCII.GetBytes(str);
        }

        public override bool InSameAssembly(Type t1, Type t2)
        {
            return t1.Assembly == t2.Assembly;
        }

        public override Type GetGenericCollectionType(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t =>
                t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(ICollection<>));
        }



        public override void SetUserAgent(HttpWebRequest httpReq, string value)
        {
            if (SetUserAgentDelegate != null)
            {
                SetUserAgentDelegate(httpReq, value);
            }
            else
            {
                if (_allowToChangeRestrictedHeaders)
                    httpReq.Headers[HttpRequestHeader.UserAgent] = value;
            }
        }

        public override void SetContentLength(HttpWebRequest httpReq, long value)
        {
            if (SetContentLengthDelegate != null)
            {
                SetContentLengthDelegate(httpReq, value);
            }
            else
            {
                if (_allowToChangeRestrictedHeaders)
                    httpReq.Headers[HttpRequestHeader.ContentLength] = value.ToString();
            }
        }

        public override void SetAllowAutoRedirect(HttpWebRequest httpReq, bool value)
        {
            SetAllowAutoRedirectDelegate?.Invoke(httpReq, value);
        }

        public override void SetKeepAlive(HttpWebRequest httpReq, bool value)
        {
            SetKeepAliveDelegate?.Invoke(httpReq, value);
        }

        public override void InitHttpWebRequest(HttpWebRequest httpReq,
            long? contentLength = null, bool allowAutoRedirect = true, bool keepAlive = true)
        {
            httpReq.UserAgent = Env.ServerUserAgent;
            httpReq.AllowAutoRedirect = allowAutoRedirect;
            httpReq.KeepAlive = keepAlive;

            if (contentLength != null)
            {
                SetContentLength(httpReq, contentLength.Value);
            }
        }

        public override void Config(HttpWebRequest req,
            bool? allowAutoRedirect = null,
            TimeSpan? timeout = null,
            TimeSpan? readWriteTimeout = null,
            string userAgent = null,
            bool? preAuthenticate = null)
        {
            try
            {
                //req.MaximumResponseHeadersLength = int.MaxValue; //throws "The message length limit was exceeded" exception
                if (allowAutoRedirect.HasValue)
                    req.AllowAutoRedirect = allowAutoRedirect.Value;

                if (userAgent != null)
                    req.UserAgent = userAgent;

                if (readWriteTimeout.HasValue) req.ReadWriteTimeout = (int) readWriteTimeout.Value.TotalMilliseconds;
                if (timeout.HasValue) req.Timeout = (int) timeout.Value.TotalMilliseconds;

                if (preAuthenticate.HasValue)
                    req.PreAuthenticate = preAuthenticate.Value;
            }
            catch (Exception ex)
            {
                Tracer.Instance.WriteError(ex);
            }
        }

        public override string GetStackTrace() => Environment.StackTrace;


    }

}
