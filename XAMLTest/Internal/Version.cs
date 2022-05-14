namespace XamlTest.Internal;

internal class Version : IVersion
{
    public Version(string appVersion, string xamlTestVersion)
    {
        AppVersion = appVersion;
        XamlTestVersion = xamlTestVersion;
    }

    public string AppVersion { get; }

    public string XamlTestVersion { get; }
}
