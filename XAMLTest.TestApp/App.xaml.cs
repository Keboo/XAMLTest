namespace XAMLTest.TestApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
}

public class OtherApp : App
{
    public string Name { get; }
    public OtherApp(string? name = null)
    {
        Name = name ?? "";
    }
}

public class CustomApp : App
{
    public string Value { get; }
    public CustomApp(string value)
    {
        Value = value;
    }
}
