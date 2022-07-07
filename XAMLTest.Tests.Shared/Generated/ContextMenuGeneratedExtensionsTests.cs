namespace XamlTest.Tests.Generated;

partial class ContextMenuGeneratedExtensionsTests
{
#if WPF
    static partial void OnClassInitialize()
    {
        GetWindowContent = x =>
        {
            return @$"
    <Window.ContextMenu>
        {x}
    </Window.ContextMenu>";
        };
    }
#endif
}
