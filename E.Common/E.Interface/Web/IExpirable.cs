using System;

namespace E.Interface.Web
{
    public interface IExpirable
    {
        DateTime? LastModified { get; }
    }
}