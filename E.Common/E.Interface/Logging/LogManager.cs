using System;

namespace E.Interface.Logging
{
    /// <summary>
    /// Logging API for this library. You can inject your own implementation otherwise
    /// will use the DebugLogFactory to write to System.Diagnostics.Debug
    /// </summary>
    public class LogManager
    {
        private static ILogFactory _logFactory;

        /// <summary>
        /// Gets or sets the log factory.
        /// Use this to override the factory that is used to create loggers
        /// </summary>
        public static ILogFactory LogFactory
        {
            get => _logFactory ?? new NullLogFactory();
            set => _logFactory = value;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public static ILog GetLogger(Type type)
        {
            return LogFactory.GetLogger(type);
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public static ILog GetLogger(string typeName)
        {
            return LogFactory.GetLogger(typeName);
        }
    }
}
