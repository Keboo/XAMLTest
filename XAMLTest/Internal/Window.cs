using System;
using System.Diagnostics.CodeAnalysis;
using XamlTest.Host;

namespace XamlTest.Internal;

internal class Window : VisualElement<System.Windows.Window>, IWindow
{
    public Window(Protocol.ProtocolClient client, string id, 
        AppContext context, Action<string>? logMessage)
        : base(client, id, typeof(System.Windows.Window), context, logMessage)
    { }

    public bool Equals([AllowNull] IWindow other)
        => base.Equals(other);
    protected override Host.ElementQuery GetFindElementQuery(string query)
        => new Host.ElementQuery
        {
            WindowId = Id,
            Query = query
        };

    public async Task<VisualTreeNodeInfo> GetVisualTree()
    {
        LogMessage?.Invoke($"{nameof(GetVisualTree)}()");
        GetVisualTreeQuery request = new()
        {
            WindowId = Id
        };
        if (await Client.GetVisualTreeAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (reply.Root is { } root)
            {
                return MapNode(root);
            }
            throw new XamlTestException("Visual tree result did not contain a root node");
        }
        throw new XamlTestException("Failed to receive a reply");
    }

    private static VisualTreeNodeInfo MapNode(VisualTreeNode node)
    {
        return new VisualTreeNodeInfo
        {
            Type = node.Type,
            Name = node.Name,
            Children = node.Children.Select(MapNode).ToList()
        };
    }
}
