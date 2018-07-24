using System;

namespace E.Interface.Messaging
{
    /// <summary>
    /// Util static generic class to create unique queue names for types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class QueueNames<T>
    {
        static QueueNames() 
        {
            Priority = QueueNames.ResolveQueueNameFn(typeof(T).Name, ".priorityq");
            In = QueueNames.ResolveQueueNameFn(typeof(T).Name, ".inq");
            Out = QueueNames.ResolveQueueNameFn(typeof(T).Name, ".outq");
            Dlq = QueueNames.ResolveQueueNameFn(typeof(T).Name, ".dlq");
        }

        public static string Priority { get; }

        public static string In { get; }

        public static string Out { get; }

        public static string Dlq { get; }

        public static string[] AllQueueNames => new[] {
            In,
            Priority,
            Out,
            Dlq,
        };
    }

    /// <summary>
    /// Util class to create unique queue names for runtime types
    /// </summary>
    public class QueueNames
    {
        public static string Exchange = "mx.E";
        public static string ExchangeDlq = "mx.E.dlq";
        public static string ExchangeTopic = "mx.E.topic";

        public static string MqPrefix = "mq:";
        public static string QueuePrefix = "";

        public static string TempMqPrefix = MqPrefix + "tmp:";
        public static string TopicIn = MqPrefix + "topic:in";
        public static string TopicOut = MqPrefix + "topic:out";

        public static Func<string, string, string> ResolveQueueNameFn = ResolveQueueName;

        public static string ResolveQueueName(string typeName, string queueSuffix)
        {
            return QueuePrefix + MqPrefix + typeName + queueSuffix;
        }

        public static bool IsTempQueue(string queueName)
        {
            return queueName != null 
                && queueName.StartsWith(TempMqPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static void SetQueuePrefix(string prefix)
        {
            TopicIn = prefix + MqPrefix + "topic:in";
            TopicOut = prefix + MqPrefix + "topic:out";
            QueuePrefix = prefix;
            TempMqPrefix = prefix + MqPrefix + "tmp:";
        }

        private readonly Type _messageType;

        public QueueNames(Type messageType)
        {
            _messageType = messageType;
        }

        public string Priority => ResolveQueueNameFn(_messageType.Name, ".priorityq");

        public string In => ResolveQueueNameFn(_messageType.Name, ".inq");

        public string Out => ResolveQueueNameFn(_messageType.Name, ".outq");

        public string Dlq => ResolveQueueNameFn(_messageType.Name, ".dlq");

        public static string GetTempQueueName()
        {
            return TempMqPrefix + Guid.NewGuid().ToString("n");
        }
    }

}