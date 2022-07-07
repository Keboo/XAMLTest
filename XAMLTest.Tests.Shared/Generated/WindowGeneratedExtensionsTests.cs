namespace XamlTest.Tests.Generated;

partial class WindowGeneratedExtensionsTests
{
#if WPF
    static partial void OnClassInitialize()
    {
        GetWindowContent = x => "";
        GetElement = _ => Task.FromResult<IVisualElement<Window>>(Window);
    }
#endif
}
