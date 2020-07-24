using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace XamlTest
{
    public interface IVisualElement : IEquatable<IVisualElement>
    {
        Task<IVisualElement> GetElement(string query);
        
        Task<IValue> GetProperty(string name);
        Task<IValue> SetProperty(string name, string value, string? valueType = null);

        Task<IResource> GetResource(string key);

        Task<Color> GetEffectiveBackground();
        Task<Rect> GetCoordinates();

        Task<IImage> GetBitmap();
    }
}
