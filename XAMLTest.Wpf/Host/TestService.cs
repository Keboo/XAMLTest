using Google.Protobuf;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using XamlTest.Internal;
using XamlTest.Transport;
using Brush = System.Windows.Media.Brush;
using Point = System.Windows.Point;
using WpfColor = System.Windows.Media.Color;



namespace XamlTest.Host;

internal partial class TestService : InternalTestService
{
    private static Guid Initialized { get; } = Guid.NewGuid();

    private List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

    private Application Application { get; }

    public TestService(Application application)
    {
        Application = application ?? throw new ArgumentNullException(nameof(application));
        AddSerializer(new GridSerializer());
    }

    protected override async Task<GetWindowsResult> GetWindows(GetWindowsQuery request)
    {
        var ids = await Application.Dispatcher.InvokeAsync(() =>
        {
            return Application.Windows
                .Cast<Window>()
                .Select(window => DependencyObjectTracker.GetOrSetId(window, KnownElements))
                .ToList();
        });

        GetWindowsResult reply = new();
        reply.WindowIds.AddRange(ids);
        return reply;
    }

    protected override async Task<GetWindowsResult> GetMainWindow(GetWindowsQuery request)
    {
        string? id = await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                System.Windows.Window mainWindow = Application.MainWindow;
                return DependencyObjectTracker.GetOrSetId(mainWindow, KnownElements);
            }
            catch (Exception)
            {
                return null;
            }
        });

        GetWindowsResult reply = new();
        if (!string.IsNullOrWhiteSpace(id))
        {
            reply.WindowIds.Add(id);
        }
        return reply;
    }

    protected override async Task<PropertyResult> GetProperty(PropertyQuery request)
    {
        PropertyResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                DependencyObject? element = GetCachedElement<DependencyObject>(request.ElementId);
                if (element is null)
                {
                    reply.ErrorMessages.Add("Could not find element");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(request.OwnerType))
                {
                    if (DependencyPropertyHelper.TryGetDependencyProperty(request.Name, request.OwnerType,
                        out DependencyProperty? dependencyProperty))
                    {
                        object? value = element.GetValue(dependencyProperty);
                        SetValue(reply, dependencyProperty.PropertyType, value);
                    }
                    else
                    {
                        reply.ErrorMessages.Add($"Could not find dependency property '{request.Name}' on '{request.OwnerType}'");
                        return;
                    }
                }
                else
                {
                    var properties = TypeDescriptor.GetProperties(element);
                    PropertyDescriptor? foundProperty = properties.Find(request.Name, false);
                    if (foundProperty is null)
                    {
                        reply.ErrorMessages.Add($"Could not find property with name '{request.Name}' on element '{element.GetType().FullName}'");
                        return;
                    }
                    object? value = foundProperty.GetValue(element);
                    SetValue(reply, foundProperty.PropertyType, value);
                }
            }
            catch (Exception ex)
            {
                reply.ErrorMessages.Add(ex.ToString());
            }
        });
        return reply;
    }

    protected override async Task<EffectiveBackgroundResult> GetEffectiveBackground(EffectiveBackgroundQuery request)
    {
        EffectiveBackgroundResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            DependencyObject? element = GetCachedElement<DependencyObject>(request.ElementId);
            if (element is null)
            {
                reply.ErrorMessages.Add("Could not find element");
                return;
            }

            DependencyObject? toElement = GetCachedElement<DependencyObject>(request.ToElementId);

            WpfColor currentColor = Colors.Transparent;
            bool reachedToElement = false;
            foreach (var ancestor in Ancestors<DependencyObject>(element))
            {
                if (reachedToElement)
                {
                    if (ancestor is FrameworkElement ancestorElement)
                    {
                        currentColor = currentColor.WithOpacity(ancestorElement.Opacity);
                    }
                    continue;
                }
                Brush? background = GetBackground(ancestor);

                if (background is SolidColorBrush brush)
                {
                    var parentBackground = brush.Color;
                    if (ancestor is FrameworkElement ancestorElement)
                    {
                        parentBackground = parentBackground.WithOpacity(ancestorElement.Opacity);
                    }

                    currentColor = currentColor.FlattenOnto(parentBackground);
                }
                else if (background != null)
                {
                    reply.ErrorMessages.Add($"Could not evaluate background brush of type '{background.GetType().Name}' on '{ancestor.GetType().FullName}'");
                    break;
                }
                if (ancestor == toElement)
                {
                    reachedToElement = true;
                }
            }
            reply.Alpha = currentColor.A;
            reply.Red = currentColor.R;
            reply.Green = currentColor.G;
            reply.Blue = currentColor.B;
        });
        return reply;

        static Brush? GetBackground(DependencyObject element)
        {
            return element switch
            {
                Border border => border.Background,
                Control control => control.Background,
                Panel panel => panel.Background,
                _ => null
            };
        }
    }

    protected override async Task<PropertyResult> SetProperty(SetPropertyRequest request)
    {
        PropertyResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                DependencyObject? element = GetCachedElement<DependencyObject>(request.ElementId);
                if (element is null)
                {
                    reply.ErrorMessages.Add("Could not find element");
                    return;
                }

                object? value;
                Type propertyType;
                if (!string.IsNullOrWhiteSpace(request.OwnerType))
                {
                    if (DependencyPropertyHelper.TryGetDependencyProperty(request.Name, request.OwnerType,
                        out DependencyProperty? dependencyProperty))
                    {
                        var propertyConverter = TypeDescriptor.GetConverter(dependencyProperty.PropertyType);
                        value = GetValue(propertyConverter);

                        element.SetValue(dependencyProperty, value);

                        //Re-retrive the value in case the dependency property coalesced it
                        value = element.GetValue(dependencyProperty);
                        propertyType = dependencyProperty.PropertyType;
                    }
                    else
                    {
                        reply.ErrorMessages.Add($"Could not find dependency property '{request.Name}' on '{request.OwnerType}'");
                        return;
                    }
                }
                else
                {
                    var properties = TypeDescriptor.GetProperties(element);
                    PropertyDescriptor? foundProperty = properties.Find(request.Name, false);
                    if (foundProperty is null)
                    {
                        reply.ErrorMessages.Add($"Could not find property with name '{request.Name}'");
                        return;
                    }

                    TypeConverter? propertyTypeConverter = null;
                    if (string.IsNullOrWhiteSpace(request.ValueType))
                    {
                        propertyTypeConverter = foundProperty.Converter;
                    }
                    else if (Type.GetType(request.ValueType) is { } requestedValueType)
                    {
                        propertyTypeConverter = TypeDescriptor.GetConverter(requestedValueType);
                    }
                    value = GetValue(propertyTypeConverter);

                    foundProperty.SetValue(element, value);

                    //Re-retrive the value in case the dependency property coalesced it
                    value = foundProperty.GetValue(element);
                    propertyType = foundProperty.PropertyType;
                }

                SetValue(reply, propertyType, value);
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
        });
        return reply;


        object? GetValue(TypeConverter? propertyConverter)
        {
            if (propertyConverter != null)
            {
                return propertyConverter.ConvertFromString(request.Value);
            }
            return request.Value;
        }
    }

    protected override async Task<ElementResult> SetXamlProperty(SetXamlPropertyRequest request)
    {
        ElementResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                DependencyObject? element = GetCachedElement<DependencyObject>(request.ElementId);
                if (element is null)
                {
                    reply.ErrorMessages.Add("Could not find element");
                    return;
                }

                object? value = LoadXaml<object>(request.Xaml, request.Namespaces);
                if (!string.IsNullOrWhiteSpace(request.OwnerType))
                {
                    if (DependencyPropertyHelper.TryGetDependencyProperty(request.Name, request.OwnerType,
                        out DependencyProperty? dependencyProperty))
                    {
                        element.SetValue(dependencyProperty, value);

                        //Re-retrive the value in case the dependency property coalesced it
                        value = element.GetValue(dependencyProperty);
                    }
                    else
                    {
                        reply.ErrorMessages.Add($"Could not find dependency property '{request.Name}' on '{request.OwnerType}'");
                        return;
                    }
                }
                else
                {
                    var properties = TypeDescriptor.GetProperties(element);
                    PropertyDescriptor? foundProperty = properties.Find(request.Name, false);
                    if (foundProperty is null)
                    {
                        reply.ErrorMessages.Add($"Could not find property with name '{request.Name}'");
                        return;
                    }
                    
                    foundProperty.SetValue(element, value);

                    //Re-retrive the value in case the dependency property coalesced it
                    value = foundProperty.GetValue(element);
                }

                reply.Elements.Add(GetElement(value as DependencyObject));
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
        });
        return reply;
    }

    private void SetValue(PropertyResult reply, Type propertyType, object? value)
    {
        reply.PropertyType = propertyType.AssemblyQualifiedName;
        Type valueType = value?.GetType() ?? propertyType;
        reply.ValueType = valueType.AssemblyQualifiedName;

        if (propertyType == typeof(DependencyObject) ||
            propertyType.IsSubclassOf(typeof(DependencyObject)))
        {
            reply.Element = GetElement(value as DependencyObject);
        }
        string? serializedValue = Serializer.Serialize(valueType, value);
        if (serializedValue is null)
        {
            reply.ErrorMessages.Add($"Failed to serialize object of type '{propertyType.AssemblyQualifiedName}'");
        }
        else
        {
            reply.Value = serializedValue;
        }
    }

    protected override async Task<ResourceResult> GetResource(ResourceQuery request)
    {
        var reply = new ResourceResult
        {
            Key = request.Key
        };
        await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                FrameworkElement? element = GetCachedElement<FrameworkElement>(request.ElementId);
                object resourceValue = element is null ?
                    Application.TryFindResource(request.Key) :
                    element.TryFindResource(request.Key);

                reply.Value = "";
                reply.ValueType = "";

                if (resourceValue?.GetType() is { } type)
                {
                    string? serializedValue = Serializer.Serialize(type, resourceValue);
                    if (serializedValue is null)
                    {
                        reply.ErrorMessages.Add($"Failed to serialize object of type '{type.AssemblyQualifiedName}'");
                    }
                    else
                    {
                        reply.ValueType = type.AssemblyQualifiedName;
                        reply.Value = serializedValue;
                    }
                }


            }
            catch (Exception ex)
            {
                reply.ErrorMessages.Add(ex.ToString());
            }
        });

        return reply;
    }

    protected override async Task<CoordinatesResult> GetCoordinates(CoordinatesQuery request)
    {
        CoordinatesResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            DependencyObject? dependencyObject = GetCachedElement<DependencyObject>(request.ElementId);
            if (dependencyObject is null)
            {
                reply.ErrorMessages.Add("Could not find element");
                return;
            }

            if (dependencyObject is FrameworkElement element)
            {
                Area rect = GetCoordinates(element);

                reply.Left = rect.Left;
                reply.Top = rect.Top;
                reply.Right = rect.Right;
                reply.Bottom = rect.Bottom;
            }
            else
            {
                reply.ErrorMessages.Add($"Element of type '{dependencyObject.GetType().FullName}' is not a {nameof(FrameworkElement)}");
            }
        });
        return reply;
    }

    protected override async Task<ApplicationResult> InitializeApplication(ApplicationConfiguration request)
    {
        ApplicationResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            if (Application.Resources[Initialized] is Guid value &&
                value == Initialized)
            {
                reply.ErrorMessages.Add("Application has already been initialized");
            }
            else
            {
                Application.Resources[Initialized] = Initialized;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                foreach (string? assembly in (IEnumerable<string?>)request.AssembliesToLoad ?? Array.Empty<string?>())
                {
                    try
                    {
                        if (assembly is string)
                        {
                            LoadedAssemblies.Add(Assembly.LoadFrom(assembly));
                        }
                        else
                        {
                            reply.ErrorMessages.Add("Assembly name must not be null");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        reply.ErrorMessages.Add($"Failed to load '{assembly}'{Environment.NewLine}{e}");
                    }
                }

                if (request.ApplicationResourceXaml is { } xaml)
                {
                    try
                    {
                        ResourceDictionary appResourceDictionary = LoadXaml<ResourceDictionary>(xaml);
                        
                        foreach (var mergedDictionary in appResourceDictionary.MergedDictionaries)
                        {
                            Application.Resources.MergedDictionaries.Add(mergedDictionary);
                        }
                        foreach (object? key in appResourceDictionary.Keys)
                        {
                            Application.Resources.Add(key, appResourceDictionary[key]);
                        }
                    }
                    catch (Exception e)
                    {
                        reply.ErrorMessages.Add($"Error loading application resources{Environment.NewLine}{e}");
                    }
                }
            }
        });
        return reply;
    }

    protected override async Task<WindowResult> CreateWindow(WindowConfiguration request)
    {
        WindowResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            System.Windows.Window? window = null;
            if (!string.IsNullOrEmpty(request.WindowType))
            {
                try
                {
                    if (Type.GetType(request.WindowType) is Type windowType)
                    {
                        window = (System.Windows.Window?)Activator.CreateInstance(windowType);
                    }
                    else
                    {
                        reply.ErrorMessages.Add($"Error loading window type '{request.WindowType}'");
                    }
                }
                catch(Exception e)
                {
                    reply.ErrorMessages.Add($"Error creating window '{request.WindowType}'{Environment.NewLine}{e}");
                }
            }
            else
            {
                try
                {
                    window = LoadXaml<System.Windows.Window>(request.Xaml);
                }
                catch (Exception e)
                {
                    reply.ErrorMessages.Add($"Error loading window{Environment.NewLine}{e}");
                }
            }
            if (window is { })
            {
                reply.WindowsId = DependencyObjectTracker.GetOrSetId(window, KnownElements);
                window.Show();

                if (request.FitToScreen)
                {
                    Rect windowRect = new(window.Left, window.Top, window.Width, window.Height);
                    Screen screen = Screen.FromRect(windowRect);
                    window.LogMessage($"Fitting window {windowRect} to screen {screen.WorkingArea}");
                    if (!screen.WorkingArea.Contains(windowRect))
                    {
                        window.Left = Math.Max(window.Left, screen.WorkingArea.Left);
                        window.Left = Math.Max(screen.WorkingArea.Left, window.Left + window.Width - screen.WorkingArea.Right - window.Width);

                        window.Top = Math.Max(window.Top, screen.WorkingArea.Top);
                        window.Top = Math.Max(screen.WorkingArea.Top, window.Top + window.Height - screen.WorkingArea.Top - window.Height);

                        window.Width = Math.Min(window.Width, screen.WorkingArea.Width);
                        window.Height = Math.Min(window.Height, screen.WorkingArea.Height);

                        window.LogMessage($"Window's new size and location {new Rect(window.Left, window.Top, window.Width, window.Height)}");
                    }
                }

                if (window.ShowActivated && !ActivateWindow(window))
                {
                    reply.ErrorMessages.Add("Failed to activate window");
                    return;
                }

                reply.LogMessages.AddRange(window.GetLogMessages());
            }
            else
            {
                reply.ErrorMessages.Add("Failed to load window");
            }
        });

        return reply;
    }

    protected override async Task<ImageResult> GetScreenshot(ImageQuery request)
    {
        ImageResult reply = new();
        await Application.Dispatcher.Invoke(async () =>
        {
            System.Windows.Window mainWindow = Application.MainWindow;
            Screen? screen;
            if (mainWindow is null)
            {
                screen = Screen.PrimaryScreen;
            }
            else
            {
                Point topLeft = mainWindow.PointToScreen(new Point(0, 0));
                screen = Screen.FromRect(new Rect(topLeft.X, topLeft.Y, mainWindow.ActualWidth, mainWindow.ActualHeight));
            }

            if (screen is null)
            {
                reply.ErrorMessages.Add("Fail to find screen");
                return;
            }

            using Bitmap screenBmp = new((int)screen.Bounds.Width, (int)screen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var bmpGraphics = Graphics.FromImage(screenBmp);
            bmpGraphics.CopyFromScreen(
                (int)Math.Floor(screen.Bounds.Left),
                (int)Math.Floor(screen.Bounds.Top),
                0, 
                0, 
                screenBmp.Size);
            using MemoryStream ms = new();
            screenBmp.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;
            reply.Data = await ByteString.FromStreamAsync(ms);
        });
        return reply;
    }

    protected override Task<ShutdownResult> Shutdown(ShutdownRequest request)
    {
        ShutdownResult reply = new();
        try
        {
            _ = Application.Dispatcher.InvokeAsync(() =>
              {
                  Application.Shutdown(request.ExitCode);
              });
        }
        catch (Exception e)
        {
            reply.ErrorMessages.Add(e.ToString());
        }
        return Task.FromResult(reply);
    }

    protected override Task<VersionResult> GetVersion(VersionRequest request)
    {
        VersionResult reply = new();
        try
        {
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            reply.XamlTestVersion = fvi.FileVersion;

            fvi = FileVersionInfo.GetVersionInfo(Application.GetType().Assembly.Location);
            reply.AppVersion = fvi.FileVersion;
        }
        catch (Exception e)
        {
            reply.ErrorMessages.Add(e.ToString());
        }
        return Task.FromResult(reply);
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        Assembly? found = LoadedAssemblies.FirstOrDefault(x => x.GetName().FullName == args.Name);
        if (found is { }) return found;

        AssemblyName assemblyName = new(args.Name!);
        string likelyAssemblyPath = Path.GetFullPath($"{assemblyName.Name}.dll");
        try
        {
            if (File.Exists(likelyAssemblyPath) && Assembly.LoadFrom(likelyAssemblyPath) is Assembly localAssemby)
            {
                LoadedAssemblies.Add(localAssemby);
                return localAssemby;
            }
        }
        catch (Exception)
        {
            //Ignored
        }
        return null;
    }

    private static T LoadXaml<T>(string xaml) where T : class
        => LoadXaml<T>(xaml, Enumerable.Empty<XamlNamespace>());

    private static T LoadXaml<T>(string xaml, IEnumerable<XamlNamespace> namespaces) where T : class
    {
        using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(xaml));
        ParserContext context = new();
        foreach(var @namespace in namespaces)
        {
            context.XmlnsDictionary.Add(@namespace.Prefix, @namespace.Uri);
        }
        return (T)XamlReader.Load(memoryStream, context);
    }

    private static IEnumerable<T> Ancestors<T>(DependencyObject? element)
        where T : DependencyObject
    {
        for (; element != null; element = VisualTreeHelper.GetParent(element))
        {
            if (element is T typedElement)
            {
                yield return typedElement;
            }
        }
    }

    private static bool ActivateWindow(System.Windows.Window window)
    {
        window.LogMessage("Activating window");

        if (window.IsActive)
        {
            window.LogMessage("Window already active");
            return true;
        }

        if (window.Activate())
        {
            return true;
        }

        window.LogMessage("Using mouse to activate Window");

        //Fall back, attempt to click on the window to activate it
        foreach (Location clickPoint in GetClickPoints(window))
        {
            Input.MouseInput.MoveCursor(clickPoint);
            Input.MouseInput.LeftClick();

            if (window.IsActive)
            {
                return true;
            }
        }

        return window.IsActive;

        static IEnumerable<Location> GetClickPoints(System.Windows.Window window)
        {
            //Skip top right and that could cause the window to close

            // Top left
            yield return new Location(window.Left + 1, window.Top + 1);

            // Bottom right
            yield return new Location(window.Left + window.Width - 1, window.Top + window.Height - 1);

            // Bottom left
            yield return new Location(window.Left + 1, window.Top + window.Height - 1);

            // Center
            yield return new Location(window.Left + window.Width / 2, window.Top + window.Height / 2);
        }
    }

    private static Area GetCoordinates(FrameworkElement element)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var window = element as System.Windows.Window ?? System.Windows.Window.GetWindow(element);
        Point windowOrigin = window.PointToScreen(new Point(0, 0));

        Point topLeft = element.TranslatePoint(new Point(0, 0), window);
        Point bottomRight = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), window);
        double left = windowOrigin.X + topLeft.X;
        double top = windowOrigin.Y + topLeft.Y;
        double right = windowOrigin.X + bottomRight.X;
        double bottom = windowOrigin.Y + bottomRight.Y;

        var rvleft = Math.Min(left, right);
        var rvtop = Math.Min(top, bottom);
        var rvright = Math.Max(left, right);
        var rvbottom = Math.Max(top, bottom);

        return new Area(rvleft, rvtop, rvright, rvbottom);
    }
}
