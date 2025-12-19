namespace XamlTest;

/// <summary>
/// Represents an application interface for XAML testing, providing methods for initialization, window management, remote execution, resource access, and logging.
/// </summary>
public interface IApp : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Initializes the application with the specified App.xaml content and referenced assemblies.
    /// </summary>
    /// <param name="applicationResourceXaml">The XAML content for App.xaml.</param>
    /// <param name="assemblies">Assemblies to reference during initialization.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Initialize(string applicationResourceXaml, params string[] assemblies);
    
    /// <summary>
    /// Creates a new window using the provided XAML.
    /// </summary>
    /// <param name="xaml">The XAML markup for the window.</param>
    /// <returns>A task that returns the created <see cref="IWindow"/>.</returns>
    Task<IWindow> CreateWindow(string xaml);

    /// <summary>
    /// Creates a new window of the specified type.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to create.</typeparam>
    /// <returns>A task that returns the created <see cref="IWindow"/>.</returns>
    Task<IWindow> CreateWindow<TWindow>() where TWindow : Window;

    /// <summary>
    /// Executes a delegate remotely in the application context and returns the result.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the delegate.</typeparam>
    /// <param name="delegate">The delegate to execute.</param>
    /// <param name="parameters">Parameters to pass to the delegate.</param>
    /// <returns>A task that returns the result of the delegate execution.</returns>
    Task<TReturn?> RemoteExecute<TReturn>(Delegate @delegate, object?[] parameters);

    /// <summary>
    /// Gets the main window of the application, if available.
    /// </summary>
    /// <returns>A task that returns the main <see cref="IWindow"/>, or null if not found.</returns>
    Task<IWindow?> GetMainWindow();

    /// <summary>
    /// Gets all windows currently open in the application.
    /// </summary>
    /// <returns>A task that returns a read-only list of <see cref="IWindow"/> instances.</returns>
    Task<IReadOnlyList<IWindow>> GetWindows();

    /// <summary>
    /// Retrieves a resource by its key.
    /// </summary>
    /// <param name="key">The key of the resource to retrieve.</param>
    /// <returns>A task that returns the requested <see cref="IResource"/>.</returns>
    Task<IResource> GetResource(string key);

    /// <summary>
    /// Captures a screenshot of the application.
    /// </summary>
    /// <returns>A task that returns an <see cref="IImage"/> representing the screenshot.</returns>
    Task<IImage> GetScreenshot();

    /// <summary>
    /// Registers a serializer at the specified index.
    /// </summary>
    /// <typeparam name="T">The type of serializer to register.</typeparam>
    /// <param name="insertIndex">The index at which to insert the serializer.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RegisterSerializer<T>(int insertIndex = 0)
        where T : ISerializer, new();

    /// <summary>
    /// Gets the list of registered serializers.
    /// </summary>
    /// <returns>A task that returns a read-only list of <see cref="ISerializer"/> instances.</returns>
    Task<IReadOnlyList<ISerializer>> GetSerializers();

    /// <summary>
    /// Logs a message to the application log.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogMessage(string message);
    
    /// <summary>
    /// Gets the default XML namespaces used by the application.
    /// </summary>
    IList<XmlNamespace> DefaultXmlNamespaces { get; }

}
