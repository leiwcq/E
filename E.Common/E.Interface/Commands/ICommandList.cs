using System.Collections.Generic;

namespace E.Interface.Commands
{
    public interface ICommandList<T> : ICommand<List<T>> {}
}