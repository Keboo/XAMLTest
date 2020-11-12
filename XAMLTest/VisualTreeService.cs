using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using XamlTest.Input;
using XamlTest.Internal;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace XamlTest
{
    internal class VisualTreeService : Protocol.ProtocolBase
    {
        private static Guid Initialized { get; } = Guid.NewGuid();

        private List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

        private Application Application { get; }

        private Serializer Serializer { get; } = new Serializer();

        private IDictionary<string, WeakReference<DependencyObject>> KnownElements { get; }
            = new Dictionary<string, WeakReference<DependencyObject>>();

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

            var reply = new GetWindowsResult();
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

            var reply = new GetWindowsResult();
            if (!string.IsNullOrWhiteSpace(id))
            {
                reply.WindowIds.Add(id);
            }
            return reply;
        }

        public override async Task<ElementResult> GetElement(ElementQuery request, ServerCallContext context)
        {
            var reply = new ElementResult();
            await Application.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    FrameworkElement? searchRoot = GetParentElement();

                    if (searchRoot is null) return;

                    var window = searchRoot as Window ?? Window.GetWindow(searchRoot);
                    window.LogMessage("Getting element");

                    if (!string.IsNullOrWhiteSpace(request.Query))
                    {
                        if (!(EvaluateQuery(searchRoot, request.Query) is DependencyObject element))
                        {
                            reply.ErrorMessages.Add($"Failed to find element by query '{request.Query}' in '{searchRoot.GetType().FullName}'");
                            return;
                        }

                        string id = DependencyObjectTracker.GetOrSetId(element, KnownElements);
                        reply.ElementIds.Add(id);

                        window.LogMessage("Got element");
                        return;
                    }

                    reply.ErrorMessages.Add($"{nameof(ElementQuery)} did not specify a query");
                }
                catch (Exception e)
                {
                    reply.ErrorMessages.Add(e.ToString());
                }
            });
            return reply;

            FrameworkElement? GetParentElement()
            {
                if (!string.IsNullOrWhiteSpace(request.WindowId))
                {
                    Window? window = GetCachedElement<Window>(request.WindowId);
                    if (window is null)
                    {
                        reply!.ErrorMessages.Add("Failed to find parent window");
                    }
                    return window;
                }
                if (!string.IsNullOrWhiteSpace(request.ParentId))
                {
                    FrameworkElement? element = GetCachedElement<FrameworkElement>(request.ParentId);
                    if (element is null)
                    {
                        reply!.ErrorMessages.Add("Failed to find parent element");
                    }
                    return element;
                }
                reply!.ErrorMessages.Add("No parent element specified as part of the query");
                return null;
            }
        }

        public override async Task<PropertyResult> GetProperty(PropertyQuery request, ServerCallContext context)
        {
            var reply = new PropertyResult();
            await Application.Dispatcher.InvokeAsync(() =>
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
            });
            return reply;
        }

        public override async Task<EffectiveBackgroundResult> GetEffectiveBackground(EffectiveBackgroundQuery request, ServerCallContext context)
        {
            var reply = new EffectiveBackgroundResult();
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
            var reply = new PropertyResult();
            await Application.Dispatcher.InvokeAsync(() =>
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
                    PropertyDescriptor foundProperty = properties.Find(request.Name, false);
                    if (foundProperty is null)
                    {
                        reply.ErrorMessages.Add($"Could not find property with name '{request.Name}'");
                        return;
                    }

                    TypeConverter propertyTypeConverter = string.IsNullOrWhiteSpace(request.ValueType)
                        ? foundProperty.Converter
                        : TypeDescriptor.GetConverter(Type.GetType(request.ValueType));
                    value = GetValue(propertyTypeConverter);

                    foundProperty.SetValue(element, value);

                    //Re-retrive the value in case the dependency property coalesced it
                    value = foundProperty.GetValue(element);
                    propertyType = foundProperty.PropertyType;
                }

                SetValue(reply, propertyType, value);
            });
            return reply;

            object? GetValue(TypeConverter propertyConverter)
            {
                return request.ValueType switch
                {
                    Types.XamlString => LoadXaml<object>(request.Value),
                    _ => propertyConverter.ConvertFromString(request.Value),
                };
            }
        }

        private void SetValue(PropertyResult reply, Type propertyType, object? value)
        {
            reply.PropertyType = propertyType.AssemblyQualifiedName;
            Type valueType = value?.GetType() ?? propertyType;
            reply.ValueType = valueType.AssemblyQualifiedName;

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
            var reply = new CoordinatesResult();
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
                    var window = element as Window ?? Window.GetWindow(element);
                    Point windowOrigin = window.PointToScreen(new Point(0, 0));

                    Point topLeft = element.TranslatePoint(new Point(0, 0), window);
                    Point bottomRight = element.TranslatePoint(new Point(element.ActualWidth, element.ActualHeight), window);
                    reply.Left = windowOrigin.X + topLeft.X;
                    reply.Top = windowOrigin.Y + topLeft.Y;
                    reply.Right = windowOrigin.X + bottomRight.X;
                    reply.Bottom = windowOrigin.Y + bottomRight.Y;
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
            var reply = new ApplicationResult();
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

        public override async Task<WindowResult> CreateWindow(WindowConfiguration request, ServerCallContext context)
        {
            var reply = new WindowResult();
            await Application.Dispatcher.InvokeAsync(() =>
            {
                Window? window = null;
                if (!string.IsNullOrEmpty(request.WindowType))
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
                else
                {
                    try
                    {
                        window = LoadXaml<Window>(request.Xaml);
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
                        var windowRect = new Rect(window.Left, window.Top, window.Width, window.Height);
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

        public override async Task<ImageResult> GetScreenshot(ImageQuery request, ServerCallContext context)
        {
            var reply = new ImageResult();
            await Application.Dispatcher.InvokeAsync(async () =>
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

                using var screenBmp = new Bitmap((int)screen.Bounds.Width, (int)screen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using var bmpGraphics = Graphics.FromImage(screenBmp);
                bmpGraphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size((int)screen.Bounds.Width, (int)screen.Bounds.Height));
                using var ms = new MemoryStream();
                screenBmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                reply.Data = await ByteString.FromStreamAsync(ms);
            });
            return reply;
        }

        public override async Task<KeyboardFocusResult> MoveKeyboardFocus(KeyboardFocusRequest request, ServerCallContext context)
        {
            var reply = new KeyboardFocusResult();
            await Application.Dispatcher.InvokeAsync(() =>
            {
                if (!(GetCachedElement<DependencyObject>(request.ElementId) is IInputElement element))
                {
                    reply.ErrorMessages.Add("Could not find element");
                    return;
                }
                if (element is DependencyObject @do &&
                    Window.GetWindow(@do) is Window window)
                {
                    if (!ActivateWindow(window))
                    {
                        reply.ErrorMessages.Add($"Failed to activate window.");
                        return;
                    }
                }
                else
                {
                    reply.ErrorMessages.Add($"Failed to find parent window.");
                }

                if (Keyboard.Focus(element) != element)
                {
                    reply.ErrorMessages.Add($"Failed to move focus to element {element}");
                }
                if (element is UIElement uIElement)
                {
                    uIElement.Focus();
                }
            });
            return reply;
        }

        public override async Task<InputResponse> SendInput(InputRequest request, ServerCallContext context)
        {
            var reply = new InputResponse();
            IntPtr windowHandle = IntPtr.Zero;
            await Application.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (!(GetCachedElement<DependencyObject>(request.ElementId) is IInputElement element))
                    {
                        reply.ErrorMessages.Add("Could not find element");
                        return;
                    }

                    Window window = Window.GetWindow((DependencyObject)element);
                    if (window is null)
                    {
                        reply.ErrorMessages.Add("Failed to find parent window");
                        return;
                    }
                    windowHandle = new WindowInteropHelper(window).EnsureHandle();

                    if (!ActivateWindow(window))
                    {
                        reply.ErrorMessages.Add($"Failed to active window");
                        return;
                    }

                    if (Keyboard.Focus(element) != element)
                    {
                        reply.ErrorMessages.Add($"Failed to move focus to element {element}");
                        return;
                    }

                }
                catch (Exception e)
                {
                    reply.ErrorMessages.Add(e.ToString());
                }
            });

            if (reply.ErrorMessages.Any())
            {
                return reply;
            }

            try
            {
                if (windowHandle != IntPtr.Zero)
                {
                    foreach (KeyboardData keyboardData in request.KeyboardData)
                    {
                        if (!string.IsNullOrEmpty(keyboardData.TextInput))
                        {
                            Input.KeyboardInput.SendKeysForText(windowHandle, keyboardData.TextInput);
                        }
                        if (keyboardData.Keys.Any())
                        {
                            Input.KeyboardInput.SendKeys(windowHandle, keyboardData.Keys.Cast<Key>().ToArray());
                        }
                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
            return reply;
        }

        public override Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            var reply = new ShutdownResponse();
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

        public override Task<SerializerResponse> RegisterSerializer(SerializerRequest request, ServerCallContext context)
        {
            var reply = new SerializerResponse();
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

        private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            Assembly? found = LoadedAssemblies.FirstOrDefault(x => x.GetName().FullName == args.Name);
            if (found is { }) return found;

            var assemblyName = new AssemblyName(args.Name!);
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

        private object? EvaluateQuery(DependencyObject root, string query)
        {
            object? result = null;
            List<string> errorParts = new List<string>();
            DependencyObject? current = root;

            while (query.Length > 0)
            {
                if (current is null)
                {
                    throw new Exception($"Could not resolve '{query}' on null element");
                }

                switch (GetNextQueryType(ref query, out string value))
                {
                    case QueryPartType.Name:
                        result = EvaluateNameQuery(current, value);
                        break;
                    case QueryPartType.Property:
                        result = EvaluatePropertyQuery(current, value);
                        break;
                    case QueryPartType.ChildType:
                        result = EvaluateChildTypeQuery(current, value);
                        break;
                }
                current = result as DependencyObject;
            }

            return result;

            static QueryPartType GetNextQueryType(ref string query, out string value)
            {
                var regex = new Regex(@"(?<=.)[\.\/\~]");

                Match match = regex.Match(query);

                string currentQuery = query;
                if (match.Success)
                {
                    currentQuery = query.Substring(0, match.Index);
                    query = query[match.Index..];
                }
                else
                {
                    query = "";
                }

                QueryPartType rv;
                if (currentQuery.StartsWith('.'))
                {
                    value = currentQuery[1..];
                    rv = QueryPartType.Property;
                }
                else if (currentQuery.StartsWith('/'))
                {
                    value = currentQuery[1..];
                    rv = QueryPartType.ChildType;
                }
                else
                {
                    if (currentQuery.StartsWith('~'))
                    {
                        value = currentQuery[1..];
                    }
                    else
                    {
                        value = currentQuery;
                    }
                    rv = QueryPartType.Name;
                }
                return rv;
            }

            static object? EvaluateNameQuery(DependencyObject root, string name)
            {
                return Decendants<FrameworkElement>(root).FirstOrDefault(x => x.Name == name);
            }

            static object EvaluatePropertyQuery(DependencyObject root, string property)
            {
                var properties = TypeDescriptor.GetProperties(root);
                if (properties.Find(property, false) is PropertyDescriptor propertyDescriptor)
                {
                    return propertyDescriptor.GetValue(root);
                }
                throw new Exception($"Failed to find property '{property}' on element of type '{root.GetType().FullName}'");
            }

            static object EvaluateChildTypeQuery(DependencyObject root, string childTypeQuery)
            {
                var indexerRegex = new Regex(@"\[(?<Index>\d+)]$");

                int index = 0;
                Match match = indexerRegex.Match(childTypeQuery);
                if (match.Success)
                {
                    index = int.Parse(match.Groups["Index"].Value);
                    childTypeQuery = childTypeQuery.Substring(0, match.Index);
                }

                foreach (DependencyObject child in Decendants<DependencyObject>(root))
                {
                    if (child.GetType().Name == childTypeQuery)
                    {
                        if (index == 0)
                        {
                            return child;
                        }
                        index--;
                    }
                }
                throw new Exception($"Failed to find child of type '{childTypeQuery}'");
            }
        }

        private enum QueryPartType
        {
            None,
            Name,
            Property,
            ChildType
        }

        private T LoadXaml<T>(string xaml) where T : class
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
            return (T)XamlReader.Load(memoryStream);
        }

        private static IEnumerable<T> Decendants<T>(DependencyObject? parent)
            where T : DependencyObject
        {
            if (parent is null) yield break;

            var queue = new Queue<DependencyObject>();
            Enqueue(GetChildren(parent));

            if (parent is UIElement parentVisual &&
                AdornerLayer.GetAdornerLayer(parentVisual) is { } layer &&
                layer.GetAdorners(parentVisual) is { } adorners &&
                adorners.Length > 0)
            {
                Enqueue(adorners);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current is T match) yield return match;

                Enqueue(GetChildren(current));
            }

            static IEnumerable<DependencyObject> GetChildren(DependencyObject item)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(item);
                for (int i = 0; i < childrenCount; i++)
                {
                    if (VisualTreeHelper.GetChild(item, i) is DependencyObject child)
                    {
                        yield return child;
                    }
                }
                if (childrenCount == 0)
                {
                    foreach (object? logicalChild in LogicalTreeHelper.GetChildren(item))
                    {
                        if (logicalChild is DependencyObject child)
                        {
                            yield return child;
                        }
                    }
                }
            }

            void Enqueue(IEnumerable<DependencyObject> items)
            {
                foreach (var item in items)
                {
                    queue!.Enqueue(item);
                }
            }
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

        private TElement? GetCachedElement<TElement>(string? id)
            where TElement : DependencyObject
        {
            if (string.IsNullOrWhiteSpace(id)) return default;
            lock (KnownElements)
            {
                if (KnownElements.TryGetValue(id, out WeakReference<DependencyObject>? weakRef) &&
                    weakRef.TryGetTarget(out DependencyObject? element))
                {
                    return element as TElement;
                }
            }
            return null;
        }

        private static bool ActivateWindow(Window window)
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
            foreach (Point clickPoint in GetClickPoints(window))
            {
                MouseInput.MoveCursor(clickPoint);
                MouseInput.LeftClick();

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
                yield return new Point(window.Left + 1, window.Top + 1);

                // Bottom right
                yield return new Point(window.Left + window.Width - 1, window.Top + window.Height - 1);

                // Bottom left
                yield return new Point(window.Left + 1, window.Top + window.Height - 1);

                // Center
                yield return new Point(window.Left + window.Width / 2, window.Top + window.Height / 2);
            }
        }
    }
}
