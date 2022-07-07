using XamlTest.Host;

namespace XamlTest.Internal;

internal class Window : VisualElement<NativeWindow>, IWindow
{
    public Window(Protocol.ProtocolClient client, string id, 
        AppContext context, Action<string>? logMessage)
        : base(client, id, typeof(NativeWindow), context, logMessage)
    { }

    public bool Equals([AllowNull] IWindow other)
        => base.Equals(other);
    protected override Host.ElementQuery GetFindElementQuery(string query)
        => new Host.ElementQuery
        {
            WindowId = Id,
            Query = query
        };
}
