﻿using System;
using System.Text;

namespace E.Interface.Logging
{
    /// <inheritdoc />
    /// <summary>
    /// Helper ILog implementation that reduces effort to extend or use without needing to impl each API
    /// </summary>
    public class GenericLogger : ILog
    {
        private const string DEBUG = "DEBUG: ";
        private const string ERROR = "ERROR: ";
        private const string FATAL = "FATAL: ";
        private const string INFO = "INFO: ";
        private const string WARN = "WARN: ";

        public Action<string> OnMessage;

        public bool CaptureLogs { get; set; }

        public StringBuilder Logs = new StringBuilder();

        public virtual void OnLog(string message)
        {
            if (CaptureLogs)
                Logs.AppendLine(message);

            OnMessage?.Invoke(message);
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="E.Common.Logging.DebugLogger" /> class.
        /// </summary>
        public GenericLogger(Type type) : this(type.Name) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="E.Common.Logging.DebugLogger"/> class.
        /// </summary>
        public GenericLogger(string type)
        {
            IsDebugEnabled = true;
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        public virtual void Log(object message, Exception exception)
        {
            string msg = message == null ? string.Empty : message.ToString();
            if (exception != null)
            {
                msg += ", Exception: " + exception.Message;
            }
            OnLog(msg);
        }

        /// <summary>
        /// Logs the format.
        /// </summary>
        public virtual void LogFormat(object message, params object[] args)
        {
            string msg = message == null ? string.Empty : message.ToString();
            OnLog(string.Format(msg, args));
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        public virtual void Log(object message)
        {
            string msg = message == null ? string.Empty : message.ToString();
            OnLog(msg);
        }

        public void Debug(object message, Exception exception)
        {
            Log(DEBUG + message, exception);
        }

        public bool IsDebugEnabled { get; set; }

        public void Debug(object message)
        {
            Log(DEBUG + message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            LogFormat(DEBUG + format, args);
        }

        public void Error(object message, Exception exception)
        {
            Log(ERROR + message, exception);
        }

        public void Error(object message)
        {
            Log(ERROR + message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            LogFormat(ERROR + format, args);
        }

        public void Fatal(object message, Exception exception)
        {
            Log(FATAL + message, exception);
        }

        public void Fatal(object message)
        {
            Log(FATAL + message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            LogFormat(FATAL + format, args);
        }

        public void Info(object message, Exception exception)
        {
            Log(INFO + message, exception);
        }

        public void Info(object message)
        {
            Log(INFO + message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            LogFormat(INFO + format, args);
        }

        public void Warn(object message, Exception exception)
        {
            Log(WARN + message, exception);
        }

        public void Warn(object message)
        {
            Log(WARN + message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            LogFormat(WARN + format, args);
        }
    }
}