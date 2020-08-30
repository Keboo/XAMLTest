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
                        object value = element.GetValue(dependencyProperty);
                        reply.PropertyType = dependencyProperty.PropertyType.AssemblyQualifiedName;
                        SetValue(reply, value);
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

                    object value = foundProperty.GetValue(element);
                    reply.PropertyType = foundProperty.PropertyType.AssemblyQualifiedName;
                    SetValue(reply, value);
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
                        reply.PropertyType = dependencyProperty.PropertyType.AssemblyQualifiedName;
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
                    reply.PropertyType = foundProperty.PropertyType.AssemblyQualifiedName;
                }

                SetValue(reply, value);
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

        // assume that reply.PropertyType is already set.
        private static void SetValue(PropertyResult reply, object value)
        {
            reply.ValueType = value?.GetType().AssemblyQualifiedName ?? reply.PropertyType;
            reply.Value = value?.ToString() ?? string.Empty;
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

                    reply.Value = resourceValue?.ToString() ?? "";
                    reply.ValueType = resourceValue?.GetType().AssemblyQualifiedName ?? "";
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
                                LoadedAssemblies.Add(Assembly.LoadFile(assembly));
                            }
                            else
                            {
                                reply.ErrorMessages.Add("Assemblies names must not be null");
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
                try
                {
                    window = LoadXaml<Window>(request.Xaml);
                    window.Activated += Window_Activated;
                    window.Deactivated += Window_Deactivated;
                }
                catch (Exception e)
                {
                    reply.ErrorMessages.Add($"Error loading window{Environment.NewLine}{e}");
                }
                if (window is { })
                {
                    reply.WindowsId = DependencyObjectTracker.GetOrSetId(window, KnownElements);
                    window.Show();
                    window.LogMessage("Window shown");
                    
                    if (!window.Activate())
                    {
                        reply.ErrorMessages.Add("Failed to activate window");
                        return;
                    }

                    if (request.FitToScreen)
                    {
                        var windowRect = new Rect(window.Left, window.Top, window.Width, window.Height);
                        Screen screen = Screen.FromRect(windowRect);
                        if (!screen.WorkingArea.Contains(windowRect))
                        {
                            window.Left = Math.Max(window.Left, screen.WorkingArea.Left);
                            window.Left = Math.Max(screen.WorkingArea.Left, window.Left + window.Width - screen.WorkingArea.Right - window.Width);

                            window.Top = Math.Max(window.Top, screen.WorkingArea.Top);
                            window.Top = Math.Max(screen.WorkingArea.Top, window.Top + window.Height - screen.WorkingArea.Top - window.Height);

                            window.Width = Math.Min(window.Width, screen.WorkingArea.Width);
                            window.Height = Math.Min(window.Height, screen.WorkingArea.Height);
                        }
                    }

                    if (!window.IsActive)
                    {
                        reply.ErrorMessages.Add("Window not active");
                    }
                    else
                    {
                        window.LogMessage("Window is active");
                    }
                }
                else
                {
                    reply.ErrorMessages.Add("Failed to load window");
                }
            });
            return reply;
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.LogMessage("Window activated");
            }
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.LogMessage("Window deactivated");
            }
        }

        public override async Task<ImageResult> GetImage(ImageQuery request, ServerCallContext context)
        {
            var reply = new ImageResult();
            await Application.Dispatcher.InvokeAsync(async () =>
            {
                FrameworkElement? element = GetCachedElement<FrameworkElement>(request.ElementId);
                if (element is null)
                {
                    reply.ErrorMessages.Add("Could not find element");
                    return;
                }

                Point topLeft = element.PointToScreen(new Point(0, 0));
                
                var screen = Screen.FromRect(new Rect(topLeft.X, topLeft.Y, element.ActualWidth, element.ActualHeight));

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
                    window.LogMessage("Activating window");
                    if (!window.Activate())
                    {
                        var foregroupWindowPtr = PInvoke.User32.GetForegroundWindow();
                        PInvoke.User32.GetWindowThreadProcessId(foregroupWindowPtr, out int processId);
                        Process foregroundProcess = Process.GetProcessById(processId);

                        reply.ErrorMessages.Add($"Failed to activate window. Foreground window '{foregroundProcess.MainWindowTitle}', PID {processId}, Name: {foregroundProcess.ProcessName}");
                        reply.ErrorMessages.AddRange(window.GetLogMessages());
                        return;
                    }
                    if (!window.IsActive)
                    {
                        reply.ErrorMessages.Add($"Window is not active");
                        return;
                    }
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
            int expectedKeyPresses = 0;
            var upKeys = new List<ulong>();
            var messages = new List<WindowMessage>();
            var hook = new HwndSourceHook(WndProc);
            HwndSource? source = null;
            await Application.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (!(GetCachedElement<DependencyObject>(request.ElementId) is IInputElement element))
                    {
                        reply.ErrorMessages.Add("Could not find element");
                        return;
                    }
                    if (Keyboard.Focus(element) != element)
                    {
                        reply.ErrorMessages.Add($"Failed to move focus to element {element}");
                        return;
                    }

                    Window window = Window.GetWindow((DependencyObject)element);
                    if (window is null)
                    {
                        reply.ErrorMessages.Add("Failed to find parent window");
                        return;
                    }
                    source = HwndSource.FromHwnd(new WindowInteropHelper(window).EnsureHandle());
                    source.AddHook(hook);
                    if (!window.Activate() || !window.IsActive)
                    {
                        reply.ErrorMessages.Add($"Failed to active window");
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
                if (!string.IsNullOrEmpty(request.TextInput))
                {
                    expectedKeyPresses += request.TextInput.Length;
                    Input.KeyboardInput.SendKeysForText(request.TextInput);
                }
                if (request.Keys.Any())
                {
                    expectedKeyPresses += request.Keys.Count;
                    Input.KeyboardInput.SendKeys(request.Keys.Cast<Key>().ToArray());

                }

                using var cts = new CancellationTokenSource();
                //Only wait for 1 second for the key presses to be processed by the window.
                cts.CancelAfter(TimeSpan.FromSeconds(1));

                await Task.Run(() =>
                {
                    CancellationToken token = cts.Token;
                    while (expectedKeyPresses != upKeys.Count && !token.IsCancellationRequested)
                    { }
                });

                if (source != null && hook != null)
                {
                    source.RemoveHook(hook);
                }

                if (expectedKeyPresses != upKeys.Count)
                {
                    reply.ErrorMessages.Add($"Failed to send keys to expected window. Expected {expectedKeyPresses}, Keys {upKeys.Count}");
                    reply.ErrorMessages.Add($"Keys: {string.Join(",", upKeys)}");
                    reply.ErrorMessages.Add($"Messages: {string.Join(",", messages)}");
                }
            }
            catch(Exception e)
            {
                reply.ErrorMessages.Add(e.ToString());
            }
            return reply;

            IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                messages.Add((WindowMessage)msg);
                switch ((WindowMessage)msg)
                {
                    /*
                    //NB: Currently sending the Enter key only appears to trigger WM_KEYUP
                    case WindowMessage.WM_SYSKEYDOWN:
                    case WindowMessage.WM_KEYDOWN:
                    case WindowMessage.WM_IME_KEYDOWN:
                        // https://docs.microsoft.com/windows/win32/inputdev/wm-keydown#remarks
                        // Because of the autorepeat feature, more than one WM_KEYDOWN message may be 
                        // posted before a WM_KEYUP message is posted. The previous key state (bit 30) 
                        // can be used to determine whether the WM_KEYDOWN message indicates the first 
                        // down transition or a repeated down transition.
                        downKeys.Add((ulong)wParam.ToInt64());
                        if ((wParam.ToInt32() & 0x0400_0000) == 0)
                        {
                            Interlocked.Increment(ref keyDowns);
                        }
                        break;
                    */
                    case WindowMessage.WM_SYSKEYUP:
                    case WindowMessage.WM_KEYUP:
                    case WindowMessage.WM_IME_KEYUP:
                        upKeys.Add((ulong)wParam.ToInt64());
                        break;
                }
                return IntPtr.Zero;
            }
        }

        private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            Assembly? found = LoadedAssemblies.FirstOrDefault(x => x.GetName().FullName == args.Name);
            if (found is { }) return found;

            var assemblyName = new AssemblyName(args.Name!);
            string likelyAssemblyPath = Path.GetFullPath($"{assemblyName.Name}.dll");
            try
            {
                if (File.Exists(likelyAssemblyPath) && Assembly.LoadFile(likelyAssemblyPath) is Assembly localAssemby)
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

            static object EvaluateNameQuery(DependencyObject root, string name)
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
    }
}
