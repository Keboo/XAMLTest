using System.Windows.Media;

namespace XamlTest;

public interface IVisualElement<TElement> : IVisualElement
{
    Task<TReturn?> RemoteExecute<TReturn>(Delegate @delegate, object?[] parameters);
}

public interface IVisualElement : IEquatable<IVisualElement>
{
    /// <summary>
    /// Convert the element to strongly typed element.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    IVisualElement<TElement> As<TElement>() where TElement : DependencyObject;
    /// <summary>
    /// Find an element given a query. The query string is made up of several parts.
    /// ~&lt;Name&gt; - Search for an element by Name. This is the default query behavior if no prefix is specified.
    /// /&lt;Type Name&gt; - Search for an element by its given class name. This type name may be a base class name as well. Optionally this query part may also include a [&lt;<Index&gt;] suffix to retrieve objects are a particular index.
    /// .&lt;Property Name&gt; - Search for an element as the property of another element. This must follow anoter query part.
    /// [&lt;Property Query&gt;] - Search for an element with a property query. The property query should be in the form &lt;Property Name&gt;=&lt;Property Value&gt;
    /// </summary>
    /// <param name="query">The element query.</param>
    /// <returns>The found element</returns>
    Task<IVisualElement> GetElement(string query);
    /// <summary>
    /// Find an element given a query. The query string is made up of several parts.
    /// ~&lt;Name&gt; - Search for an element by Name. This is the default query behavior if no prefix is specified.
    /// /&lt;Type Name&gt; - Search for an element by its given class name. This type name may be a base class name as well. Optionally this query part may also include a [&lt;<Index&gt;] suffix to retrieve objects are a particular index.
    /// .&lt;Property Name&gt; - Search for an element as the property of another element. This must follow anoter query part.
    /// [&lt;Property Query&gt;] - Search for an element with a property query. The property query should be in the form &lt;Property Name&gt;=&lt;Property Value&gt;
    /// </summary>
    /// <param name="query">The element query.</param>
    /// <returns>The found element</returns>
    Task<IVisualElement<TElement>> GetElement<TElement>(string query);

    /// <summary>
    /// Find an element given a query. The query string is made up of several parts.
    /// ~&lt;Name&gt; - Search for an element by Name. This is the default query behavior if no prefix is specified.
    /// /&lt;Type Name&gt; - Search for an element by its given class name. This type name may be a base class name as well. Optionally this query part may also include a [&lt;<Index&gt;] suffix to retrieve objects are a particular index.
    /// .&lt;Property Name&gt; - Search for an element as the property of another element. This must follow anoter query part.
    /// [&lt;Property Query&gt;] - Search for an element with a property query. The property query should be in the form &lt;Property Name&gt;=&lt;Property Value&gt;
    /// </summary>
    /// <param name="query">The element query.</param>
    /// <returns>The found element or null if it is not found</returns>
    Task<IVisualElement?> FindElement(string query);

    /// <summary>
    /// Find an element given a query. The query string is made up of several parts.
    /// ~&lt;Name&gt; - Search for an element by Name. This is the default query behavior if no prefix is specified.
    /// /&lt;Type Name&gt; - Search for an element by its given class name. This type name may be a base class name as well. Optionally this query part may also include a [&lt;<Index&gt;] suffix to retrieve objects are a particular index.
    /// .&lt;Property Name&gt; - Search for an element as the property of another element. This must follow anoter query part.
    /// [&lt;Property Query&gt;] - Search for an element with a property query. The property query should be in the form &lt;Property Name&gt;=&lt;Property Value&gt;
    /// </summary>
    /// <param name="query">The element query.</param>
    /// <returns>The found element or null if not found</returns>
    Task<IVisualElement<TElement>?> FindElement<TElement>(string query);

    /// <summary>
    /// Retrieve a property's value
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="ownerType">The assembly qualified name of the type that contains the property</param>
    /// <returns>The value for the property</returns>
    Task<IValue> GetProperty(string name, string? ownerType);
    /// <summary>
    /// Set a property's value.
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="value">The serialized value for the property</param>
    /// <param name="valueType">The assembly qualified type of the value</param>
    /// <param name="ownerType">The assembly qualified name of the type that owns the property</param>
    /// <returns></returns>
    Task<IValue> SetProperty(string name, string value, string? valueType, string? ownerType);

    /// <summary>
    /// Sets the value of a property by loading XAML content.
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="xaml">The XAML content to load</param>
    /// <returns>The root XAML element</returns>
    Task<IVisualElement> SetXamlProperty(string propertyName, XamlSegment xaml);
    /// <summary>
    /// Sets the value of a property by loading XAML content.
    /// </summary>
    /// <typeparam name="TElement">The root XAML element type</typeparam>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="xaml">The XAML content to load</param>
    /// <returns>The root XAML element</returns>
    Task<IVisualElement<TElement>> SetXamlProperty<TElement>(string propertyName, XamlSegment xaml);
    Task<IResource> GetResource(string key);

    Task<Color> GetEffectiveBackground(IVisualElement? toElement);
    /// <summary>
    /// Gets the coordinates of the element in screen coordinates.
    /// </summary>
    /// <returns>The smallest bounding rectangle encompassing the element in screen coordinates.</returns>
    Task<Rect> GetCoordinates();

    /// <summary>
    /// Registers for an event by its name.
    /// </summary>
    /// <param name="name">The name of the event</param>
    /// <returns></returns>
    Task<IEventRegistration> RegisterForEvent(string name);
    /// <summary>
    /// Un-regsiter an event that was previously registered. <see cref="RegisterForEvent(string)"/>
    /// </summary>
    /// <param name="eventRegistration">The event registration to unregister.</param>
    /// <returns></returns>
    Task UnregisterEvent(IEventRegistration eventRegistration);

    /// <summary>
    /// Moves the keyboard focus to this element if it is focusable.
    /// </summary>
    /// <returns></returns>
    Task MoveKeyboardFocus();

    /// <summary>
    /// Send keyboard input commands.
    /// </summary>
    /// <param name="keyboardInput">The keyboard input commands.</param>
    /// <returns></returns>
    Task SendInput(KeyboardInput keyboardInput);

    /// <summary>
    /// Send mouse input commands.
    /// </summary>
    /// <param name="mouseInput">The mouse input commands.</param>
    /// <returns>The final position of the mouse cursor in screen coordinates.</returns>
    Task<Point> SendInput(MouseInput mouseInput);

    /// <summary>
    /// Applies highlighting to the control.
    /// </summary>
    /// <returns></returns>
    Task Highlight(HighlightConfig highlightConfig);
}
