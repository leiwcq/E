using System.Collections.Generic;

namespace E.Interface.Web
{
    public interface IHasOptions
    {
        IDictionary<string, string> Options { get; }
    }
}