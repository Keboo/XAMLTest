using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace XamlTest
{
    partial class VisualElementMixins
    {
        public static async Task<bool> GetAllowDrop(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.AllowDrop));

        public static async Task<bool> GetAreAnyTouchesCaptured(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.AreAnyTouchesCaptured));

        public static async Task<bool> GetAreAnyTouchesCapturedWithin(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.AreAnyTouchesCapturedWithin));

        public static async Task<bool> GetAreAnyTouchesDirectlyOver(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.AreAnyTouchesDirectlyOver));

        public static async Task<bool> GetAreAnyTouchesOver(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.AreAnyTouchesOver));

        public static async Task<CacheMode> GetCacheMode(this IVisualElement element)
            => await element.GetProperty<CacheMode>(nameof(UIElement.CacheMode));

        public static async Task<Geometry> GetClip(this IVisualElement element)
            => await element.GetProperty<Geometry>(nameof(UIElement.Clip));

        public static async Task<bool> GetClipToBounds(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.ClipToBounds));

        public static async Task<DependencyObjectType> GetDependencyObjectType(this IVisualElement element)
            => await element.GetProperty<DependencyObjectType>(nameof(UIElement.DependencyObjectType));

        public static async Task<Size> GetDesiredSize(this IVisualElement element)
            => await element.GetProperty<Size>(nameof(UIElement.DesiredSize));

        public static async Task<bool> GetFocusable(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.Focusable));

        public static async Task<bool> GetHasAnimatedProperties(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.HasAnimatedProperties));

        public static async Task<bool> GetIsArrangeValid(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsArrangeValid));

        public static async Task<bool> GetIsEnabled(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsEnabled));

        public static async Task<bool> GetIsFocused(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsFocused));

        public static async Task<bool> GetIsHitTestVisible(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsHitTestVisible));

        public static async Task<bool> GetIsInputMethodEnabled(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsInputMethodEnabled));

        public static async Task<bool> GetIsKeyboardFocused(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsKeyboardFocused));

        public static async Task<bool> GetIsKeyboardFocusWithin(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsKeyboardFocusWithin));

        public static async Task<bool> GetIsManipulationEnabled(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsManipulationEnabled));

        public static async Task<bool> GetIsMeasureValid(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsMeasureValid));

        public static async Task<bool> GetIsMouseCaptured(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsMouseCaptured));

        public static async Task<bool> GetIsMouseCaptureWithin(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsMouseCaptureWithin));

        public static async Task<bool> GetIsMouseDirectlyOver(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsMouseDirectlyOver));

        public static async Task<bool> GetIsMouseOver(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsMouseOver));

        public static async Task<bool> GetIsSealed(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsSealed));

        public static async Task<bool> GetIsStylusCaptured(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsStylusCaptured));

        public static async Task<bool> GetIsStylusCaptureWithin(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsStylusCaptureWithin));

        public static async Task<bool> GetIsStylusDirectlyOver(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsStylusDirectlyOver));

        public static async Task<bool> GetIsStylusOver(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsStylusOver));

        public static async Task<bool> GetIsVisible(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.IsVisible));

        public static async Task<double> GetOpacity(this IVisualElement element)
            => await element.GetProperty<double>(nameof(UIElement.Opacity));

        public static async Task<Brush> GetOpacityMask(this IVisualElement element)
            => await element.GetProperty<Brush>(nameof(UIElement.OpacityMask));

        public static async Task<Size> GetRenderSize(this IVisualElement element)
            => await element.GetProperty<Size>(nameof(UIElement.RenderSize));

        public static async Task<Transform> GetRenderTransform(this IVisualElement element)
            => await element.GetProperty<Transform>(nameof(UIElement.RenderTransform));

        public static async Task<Point> GetRenderTransformOrigin(this IVisualElement element)
            => await element.GetProperty<Point>(nameof(UIElement.RenderTransformOrigin));

        public static async Task<bool> GetSnapsToDevicePixels(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.SnapsToDevicePixels));

        public static async Task<string> GetUid(this IVisualElement element)
            => await element.GetProperty<string>(nameof(UIElement.Uid));

        public static async Task<Visibility> GetVisibility(this IVisualElement element)
            => await element.GetProperty<Visibility>(nameof(UIElement.Visibility));

        public static async Task<bool> SetAllowDrop(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.AllowDrop), value);

        public static async Task<CacheMode> SetCacheMode(this IVisualElement element, CacheMode value)
            => await element.SetProperty(nameof(UIElement.CacheMode), value);

        public static async Task<Geometry> SetClip(this IVisualElement element, Geometry value)
            => await element.SetProperty(nameof(UIElement.Clip), value);

        public static async Task<bool> SetClipToBounds(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.ClipToBounds), value);

        public static async Task<bool> SetFocusable(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.Focusable), value);

        public static async Task<bool> SetIsEnabled(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.IsEnabled), value);

        public static async Task<bool> SetIsHitTestVisible(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.IsHitTestVisible), value);

        public static async Task<bool> SetIsManipulationEnabled(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.IsManipulationEnabled), value);

        public static async Task<double> SetOpacity(this IVisualElement element, double value)
            => await element.SetProperty(nameof(UIElement.Opacity), value);

        public static async Task<Brush> SetOpacityMask(this IVisualElement element, Brush value)
            => await element.SetProperty(nameof(UIElement.OpacityMask), value);

        public static async Task<Size> SetRenderSize(this IVisualElement element, Size value)
            => await element.SetProperty(nameof(UIElement.RenderSize), value);

        public static async Task<Transform> SetRenderTransform(this IVisualElement element, Transform value)
            => await element.SetProperty(nameof(UIElement.RenderTransform), value);

        public static async Task<Point> SetRenderTransformOrigin(this IVisualElement element, Point value)
            => await element.SetProperty(nameof(UIElement.RenderTransformOrigin), value);

        public static async Task<bool> SetSnapsToDevicePixels(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.SnapsToDevicePixels), value);

        public static async Task<Visibility> SetVisibility(this IVisualElement element, Visibility value)
            => await element.SetProperty(nameof(UIElement.Visibility), value);
    }
}
