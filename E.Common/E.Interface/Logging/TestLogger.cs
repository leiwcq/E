using System;
using System.Collections.Generic;

namespace E.Interface.Logging 
{
    /// <inheritdoc />
    /// <summary>
    /// Tests logger which  stores all log messages in a member list which can be examined later
    /// Made public so its testable
    /// </summary>
    public class TestLogger : ILog {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogger"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TestLogger(string type) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogger"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TestLogger(Type type) {
        }

        public enum Levels {
            Debug,
            Error,
            Fatal,
            Info,
            Warn,
        };

        private static readonly List<KeyValuePair<Levels, string>> Logs = new List<KeyValuePair<Levels, string>>();


        public static IList<KeyValuePair<Levels, string>> GetLogs() { return Logs; }

        public bool IsDebugEnabled { get; set; }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        private static void Log(Levels level, object message, Exception exception) {
            string msg = message == null ? string.Empty : message.ToString();
            if(exception != null) {
                msg += ", Exception: " + exception.Message;
            }
            Logs.Add(new KeyValuePair<Levels, string>(level, msg));
        }

        /// <summary>
        /// Logs the format.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        private static void LogFormat(Levels level, object message, params object[] args) {
            string msg = message == null ? string.Empty : message.ToString();
            Logs.Add(new KeyValuePair<Levels, string>(level, string.Format(msg, args)));
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message">The message.</param>
        private static void Log(Levels level, object message) {
            string msg = message == null ? string.Empty : message.ToString();
            Logs.Add(new KeyValuePair<Levels, string>(level, msg));
        }

        public void Debug(object message, Exception exception) {
            Log(Levels.Debug, message, exception);
        }

        public void Debug(object message) {
            Log(Levels.Debug, message);
        }

        public void DebugFormat(string format, params object[] args) {
            LogFormat(Levels.Debug, format, args);
        }

        public void Error(object message, Exception exception) {
            Log(Levels.Error, message, exception);
        }

        public void Error(object message) {
            Log(Levels.Error, message);
        }

        public void ErrorFormat(string format, params object[] args) {
            LogFormat(Levels.Error, format, args);
        }

        public void Fatal(object message, Exception exception) {
            Log(Levels.Fatal, message, exception);
        }

        public void Fatal(object message) {
            Log(Levels.Fatal, message);
        }

        public void FatalFormat(string format, params object[] args) {
            LogFormat(Levels.Fatal, format, args);
        }

        public void Info(object message, Exception exception) {
            Log(Levels.Info, message, exception);
        }

        public void Info(object message) {
            Log(Levels.Info, message);
        }

        public void InfoFormat(string format, params object[] args) {
            LogFormat(Levels.Info, format, args);
        }

        public void Warn(object message, Exception exception) {
            Log(Levels.Warn, message, exception);
        }

        public void Warn(object message) {
            Log(Levels.Warn, message);
        }

        public void WarnFormat(string format, params object[] args) {
            LogFormat(Levels.Warn, format, args);
        }
    }
}
