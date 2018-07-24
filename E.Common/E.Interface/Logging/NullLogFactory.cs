using System;

namespace E.Interface.Logging
{
    /// <inheritdoc />
    /// <summary>
    /// Creates a Debug Logger, that logs all messages to: System.Diagnostics.Debug
    /// Made public so its testable
    /// </summary>
	public class NullLogFactory : ILogFactory
    {
        private readonly bool _debugEnabled;

        public NullLogFactory(bool debugEnabled=false)
        {
            this._debugEnabled = debugEnabled;
        }

        public ILog GetLogger(Type type)
        {
			return new NullDebugLogger(type) { IsDebugEnabled = _debugEnabled };
        }

        public ILog GetLogger(string typeName)
        {
            return new NullDebugLogger(typeName) { IsDebugEnabled = _debugEnabled };
        }
    }
}
