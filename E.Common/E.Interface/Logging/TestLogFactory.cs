using System;

namespace E.Interface.Logging
{
    /// <inheritdoc />
    /// <summary>
    /// Creates a test Logger, that stores all log messages in a member list
    /// </summary>
	public class TestLogFactory : ILogFactory
    {
        private readonly bool _debugEnabled;

        public TestLogFactory(bool debugEnabled = true)
        {
            _debugEnabled = debugEnabled;
        }

        public ILog GetLogger(Type type)
        {
            return new TestLogger(type) { IsDebugEnabled = _debugEnabled };
        }

        public ILog GetLogger(string typeName)
        {
            return new TestLogger(typeName) { IsDebugEnabled = _debugEnabled };
        }
    }
}
