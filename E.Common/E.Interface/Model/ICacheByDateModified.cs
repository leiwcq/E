using System;

namespace E.Interface.Model
{
    public interface ICacheByDateModified
    {
        DateTime? LastModified { get; }
    }
}