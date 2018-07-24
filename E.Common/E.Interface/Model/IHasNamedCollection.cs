using System.Collections.Generic;

namespace E.Interface.Model
{
    public interface IHasNamedCollection<T> : IHasNamed<ICollection<T>>
    {
    }
}