using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace E.Common
{
    public abstract class PclExport
    {
        public static class Platforms
        {
            public const string Uwp = "UWP";
            public const string Android = "Android";
            public const string IOS = "IOS";
            public const string Mac = "MAC";
            public const string NetStandard = "NETStandard";
        }

        public static PclExport Instance= new NetStandardPclExport();

        static PclExport() { }

        public static bool ConfigureProvider(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                return false;

            var mi = type.GetMethod("Configure");
            if (mi != null)
            {
                mi.Invoke(null, new object[0]);
            }

            return true;
        }

        public static void Configure(PclExport instance)
        {
            Instance = instance ?? Instance;

            if (Instance != null && Instance.EmptyTask == null)
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                Instance.EmptyTask = tcs.Task;
            }
        }

        public Task EmptyTask;

        public bool SupportsExpression;

        public bool SupportsEmit;

        public char DirSep = '\\';

        public char AltDirSep = '/';

        public static readonly char[] DirSeps = { '\\', '/' };

        public string PlatformName = "Unknown";

        public RegexOptions RegexOptions = RegexOptions.None;

        public StringComparison InvariantComparison = StringComparison.Ordinal;

        public StringComparison InvariantComparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;

        public StringComparer InvariantComparer = StringComparer.Ordinal;

        public StringComparer InvariantComparerIgnoreCase = StringComparer.OrdinalIgnoreCase;

        public abstract string ReadAllText(string filePath);

        // HACK: The only way to detect anonymous types right now.
        public virtual bool IsAnonymousType(Type type)
        {
            return type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>", StringComparison.Ordinal) || type.Name.StartsWith("VB$", StringComparison.Ordinal));
        }

        public virtual string ToInvariantUpper(char value)
        {
            return value.ToString().ToUpperInvariant();
        }

        public virtual bool FileExists(string filePath)
        {
            return false;
        }

        public virtual bool DirectoryExists(string dirPath)
        {
            return false;
        }

        public virtual void CreateDirectory(string dirPath)
        {
        }

        public virtual string GetEnvironmentVariable(string name)
        {
            return null;
        }

        public virtual string[] GetFileNames(string dirPath, string searchPattern = null)
        {
            return TypeConstants.EmptyStringArray;
        }

        public virtual string[] GetDirectoryNames(string dirPath, string searchPattern = null)
        {
            return TypeConstants.EmptyStringArray;
        }

        public virtual void WriteLine(string line)
        {
        }

        public virtual void WriteLine(string line, params object[] args)
        {
        }

        public virtual HttpWebRequest CreateWebRequest(string requestUri, bool? emulateHttpViaPost = null)
        {
            return (HttpWebRequest)WebRequest.Create(requestUri);
        }

        public virtual void Config(HttpWebRequest req,
            bool? allowAutoRedirect = null,
            TimeSpan? timeout = null,
            TimeSpan? readWriteTimeout = null,
            string userAgent = null,
            bool? preAuthenticate = null)
        {
        }

        public virtual void AddCompression(WebRequest webRequest)
        {
        }

        public virtual Stream GetRequestStream(WebRequest webRequest)
        {
            var async = webRequest.GetRequestStreamAsync();
            async.Wait();
            return async.Result;
        }

        public virtual WebResponse GetResponse(WebRequest webRequest)
        {
            try
            {
                var async = webRequest.GetResponseAsync();
                async.Wait();
                return async.Result;
            }
            catch (Exception ex)
            {
                throw ex.UnwrapIfSingleException();
            }
        }

        public virtual bool IsDebugBuild(Assembly assembly)
        {
            return assembly.AllAttributes()
                .Any(x => x.GetType().Name == "DebuggableAttribute");
        }

        public virtual string MapAbsolutePath(string relativePath, string appendPartialPathModifier)
        {
            return relativePath;
        }

        public virtual Assembly LoadAssembly(string assemblyPath)
        {
            return null;
        }

        public virtual void AddHeader(WebRequest webReq, string name, string value)
        {
            webReq.Headers[name] = value;
        }

        public virtual void SetUserAgent(HttpWebRequest httpReq, string value)
        {
            httpReq.Headers[HttpRequestHeader.UserAgent] = value;
        }

        public virtual void SetContentLength(HttpWebRequest httpReq, long value)
        {
            httpReq.Headers[HttpRequestHeader.ContentLength] = value.ToString();
        }

        public virtual void SetAllowAutoRedirect(HttpWebRequest httpReq, bool value)
        {
        }

        public virtual void SetKeepAlive(HttpWebRequest httpReq, bool value)
        {
        }

        public virtual Assembly[] GetAllAssemblies()
        {
            return new Assembly[0];
        }

        public virtual Type FindType(string typeName, string assemblyName)
        {
            return null;
        }

        public virtual string GetAssemblyCodeBase(Assembly assembly)
        {
            return assembly.FullName;
        }

        public virtual string GetAssemblyPath(Type source)
        {
            return null;
        }

        public virtual string GetAsciiString(byte[] bytes)
        {
            return GetAsciiString(bytes, 0, bytes.Length);
        }

        public virtual string GetAsciiString(byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        public virtual byte[] GetAsciiBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public virtual Encoding GetUTF8Encoding(bool emitBom = false)
        {
            return new UTF8Encoding(emitBom);
        }

        

        

        public virtual bool InSameAssembly(Type t1, Type t2)
        {
            return t1.AssemblyQualifiedName != null && t1.AssemblyQualifiedName.Equals(t2.AssemblyQualifiedName);
        }

        public virtual Type GetGenericCollectionType(Type type)
        {
            return type.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        


        public virtual void InitHttpWebRequest(HttpWebRequest httpReq,
            long? contentLength = null, bool allowAutoRedirect = true, bool keepAlive = true)
        {
        }

        public virtual void CloseStream(Stream stream)
        {
            stream.Flush();
        }

        public virtual void ResetStream(Stream stream)
        {
            stream.Position = 0;
        }

        public virtual void BeginThreadAffinity()
        {
        }

        public virtual void EndThreadAffinity()
        {
        }

        public virtual DataContractAttribute GetWeakDataContract(Type type)
        {
            return null;
        }

        public virtual DataMemberAttribute GetWeakDataMember(PropertyInfo pi)
        {
            return null;
        }

        public virtual DataMemberAttribute GetWeakDataMember(FieldInfo pi)
        {
            return null;
        }

        

        public virtual string GetStackTrace()
        {
            return null;
        }

        public virtual Task WriteAndFlushAsync(Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            return EmptyTask;
        }
    }
}
