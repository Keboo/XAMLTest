namespace XamlTest;

public interface IWindow : IVisualElement<Window>, IEquatable<IWindow>
{
    /// <summary>
    /// Gets the visual tree rooted at this window.
    /// </summary>
    /// <returns>A task that returns the root <see cref="VisualTreeNodeInfo"/> representing the window's visual tree.</returns>
    Task<VisualTreeNodeInfo> GetVisualTree();
}
