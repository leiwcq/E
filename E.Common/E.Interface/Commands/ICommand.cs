namespace E.Interface.Commands
{
    public interface ICommand<ReturnType>
    {
        ReturnType Execute();
    }
}