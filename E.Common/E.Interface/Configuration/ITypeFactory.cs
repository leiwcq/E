using System;

namespace E.Interface.Configuration
{
    public interface ITypeFactory
    {
        object CreateInstance(IResolver resolver, Type type);
    }
}