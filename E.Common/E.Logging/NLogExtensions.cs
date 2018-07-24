using System.Text;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace E.Logging
{
    public static class NLogExtensions
    {
        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            return AddNLog(factory, null);
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddNLog(this ILoggerFactory factory, NLogProviderOptions options)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var provider = new NLogLoggerProvider(options))
            {
                factory.AddProvider(provider);
            }
            return factory;
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory)
        {
            return AddNLog(factory, null);
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, NLogProviderOptions options)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var provider = new NLogLoggerProvider(options))
            {
                factory.AddProvider(provider);
            }
            return factory;
        }
    }
}
