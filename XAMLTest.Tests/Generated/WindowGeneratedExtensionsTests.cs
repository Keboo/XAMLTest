using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTest.Tests.Generated
{
    partial class WindowGeneratedExtensionsTests
    {
        static partial void OnClassInitialize()
        {
            GetWindowContent = x => "";
            GetElement = _ => Task.FromResult<IVisualElement<Window>>(Window);
        }
    }
}
