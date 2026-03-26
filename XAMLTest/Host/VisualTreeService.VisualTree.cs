using Grpc.Core;
using System.Windows.Media;
using Window = System.Windows.Window;

namespace XamlTest.Host;

internal partial class VisualTreeService
{
    public override async Task<GetVisualTreeResult> GetVisualTree(GetVisualTreeQuery request, ServerCallContext context)
    {
        GetVisualTreeResult reply = new();

        await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                Window? window = GetCachedElement<Window>(request.WindowId);
                if (window is null)
                {
                    reply.ErrorMessages.Add("Failed to find window");
                    return;
                }

                reply.Root = BuildVisualTreeNode(window);
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
        });

        return reply;
    }

    private static VisualTreeNode BuildVisualTreeNode(DependencyObject element)
    {
        VisualTreeNode node = new()
        {
            Type = element.GetType().Name,
            Name = (element as FrameworkElement)?.Name ?? ""
        };

        foreach (DependencyObject child in GetVisualChildren(element))
        {
            node.Children.Add(BuildVisualTreeNode(child));
        }

        return node;
    }

    private static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent)
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            if (VisualTreeHelper.GetChild(parent, i) is DependencyObject child)
            {
                yield return child;
            }
        }

        if (childCount == 0)
        {
            foreach (object? logicalChild in LogicalTreeHelper.GetChildren(parent))
            {
                if (logicalChild is DependencyObject child)
                {
                    yield return child;
                }
            }
        }
    }
}
