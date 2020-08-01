using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    partial class VisualElementMixins
    {
        public static async Task<double> GetActualHeight(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.ActualHeight));

        public static async Task<double> GetActualWidth(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.ActualWidth));

        public static async Task<double> GetHeight(this IVisualElement element)
            => await element.GetProperty<double>(nameof(FrameworkElement.Height));

        public static async Task<HorizontalAlignment> GetHorizontalAlignment(this IVisualElement element)
            => await element.GetProperty<HorizontalAlignment>(nameof(FrameworkElement.HorizontalAlignment));


        public static async Task<double> SetHeight(this IVisualElement element, double value)
            => await element.SetProperty(nameof(FrameworkElement.Height), value);

        public static async Task<HorizontalAlignment> SetHorizontalAlignment(this IVisualElement element, HorizontalAlignment value)
            => await element.SetProperty(nameof(FrameworkElement.HorizontalAlignment), value);


    }
}
