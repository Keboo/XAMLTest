using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using XamlTest.Event;
using XamlTest.Input;
using XamlTest.Internal;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace XamlTest.Host
{
    internal partial class VisualTreeService : Protocol.ProtocolBase
    {
        private static Guid Initialized { get; } = Guid.NewGuid();

        private List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

        private Application Application { get; }

        private Serializer Serializer { get; } = new Serializer();

        private Dictionary<string, WeakReference<DependencyObject>> KnownElements { get; } = new();

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

        public override async Task<ElementResult> GetElement(ElementQuery request, ServerCallContext context)
        {
            ElementResult reply = new();
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

                        reply.Elements.Add(GetElement(element));

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
            WindowResult reply = new();
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
                bmpGraphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size((int)screen.Bounds.Width, (int)screen.Bounds.Height));
                using MemoryStream ms = new();
                screenBmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                reply.Data = await ByteString.FromStreamAsync(ms);
            });
            return reply;
        }

        public override Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
        {
            ShutdownResponse reply = new();
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
            SerializerResponse reply = new();
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

        private static object? EvaluateQuery(DependencyObject root, string query)
        {
            object? result = null;
            List<string> errorParts = new();
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
                    case QueryPartType.PropertyExpression:
                        result = EvaluatePropertyExpressionQuery(current, value);
                        break;
                }
                current = result as DependencyObject;
            }

            return result;

            static QueryPartType GetNextQueryType(ref string query, out string value)
            {
                Regex propertyExpressionRegex = new(@"(?<=^\[[^=\]]+=[^=\]]+)\]");
                Regex regex = new(@"(?<=.)[\.\/\~]");

                string currentQuery = query;
                if (propertyExpressionRegex.Match(query) is { } propertyExpressionMatch &&
                    propertyExpressionMatch.Success)
                {
                    currentQuery = query.Substring(0, propertyExpressionMatch.Index + 1);
                    query = query[(propertyExpressionMatch.Index + 1)..];
                }
                else if (regex.Match(query) is { } match &&
                    match.Success)
                {
                    currentQuery = query.Substring(0, match.Index);
                    query = query[match.Index..];
                }
                else
                {
                    query = "";
                }

                QueryPartType rv;
                if (currentQuery.StartsWith('[') && currentQuery.EndsWith(']'))
                {
                    value = currentQuery[1..^1];
                    rv = QueryPartType.PropertyExpression;
                }
                else if (currentQuery.StartsWith('.'))
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
                Regex indexerRegex = new(@"\[(?<Index>\d+)]$");

                int index = 0;
                Match match = indexerRegex.Match(childTypeQuery);
                if (match.Success)
                {
                    index = int.Parse(match.Groups["Index"].Value);
                    childTypeQuery = childTypeQuery.Substring(0, match.Index);
                }

                foreach (DependencyObject child in Decendants<DependencyObject>(root))
                {
                    if (GetTypeNames(child).Any(x => x == childTypeQuery))
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

            static object EvaluatePropertyExpressionQuery(DependencyObject root, string propertyExpression)
            {
                var parts = propertyExpression.Split('=');
                string property = parts[0].TrimEnd();
                string propertyValueString = parts[1].Trim('"');

                foreach (DependencyObject child in Decendants<DependencyObject>(root))
                {
                    var properties = TypeDescriptor.GetProperties(child);
                    if (properties.Find(property, false) is PropertyDescriptor propertyDescriptor)
                    {
                        var value = propertyDescriptor.GetValue(child)?.ToString();
                        //TODO: More advanced comparison
                        if (string.Equals(value, propertyValueString))
                        {
                            return child;
                        }
                    }
                }
                throw new Exception($"Failed to find child with property expression '{propertyExpression}'");
            }

            static IEnumerable<string> GetTypeNames(DependencyObject child)
            {
                for(Type? type = child.GetType();
                    type is not null;
                    type = type.BaseType)
                {
                    yield return type.Name;
                }
            }
        }

        private enum QueryPartType
        {
            None,
            Name,
            Property,
            ChildType,
            PropertyExpression
        }

        private static T LoadXaml<T>(string xaml) where T : class
        {
            using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(xaml));
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
                if (item is FrameworkElement fe)
                {
                    if (fe.ContextMenu is { } contextMenu)
                    {
                        yield return contextMenu;
                    }
                    if (fe.ToolTip as DependencyObject is { } toolTip )
                    {
                        yield return toolTip;
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

        private Element GetElement(DependencyObject? element)
        {
            Element rv = new();
            if (element is not null &&
                (element is not Freezable freeze || !freeze.IsFrozen))
            {
                rv.Id = DependencyObjectTracker.GetOrSetId(element, KnownElements);
                rv.Type = element.GetType().AssemblyQualifiedName;
            }
            return rv;
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
                Input.MouseInput.MoveCursor(clickPoint);
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
                yield return new Point(window.Left + 1, window.Top + 1);

                // Bottom right
                yield return new Point(window.Left + window.Width - 1, window.Top + window.Height - 1);

                // Bottom left
                yield return new Point(window.Left + 1, window.Top + window.Height - 1);

                // Center
                yield return new Point(window.Left + window.Width / 2, window.Top + window.Height / 2);
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

            return new Rect(rvleft, rvtop, rvright - rvleft, rvbottom - rvtop);
        }
    }
}
