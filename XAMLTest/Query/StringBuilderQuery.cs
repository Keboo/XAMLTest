using System.Text;

namespace XamlTest.Query;

internal class StringBuilderQuery<T> : IQuery<T>
{
    private StringBuilder Builder { get; } = new();
    public string QueryString => Builder.ToString();
    public void Add(string queryPart) => Builder.Append(queryPart);

    public StringBuilderQuery()
    { }

    public StringBuilderQuery(string query)
        => Builder.Append(query);
}