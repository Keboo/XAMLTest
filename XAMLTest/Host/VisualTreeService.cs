using Google.Protobuf;
using Grpc.Core;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using XamlTest.Internal;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace XamlTest.Host;

internal partial class VisualTreeService : Protocol.ProtocolBase
{
    private static Guid Initialized { get; } = Guid.NewGuid();

    private List<Assembly> LoadedAssemblies { get; } = new List<Assembly>
    {
        Assembly.GetExecutingAssembly()
    };

    private Application Application { get; }

    private Serializer Serializer { get; } = new Serializer();

    public VisualTreeService(Application application)
        => Application = application ?? throw new ArgumentNullException(nameof(application));

    public override async Task<GetWindowsResult> GetWindows(GetWindowsQuery request, ServerCallContext context)
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

    public override async Task<GetWindowsResult> GetMainWindow(GetWindowsQuery request, ServerCallContext context)
    {
        string? id = await Application.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                Window mainWindow = Application.MainWindow;
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

    public override async Task<PropertyResult> GetProperty(PropertyQuery request, ServerCallContext context)
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

    public override async Task<EffectiveBackgroundResult> GetEffectiveBackground(EffectiveBackgroundQuery request, ServerCallContext context)
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

            Color currentColor = Colors.Transparent;
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
                    Color parentBackground = brush.Color;
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

    public override async Task<PropertyResult> SetProperty(SetPropertyRequest request, ServerCallContext context)
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

                        //Re-retrieve the value in case the dependency property coalesced it
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
                    else if (Type.GetType(request.ValueType) is { } requestedValueType &&
                             requestedValueType != typeof(object))
                    {
                        propertyTypeConverter = TypeDescriptor.GetConverter(requestedValueType);
                    }
                    value = GetValue(propertyTypeConverter);

                    foundProperty.SetValue(element, value);

                    //Re-retrieve the value in case the dependency property coalesced it
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
                return propertyConverter.ConvertFromString(null!, CultureInfo.InvariantCulture, request.Value);
            }
            return request.Value;
        }
    }

    public override async Task<ElementResult> SetXamlProperty(SetXamlPropertyRequest request, ServerCallContext context)
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

                        //Re-retrieve the value in case the dependency property coalesced it
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

                    //Re-retrieve the value in case the dependency property coalesced it
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

    public override async Task<ResourceResult> GetResource(ResourceQuery request, ServerCallContext context)
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

    public override async Task<CoordinatesResult> GetCoordinates(CoordinatesQuery request, ServerCallContext context)
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
                Rect rect = GetCoordinates(element);

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

    public override async Task<ApplicationResult> InitializeApplication(ApplicationConfiguration request, ServerCallContext context)
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
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                foreach (string? assembly in (IEnumerable<string?>)request.AssembliesToLoad ?? Array.Empty<string?>())
                {
                    try
                    {
                        if (assembly is not null)
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

    public override async Task<WindowResult> CreateWindow(WindowConfiguration request, ServerCallContext context)
    {
        WindowResult reply = new();
        await Application.Dispatcher.InvokeAsync(() =>
        {
            Window? window = null;
            try
            {
                if (!string.IsNullOrEmpty(request.WindowType))
                {
                    try
                    {
                        if (Type.GetType(request.WindowType) is Type windowType)
                        {
                            window = (Window?)Activator.CreateInstance(windowType);
                        }
                        else
                        {
                            reply.ErrorMessages.Add($"Error loading window type '{request.WindowType}'");
                        }
                    }
                    catch (Exception e)
                    {
                        reply.ErrorMessages.Add($"Error creating window '{request.WindowType}'{Environment.NewLine}{e}");
                    }
                }
                else
                {
                    window = LoadXaml<Window>(request.Xaml);

                }
                if (window is not null)
                {
                    reply.WindowsId = DependencyObjectTracker.GetOrSetId(window, KnownElements);
                    window.Show();

                    if (request.FitToScreen)
                    {
                        Rect windowRect = new(window.Left, window.Top, window.Width, window.Height);
                        Screen screen = Screen.FromRect(windowRect);
                        Logger.Log($"Fitting window {windowRect} to screen {screen.WorkingArea}");
                        if (!screen.WorkingArea.Contains(windowRect))
                        {
                            window.Left = Math.Max(window.Left, screen.WorkingArea.Left);
                            window.Left = Math.Max(screen.WorkingArea.Left, window.Left + window.Width - screen.WorkingArea.Right - window.Width);

                            window.Top = Math.Max(window.Top, screen.WorkingArea.Top);
                            window.Top = Math.Max(screen.WorkingArea.Top, window.Top + window.Height - screen.WorkingArea.Top - window.Height);

                            window.Width = Math.Min(window.Width, screen.WorkingArea.Width);
                            window.Height = Math.Min(window.Height, screen.WorkingArea.Height);

                            Logger.Log($"Window's new size and location {new Rect(window.Left, window.Top, window.Width, window.Height)}");
                        }
                    }

                    if (window.ShowActivated && !ActivateWindow(window))
                    {
                        reply.ErrorMessages.Add("Failed to activate window");
                        return;
                    }

                    reply.LogMessages.AddRange(Logger.GetMessages());
                }
                else
                {
                    reply.ErrorMessages.Add("Failed to load window");
                }
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add($"Error loading window{Environment.NewLine}{e}");
            }
        });

        return reply;
    }

    public override async Task<ImageResult> GetScreenshot(ImageQuery request, ServerCallContext context)
    {
        ImageResult reply = new();
        await Application.Dispatcher.Invoke(async () =>
        {
            Window mainWindow = Application.MainWindow;
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

    public override Task<ShutdownResult> Shutdown(ShutdownRequest request, ServerCallContext context)
    {
        Logger.CloseLogger();
        Task.Run(async () =>
        {
            //Allow for some time for the response to be sent back to the caller
            await Task.Delay(500);
            _ = Application.Dispatcher.InvokeAsync(() =>
            {
                Application.Shutdown(request.ExitCode);
            });
        });
        ShutdownResult reply = new();
        return Task.FromResult(reply);
    }

    public override Task<SerializerResult> RegisterSerializer(SerializerRequest request, ServerCallContext context)
    {
        SerializerResult reply = new();
        try
        {
            if (string.IsNullOrWhiteSpace(request.SerializerType))
            {
                reply.ErrorMessages.Add("Serializer type must be specified");
                return Task.FromResult(reply);
            }
            if (Type.GetType(request.SerializerType) is { } serializerType &&
                Activator.CreateInstance(serializerType) is ISerializer serializer)
            {
                Serializer.AddSerializer(serializer, request.InsertIndex);
            }
            else
            {
                reply.ErrorMessages.Add($"Failed to resolve serializer type '{request.SerializerType}'");
            }
        }
        catch (Exception e)
        {
            reply.ErrorMessages.Add(e.ToString());
        }
        return Task.FromResult(reply);
    }

    public override Task<VersionResult> GetVersion(VersionRequest request, ServerCallContext context)
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
            if (File.Exists(likelyAssemblyPath) && Assembly.LoadFrom(likelyAssemblyPath) is Assembly localAssembly)
            {
                LoadedAssemblies.Add(localAssembly);
                return localAssembly;
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
        foreach (var @namespace in namespaces)
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

    private static bool ActivateWindow(Window window)
    {
        Logger.Log("Activating window");

        if (window.IsActive)
        {
            Logger.Log("Window already active");
            return true;
        }

        if (window.Activate())
        {
            return true;
        }

        //Check if focus is in child window, such as a popup
        IntPtr foregroundWindowHandle = PInvoke.User32.GetForegroundWindow();
        IntPtr expectedParentHandle = new WindowInteropHelper(window).EnsureHandle();

        while (foregroundWindowHandle != IntPtr.Zero &&
            expectedParentHandle != IntPtr.Zero)
        {
            if (foregroundWindowHandle == expectedParentHandle)
            {
                Logger.Log("Child window has is foreground");
                return true;
            }
            foregroundWindowHandle = PInvoke.User32.GetParent(foregroundWindowHandle);
        }

        Logger.Log("Using mouse to activate Window");

        //Fall back, attempt to click on the window to activate it
        foreach (Point clickPoint in GetClickPoints(window))
        {
            Input.MouseInput.MoveCursor(window.PointToScreen(clickPoint));
            Input.MouseInput.LeftClick();

            if (window.IsActive)
            {
                return true;
            }
        }

        return window.IsActive;

        static IEnumerable<Point> GetClickPoints(Window window)
        {
            //Skip top right and that could cause the window to close

            // Top left
            yield return new Point(1, 1);

            // Bottom right
            yield return new Point(window.Width - 1, window.Height - 1);

            // Bottom left
            yield return new Point(1, window.Height - 1);

            // Center
            yield return new Point(window.Width / 2, window.Height / 2);
        }
    }

    private static Rect GetCoordinates(FrameworkElement element)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var window = element as Window ?? Window.GetWindow(element);

        Point windowOrigin = window.PointToScreen(new Point(0, 0));

        var scale = VisualElementMixins.GetVisualScale(element);
        Point topLeft = element.TranslatePoint(new Point(0, 0), window);
        Point bottomRight = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), window);

        double left = windowOrigin.X + (topLeft.X * scale.DpiScaleX);
        double top = windowOrigin.Y + (topLeft.Y * scale.DpiScaleY);
        double right = windowOrigin.X + (bottomRight.X * scale.DpiScaleX);
        double bottom = windowOrigin.Y + (bottomRight.Y * scale.DpiScaleY);

        var rvLeft = Math.Min(left, right);
        var rvTop = Math.Min(top, bottom);
        var rvRight = Math.Max(left, right);
        var rvBottom = Math.Max(top, bottom);

        return new Rect(rvLeft, rvTop, rvRight - rvLeft, rvBottom - rvTop);
    }

    private static (double ScaleX, double ScaleY) GetScalingFromVisual(Visual visual)
    {
        PresentationSource source = PresentationSource.FromVisual(visual);

        if (source != null)
        {
            double scaleX = source.CompositionTarget.TransformToDevice.M11;
            double scaleY = source.CompositionTarget.TransformToDevice.M22;
            return (scaleX, scaleY);
        }
        return (1.0, 1.0);
    }
}
