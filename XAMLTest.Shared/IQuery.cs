namespace XamlTest;

public interface IQuery
{
    string QueryString { get; }
}

public interface IQuery<T> : IQuery
{
    void Add(string queryPart);
}