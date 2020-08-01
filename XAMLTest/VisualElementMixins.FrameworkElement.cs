using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace XamlTest
{
    partial class VisualElementMixins
    {
        public static async Task<double> GetActualHeight(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.ActualHeight));

        public static async Task<double> GetActualWidth(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.ActualWidth));

        public static async Task<Cursor> GetCursor(this IVisualElement element)
            => await element.GetProperty<Cursor>(nameof(FrameworkElement.Cursor));

        public static async Task<object> GetDataContext(this IVisualElement element)
            => await element.GetProperty<object>(nameof(FrameworkElement.DataContext));

        public static async Task<FlowDirection> GetFlowDirection(this IVisualElement element)
            => await element.GetProperty<FlowDirection>(nameof(FrameworkElement.FlowDirection));

        public static async Task<Style> GetFocusVisualStyle(this IVisualElement element)
            => await element.GetProperty<Style>(nameof(FrameworkElement.FocusVisualStyle));

        public static async Task<bool> GetForceCursor(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(FrameworkElement.ForceCursor));

        public static async Task<double> GetHeight(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.Height));

        public static async Task<HorizontalAlignment> GetHorizontalAlignment(this IVisualElement element)
            => await element.GetProperty<HorizontalAlignment>(nameof(FrameworkElement.HorizontalAlignment));

        public static async Task<InputScope> GetInputScope(this IVisualElement element)
            => await element.GetProperty<InputScope>(nameof(FrameworkElement.InputScope));

        public static async Task<bool> GetIsInitialized(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(FrameworkElement.IsInitialized));

        public static async Task<bool> GetIsLoaded(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(FrameworkElement.IsLoaded));

        public static async Task<XmlLanguage> GetLanguage(this IVisualElement element)
            => await element.GetProperty<XmlLanguage>(nameof(FrameworkElement.Language));

        public static async Task<Transform> GetLayoutTransform(this IVisualElement element)
            => await element.GetProperty<Transform>(nameof(FrameworkElement.LayoutTransform));

        public static async Task<Thickness> GetMargin(this IVisualElement element)
            => await element.GetProperty<Thickness>(nameof(FrameworkElement.Margin));

        public static async Task<double> GetMaxHeight(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.MaxHeight));

        public static async Task<double> GetMaxWidth(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.MaxWidth));

        public static async Task<double> GetMinHeight(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.MinHeight));

        public static async Task<double> GetMinWidth(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.MinWidth));

        public static async Task<string> GetName(this IVisualElement element)
            => await element.GetProperty<string>(nameof(FrameworkElement.Name));

        public static async Task<bool> GetOverridesDefaultStyle(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(FrameworkElement.OverridesDefaultStyle));

        public static async Task<DependencyObject> GetParent(this IVisualElement element)
            => await element.GetProperty<DependencyObject>(nameof(FrameworkElement.Parent));

        public static async Task<ResourceDictionary> GetResources(this IVisualElement element)
            => await element.GetProperty<ResourceDictionary>(nameof(FrameworkElement.Resources));

        public static async Task<Style> GetStyle(this IVisualElement element)
            => await element.GetProperty<Style>(nameof(FrameworkElement.Style));

        public static async Task<object> GetTag(this IVisualElement element)
            => await element.GetProperty<object>(nameof(FrameworkElement.Tag));

        public static async Task<DependencyObject> GetTemplatedParent(this IVisualElement element)
            => await element.GetProperty<DependencyObject>(nameof(FrameworkElement.TemplatedParent));

        public static async Task<object> GetToolTip(this IVisualElement element)
            => await element.GetProperty<object>(nameof(FrameworkElement.ToolTip));

        public static async Task<TriggerCollection> GetTriggers(this IVisualElement element)
            => await element.GetProperty<TriggerCollection>(nameof(FrameworkElement.Triggers));

        public static async Task<bool> GetUseLayoutRounding(this IVisualElement element)
            => await element.GetProperty<bool>(nameof(FrameworkElement.UseLayoutRounding));

        public static async Task<VerticalAlignment> GetVerticalAlignment(this IVisualElement element)
            => await element.GetProperty<VerticalAlignment>(nameof(FrameworkElement.VerticalAlignment));

        public static async Task<double> GetWidth(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.Width));

        public static async Task<Cursor> SetCursor(this IVisualElement element, Cursor value)
            => await element.SetProperty(nameof(FrameworkElement.Cursor), value);

        public static async Task<Object> SetDataContext(this IVisualElement element, Object value)
            => await element.SetProperty(nameof(FrameworkElement.DataContext), value);

        public static async Task<FlowDirection> SetFlowDirection(this IVisualElement element, FlowDirection value)
            => await element.SetProperty(nameof(FrameworkElement.FlowDirection), value);

        public static async Task<Style> SetFocusVisualStyle(this IVisualElement element, Style value)
            => await element.SetProperty(nameof(FrameworkElement.FocusVisualStyle), value);

        public static async Task<bool> SetForceCursor(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(FrameworkElement.ForceCursor), value);

        public static async Task<double> SetHeight(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.Height), value);

        public static async Task<HorizontalAlignment> SetHorizontalAlignment(this IVisualElement element, HorizontalAlignment value)
            => await element.SetProperty(nameof(FrameworkElement.HorizontalAlignment), value);

        public static async Task<InputScope> SetInputScope(this IVisualElement element, InputScope value)
            => await element.SetProperty(nameof(FrameworkElement.InputScope), value);

        public static async Task<XmlLanguage> SetLanguage(this IVisualElement element, XmlLanguage value)
            => await element.SetProperty(nameof(FrameworkElement.Language), value);

        public static async Task<Transform> SetLayoutTransform(this IVisualElement element, Transform value)
            => await element.SetProperty(nameof(FrameworkElement.LayoutTransform), value);

        public static async Task<Thickness> SetMargin(this IVisualElement element, Thickness value)
            => await element.SetProperty(nameof(FrameworkElement.Margin), value);

        public static async Task<double> SetMaxHeight(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.MaxHeight), value);

        public static async Task<double> SetMaxWidth(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.MaxWidth), value);

        public static async Task<double> SetMinHeight(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.MinHeight), value);

        public static async Task<double> SetMinWidth(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.MinWidth), value);

        public static async Task<string> SetName(this IVisualElement element, string value)
            => await element.SetProperty<string>(nameof(FrameworkElement.Name), value);

        public static async Task<bool> SetOverridesDefaultStyle(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(FrameworkElement.OverridesDefaultStyle), value);

        public static async Task<ResourceDictionary> SetResources(this IVisualElement element, ResourceDictionary value)
            => await element.SetProperty(nameof(FrameworkElement.Resources), value);

        public static async Task<Style> SetStyle(this IVisualElement element, Style value)
            => await element.SetProperty(nameof(FrameworkElement.Style), value);

        public static async Task<object> SetTag(this IVisualElement element, Object value)
            => await element.SetProperty(nameof(FrameworkElement.Tag), value);

        public static async Task<object> SetToolTip(this IVisualElement element, Object value)
            => await element.SetProperty(nameof(FrameworkElement.ToolTip), value);

        public static async Task<bool> SetUseLayoutRounding(this IVisualElement element, bool value)
            => await element.SetProperty(nameof(FrameworkElement.UseLayoutRounding), value);

        public static async Task<VerticalAlignment> SetVerticalAlignment(this IVisualElement element, VerticalAlignment value)
            => await element.SetProperty(nameof(FrameworkElement.VerticalAlignment), value);

        public static async Task<double> SetWidth(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.Width), value);
    }
}
