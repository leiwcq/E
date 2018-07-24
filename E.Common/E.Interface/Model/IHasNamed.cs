namespace E.Interface.Model
{
    public interface IHasNamed<T>
    {
        T this[string listId] { get; set; }
    }
}