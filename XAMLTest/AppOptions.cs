namespace XamlTest;

public class AppOptions
{
    private string? _xamlTestPath;

    public string? ApplicationType { get; set; }
    public string? RemoteAppPath { get; set; }
    public string XamlTestPath
    {
        get
        {
            if (_xamlTestPath is { } path)
            {
                return path;
            }
            var xamlTestPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");
            return Path.GetFullPath(xamlTestPath);
        }

        set => _xamlTestPath = value;
    }
    public Action<string>? LogMessage { get; set; }
    public FileInfo? RemoteProcessLogFile { get; }
    public bool AllowVisualStudioDebuggerAttach { get; set; } = true;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(1);

    internal RemoteFactoryMethod? RemoteFactoryMethod { get; set; }
    
    public AppOptions()
    {
        var directory = Path.GetDirectoryName(XamlTestPath);
        var file = Path.ChangeExtension(Path.GetRandomFileName(), ".xamltest.log");
        RemoteProcessLogFile = new FileInfo(Path.Combine(directory!, file));
    }

    public void WithRemoteApp<TApp>(Func<TApp>? factory = null)
        where TApp : Application
    {
        RemoteAppPath = typeof(TApp).Assembly.Location;
        ApplicationType = typeof(TApp).AssemblyQualifiedName;

        if (factory is not null)
        {
            if (factory.Target is not null)
            {
                throw new ArgumentException("Cannot execute a non-static factory method");
            }
            if (factory.Method.DeclaringType is null)
            {
                throw new ArgumentException("Could not find containing type for factory method");
            }
            if (factory.Method.IsGenericMethod)
            {
                throw new ArgumentException("Factory method must not be generic");
            }

            RemoteFactoryMethod = new(factory.Method.Name,
                factory.Method.DeclaringType.FullName!, 
                factory.Method.DeclaringType.Assembly.Location);
        }
    }
}

internal class RemoteFactoryMethod
{
    public string MethodName { get; }
    public string MethodContainerType { get; }
    public string Assembly { get; }

    public RemoteFactoryMethod(string methodName, string methodContainerType, string assembly)
    {
        MethodName = methodName;
        MethodContainerType = methodContainerType;
        Assembly = assembly;
    }
}
