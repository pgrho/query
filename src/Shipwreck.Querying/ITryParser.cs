namespace Shipwreck.Querying
{
    public interface ITryParser<T>
    {
        bool TryParse(string s, out T result);
    }
}