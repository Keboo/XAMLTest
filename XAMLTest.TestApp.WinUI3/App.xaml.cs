// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace XAMLTest.TestApp;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        Assembly a = Assembly.LoadFrom(@"D:\Dev\XAMLTest\ClassLibrary1\bin\Debug\net6.0-windows10.0.19041.0\ClassLibrary1.dll");
        Type windowType = a.GetType("ClassLibrary1.BlankWindow1")!;
        m_window = (Window)Activator.CreateInstance(windowType)!;
        //m_window = new MainWindow();
        m_window.Activate();
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        return null;
    }

    private Window? m_window;
}
