namespace XamlTest;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GenerateHelpersAttribute : Attribute
{
    public Type ControlType { get; set; }

    public string? Namespace { get; set; }

    public GenerateHelpersAttribute(Type controlType)
    {
        ControlType = controlType;
    }
}
