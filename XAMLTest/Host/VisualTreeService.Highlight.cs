using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using XamlTest.Internal;

namespace XamlTest.Host;

#if WPF
partial class VisualTreeService
{
    public override Task<HighlightResult> HighlightElement(HighlightRequest request, ServerCallContext context)
    {
        HighlightResult reply = new();
        Application.Dispatcher.Invoke(() =>
        {
            DependencyObject? dependencyObject = GetCachedElement<DependencyObject>(request.ElementId);
            if (dependencyObject is null)
            {
                reply.ErrorMessages.Add("Could not find element");
                return;
            }

            if (dependencyObject is not UIElement uiElement)
            {
                reply.ErrorMessages.Add($"Element {dependencyObject.GetType().FullName} is not a {typeof(UIElement).FullName}");
                return;
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);

            if (adornerLayer is null)
            {
                reply.ErrorMessages.Add("Could not find adnorner layer");
                return;
            }

            foreach(var adorner in adornerLayer.GetAdorners(uiElement)?.OfType<SelectionAdorner>().ToList() ?? Enumerable.Empty<SelectionAdorner>())
            {
                adornerLayer.Remove(adorner);
            }

            if (request.IsVisible)
            {
                Brush? borderBrush = Serializer.Deserialize<Brush>(request.BorderBrush);
                Brush? overlayBrush = Serializer.Deserialize<Brush>(request.OverlayBrush);

                var selectionAdorner = new SelectionAdorner(uiElement)
                {
                    AdornerLayer = adornerLayer,
                    BorderBrush = borderBrush,
                    BorderThickness = request.BorderThickness,
                    OverlayBrush = overlayBrush
                };

                adornerLayer.Add(selectionAdorner);
            }
        });
        return Task.FromResult(reply);
    }

}
#endif