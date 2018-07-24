using System.Collections.Generic;

namespace E.Interface
{
    public interface IMeta
    {
        Dictionary<string, string> Meta { get; set; }
    }

    public interface IHasSessionId
    {
        string SessionId { get; set; }
    }

    public interface IHasVersion
    {
        int Version { get; set; }
    }
}