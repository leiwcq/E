using System.Collections.Generic;

namespace E.Interface
{
    public interface IOneWayClient
    {
        void SendOneWay(object requestDto);

        void SendOneWay(string relativeOrAbsoluteUri, object requestDto);

        void SendAllOneWay(IEnumerable<object> requests);
    }
}