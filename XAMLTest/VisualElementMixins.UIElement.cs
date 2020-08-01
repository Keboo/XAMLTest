using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    partial class VisualElementMixins
    {
        public static async Task<bool> GetAllowDrop(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.AllowDrop));

        public static async Task<bool> GetClipToBounds(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.ClipToBounds));

        public static async Task<Size> GetDesiredSize(this IVisualElement element)
            => await element.GetProperty<Size>(nameof(UIElement.DesiredSize));

        public static async Task<bool> GetFocusable(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(UIElement.Focusable));

        public static async Task<bool> SetAllowDrop(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.AllowDrop), value);

        public static async Task<bool> SetClipToBounds(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.ClipToBounds), value);

        public static async Task<bool> SetFocusable(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(UIElement.Focusable), value);
    }
}
