using System;
using System.Windows;

namespace XamlTest
{
    public interface IWindow : IVisualElement<Window>, IEquatable<IWindow>
    {
        
    }
}
