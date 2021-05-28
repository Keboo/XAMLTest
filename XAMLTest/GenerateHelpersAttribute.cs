using System;
using System.Windows.Controls;
using XamlTest;

[assembly: GenerateHelpers(typeof(Button))]

namespace XamlTest
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class GenerateHelpersAttribute : Attribute
    {
        public Type ControlType { get; set; }
        public GenerateHelpersAttribute(Type controlType)
        {
            ControlType = controlType;
        }
    }
}
