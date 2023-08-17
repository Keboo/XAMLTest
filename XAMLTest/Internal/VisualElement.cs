using System.Windows.Media;
using XamlTest.Host;
using XamlTest.Input;

namespace XamlTest.Internal;

internal class VisualElement<T> : IVisualElement, IVisualElement<T>, IElementId
{
    public VisualElement(
        Protocol.ProtocolClient client,
        string id,
        AppContext context,
        Action<string>? logMessage)
        : this(client, id, typeof(T), context, logMessage)
    {
    }

    public VisualElement(
        Protocol.ProtocolClient client,
        string id,
        Type type,
        AppContext context,
        Action<string>? logMessage)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        LogMessage = logMessage;
    }

    private AppContext Context { get; }
    private Serializer Serializer => Context.Serializer;
    private Protocol.ProtocolClient Client { get; }

    public string Id { get; }
    public Type Type { get; }
    public Action<string>? LogMessage { get; }

    public Task<IVisualElement> GetElement(string query)
        => GetElement(query, null);

    public async Task<IVisualElement<TElement>> GetElement<TElement>(string query)
        => (IVisualElement<TElement>)await GetElement(query, typeof(TElement));

    private async Task<IVisualElement> GetElement(string query, Type? desiredType)
    {
        LogMessage?.Invoke($"{nameof(GetElement)}({query})");
        Host.ElementQuery elementQuery = GetFindElementQuery(query);
        return (await GetElementQuery(elementQuery, desiredType)) ?? throw new XamlTestException($"Did not find element matching query {query}");
    }

    public Task<IVisualElement?> FindElement(string query)
        => FindElement(query, null);

    public async Task<IVisualElement<TElement>?> FindElement<TElement>(string query) 
        => (IVisualElement<TElement>?)await FindElement(query, typeof(TElement));

    private Task<IVisualElement?> FindElement(string query, Type? desiredType)
    {
        LogMessage?.Invoke($"{nameof(FindElement)}({query})");
        Host.ElementQuery elementQuery = GetFindElementQuery(query);
        elementQuery.IgnoreMissing = true;
        return GetElementQuery(elementQuery, desiredType);
    }

    private async Task<IVisualElement?> GetElementQuery(Host.ElementQuery elementQuery, Type? desiredType)
    {
        if (await Client.GetElementAsync(elementQuery) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (reply.Elements.Count == 1)
            {
                Element element = reply.Elements[0];
                if (Type.GetType(element.Type) is { } elementType)
                {
                    if (desiredType is null)
                    {
                        return Create(Client, element.Id, elementType, Context, LogMessage);
                    }
                    if (desiredType != elementType &&
                        !elementType.IsSubclassOf(desiredType))
                    {
                        throw new XamlTestException($"Element of type '{element.Type}' does not match desired type '{desiredType.AssemblyQualifiedName}'");
                    }
                    return Create(Client, element.Id, desiredType, Context, LogMessage);
                }
            }
            return null;
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<IValue> GetProperty(string name, string? ownerType)
    {
        PropertyQuery propertyQuery = new()
        {
            ElementId = Id,
            Name = name,
            OwnerType = ownerType ?? ""
        };
        LogMessage?.Invoke($"{nameof(GetProperty)}({name}{(!string.IsNullOrEmpty(ownerType) ? "," : "")}{ownerType})");
        if (await Client.GetPropertyAsync(propertyQuery) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (reply.PropertyType is { } propertyType)
            {
                IVisualElement? visualElement = null;
                if (reply.Element is { } element &&
                    !string.IsNullOrEmpty(element.Type))
                {
                    Type? elementType = Type.GetType(element.Type);
                    if (elementType is null)
                    {
                        throw new XamlTestException($"Could not find element type '{element.Type}'");
                    }
                    visualElement = Create(Client, element.Id, elementType, Context, LogMessage);
                }
                return new Property(propertyType, reply.ValueType, reply.Value, visualElement, Context);
            }
            throw new XamlTestException("Property does not have a type specified");
        }
        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<IValue> SetProperty(string name, string value, string? valueType, string? ownerType)
    {
        SetPropertyRequest query = new()
        {
            ElementId = Id,
            Name = name,
            Value = value,
            ValueType = valueType,
            OwnerType = ownerType ?? ""
        };
        LogMessage?.Invoke($"{nameof(SetProperty)}({name},{value},{valueType},{ownerType})");
        if (await Client.SetPropertyAsync(query) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (reply.PropertyType is { } propertyType)
            {
                return new Property(propertyType, reply.ValueType, reply.Value, null, Context);
            }
            throw new XamlTestException("Property reply does not have a type specified");
        }
        throw new XamlTestException("Failed to receive a reply");
    }

    public Task<IVisualElement> SetXamlProperty(string propertyName, XamlSegment xaml)
        => SetXamlProperty(propertyName, xaml, null);

    public async Task<IVisualElement<TElement>> SetXamlProperty<TElement>(string propertyName, XamlSegment xaml)
        => (IVisualElement<TElement>)await SetXamlProperty(propertyName, xaml, typeof(TElement));

    private async Task<IVisualElement> SetXamlProperty(string propertyName, XamlSegment xaml, Type? desiredType)
    {
        SetXamlPropertyRequest request = new()
        {
            ElementId = Id,
            Name = propertyName,
            Xaml = xaml.Xaml
            //OwnerType = "???"
        };
        var namespaces = xaml.Namespaces.Union(Context.DefaultNamespaces).Distinct();
        request.Namespaces.AddRange(namespaces.Select(x => new XamlNamespace
        {
            Prefix = x.Prefix,
            Uri = x.Uri
        }));
        LogMessage?.Invoke($"{nameof(SetXamlProperty)}({propertyName},{xaml})");
        if (await Client.SetXamlPropertyAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (reply.Elements.Count == 1)
            {
                Element element = reply.Elements[0];
                if (Type.GetType(element.Type) is { } elementType)
                {
                    if (desiredType is null)
                    {
                        return Create(Client, element.Id, elementType, Context, LogMessage);
                    }
                    if (desiredType != elementType &&
                        !elementType.IsSubclassOf(desiredType))
                    {
                        throw new XamlTestException($"Element of type '{element.Type}' does not match desired type '{desiredType.AssemblyQualifiedName}'");
                    }
                    return Create(Client, element.Id, desiredType, Context, LogMessage);
                }
                throw new XamlTestException($"Could not find element type '{element.Type}'");
            }
            throw new XamlTestException("XAML did not contain any elements");
        }
        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<IResource> GetResource(string key)
    {
        ResourceQuery query = new()
        {
            ElementId = Id,
            Key = key
        };
        LogMessage?.Invoke($"{nameof(GetResource)}({key})");
        if (await Client.GetResourceAsync(query) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (!string.IsNullOrWhiteSpace(reply.ValueType))
            {
                return new Resource(reply.Key, reply.ValueType, reply.Value, Context);
            }
            throw new XamlTestException($"Resource with key '{reply.Key}' not found");
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<Color> GetEffectiveBackground(IVisualElement? toElement)
    {
        string? toElementId = (toElement as IElementId)?.Id;

        EffectiveBackgroundQuery propertyQuery = new()
        {
            ElementId = Id,
            ToElementId = toElementId ?? ""
        };
        LogMessage?.Invoke($"{nameof(GetEffectiveBackground)}()");
        if (await Client.GetEffectiveBackgroundAsync(propertyQuery) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return Color.FromArgb((byte)reply.Alpha, (byte)reply.Red, (byte)reply.Green, (byte)reply.Blue);
        }
        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<Rect> GetCoordinates()
    {
        CoordinatesQuery query = new()
        {
            ElementId = Id
        };
        LogMessage?.Invoke($"{nameof(GetCoordinates)}()");
        if (await Client.GetCoordinatesAsync(query) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return new Rect(reply.Left, reply.Top, reply.Right - reply.Left, reply.Bottom - reply.Top);
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task MoveKeyboardFocus()
    {
        KeyboardFocusRequest request = new()
        {
            ElementId = Id
        };

        LogMessage?.Invoke($"{nameof(MoveKeyboardFocus)}()");
        if (await Client.MoveKeyboardFocusAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return;
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task SendInput(KeyboardInput keyboardInput)
    {
        if (keyboardInput is null)
        {
            throw new ArgumentNullException(nameof(keyboardInput));
        }

        InputRequest request = new()
        {
            ElementId = Id
        };
        request.KeyboardData.AddRange(keyboardInput.Inputs.Select(i =>
        {
            KeyboardData rv = new();
            switch (i)
            {
                case ModifiersInput modifiersInput:
                    rv.Modifiers.Add((int)modifiersInput.Modifiers);
                    break;
                case KeysInput keysInput:
                    rv.Keys.AddRange(keysInput.Keys.Cast<int>());
                    break;
                case TextInput textInput:
                    rv.TextInput = textInput.Text;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown input type {i.GetType().FullName}");
            }
            return rv;
        }));
        LogMessage?.Invoke($"{nameof(SendInput)}({keyboardInput})");
        if (await Client.SendInputAsync(request) is { } reply)
        {
            if (reply.LogMessages.Any() && LogMessage is { } logMessage)
            {
                foreach (var message in reply.LogMessages)
                {
                    logMessage(message);
                }
            }
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return;
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<Point> SendInput(MouseInput mouseInput)
    {
        if (mouseInput is null)
        {
            throw new ArgumentNullException(nameof(mouseInput));
        }

        InputRequest request = new()
        {
            ElementId = Id
        };
        request.MouseData.AddRange(mouseInput.Inputs.SelectMany(GetAll));
        LogMessage?.Invoke($"{nameof(SendInput)}({mouseInput})");
        if (await Client.SendInputAsync(request) is { } reply)
        {
            if (reply.LogMessages.Any() && LogMessage is { } logMessage)
            {
                foreach (var message in reply.LogMessages)
                {
                    logMessage(message);
                }
            }
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return new Point(reply.CursorX, reply.CursorY);
        }

        throw new XamlTestException("Failed to receive a reply");

        static IEnumerable<MouseData> GetAll(IInput input)
        {
            switch (input)
            {
                case MouseInput.MouseInputData data:
                    yield return GetData(data);
                    break;
                case MouseInput mouseInput:
                    foreach (MouseData item in mouseInput.Inputs.SelectMany(x => GetAll(x)))
                    {
                        yield return item;
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown input type {input.GetType().FullName}");
            }
        }

        static MouseData GetData(MouseInput.MouseInputData inputData)
        {
            return new MouseData
            {
                Event = inputData.Event,
                Value = inputData.Value ?? ""
            };
        }
    }

    public async Task<IEventRegistration> RegisterForEvent(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        EventRegistrationRequest request = new()
        {
            ElementId = Id,
            EventName = name
        };

        LogMessage?.Invoke($"{nameof(RegisterForEvent)}({name})");
        if (await Client.RegisterForEventAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return new EventRegistration(Client, reply.EventId, name, Serializer, LogMessage);
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task UnregisterEvent(IEventRegistration eventRegistration)
    {
        if (eventRegistration is null)
        {
            throw new ArgumentNullException(nameof(eventRegistration));
        }
        if (eventRegistration is not EventRegistration registration)
        {
            throw new ArgumentException($"Registration is not an {nameof(EventRegistration)}", nameof(eventRegistration));
        }

        EventUnregisterRequest request = new()
        {
            EventId = registration.EventId
        };

        LogMessage?.Invoke($"{nameof(UnregisterEvent)}({eventRegistration})");
        if (await Client.UnregisterForEventAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return;
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    protected virtual Host.ElementQuery GetFindElementQuery(string query)
        => new()
        {
            ParentId = Id,
            Query = query
        };

    public bool Equals([AllowNull] IVisualElement other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other is IElementId visualElement)
        {
            return Id == visualElement.Id;
        }
        return false;
    }

    public override bool Equals([AllowNull] object other)
        => Equals(other as IVisualElement);

    public override int GetHashCode()
        => Id.GetHashCode();

    private static IVisualElement Create(
        Protocol.ProtocolClient client,
        string id,
        Type type,
        AppContext context,
        Action<string>? logMessage,
        Type? visualElementType = null)
    {
        ConstructorInfo ctor = typeof(VisualElement<>)
            .MakeGenericType(visualElementType ?? type)
            .GetConstructors()
            .Single(x =>
            {
                return x.GetParameters()
                .Select(x => x.ParameterType)
                .SequenceEqual(new[]
                {
                    typeof(Protocol.ProtocolClient),
                    typeof(string),
                    typeof(Type),
                    typeof(AppContext),
                    typeof(Action<string>)
                });
            });
        return (IVisualElement)ctor.Invoke(new object?[] { client, id, type, context, logMessage });
    }

    private TVisualElement Convert<TVisualElement>()
    {
        if (this is TVisualElement current)
        {
            return current;
        }
        var targetType = typeof(TVisualElement);
        if (targetType.IsGenericType &&
            targetType.GetGenericTypeDefinition() == typeof(IVisualElement<>))
        {
            Type elementType = targetType.GetGenericArguments()[0];
            if (GetValidTypes(Type).Contains(elementType))
            {
                return (TVisualElement)Create(Client, Id, Type, Context, LogMessage, elementType);
            }
        }
        throw new InvalidOperationException($"Cannot convert {typeof(IVisualElement<T>)} to {typeof(IVisualElement)}");

        static IEnumerable<Type> GetValidTypes(Type type)
        {
            for (Type? t = type;
                t != null;
                t = t.BaseType)
            {
                yield return t;
            }
            foreach (Type interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }
    }

    public IVisualElement<TElement> As<TElement>() where TElement : DependencyObject
        => Convert<IVisualElement<TElement>>();
    
    public async Task<TReturn?> RemoteExecute<TReturn>(Delegate @delegate, object?[] parameters)
    {
        if (@delegate.Target is not null)
        {
            throw new ArgumentException("Cannot execute a non-static delegate remotely");
        }
        if (@delegate.Method.DeclaringType is null)
        {
            throw new ArgumentException("Could not find containing type for delegate");
        }

        var request = new RemoteInvocationRequest()
        {
            ElementId = Id,
            MethodName = @delegate.Method.Name,
            MethodContainerType = @delegate.Method.DeclaringType!.AssemblyQualifiedName,
            Assembly = @delegate.Method.DeclaringType.Assembly.FullName,
        };
        foreach (var parameter in parameters)
        {
            request.Parameters.Add(Serializer.Serialize(parameter?.GetType() ?? typeof(object), parameter));
        }
        if (@delegate.Method.IsGenericMethod)
        {
            foreach(var genericArguments in @delegate.Method.GetGenericArguments())
            {
                request.MethodGenericTypes.Add(genericArguments.AssemblyQualifiedName);
            }
        }
        LogMessage?.Invoke($"{nameof(RemoteExecute)}({request})");
        if (await Client.RemoteInvocationAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }

            if (reply.ValueType is null)
            {
                return default;
            }

            if (reply.Value is TReturn converted && typeof(TReturn) != typeof(string))
            {
                return converted;
            }

            return (TReturn)Serializer.Deserialize(typeof(TReturn), reply.Value ?? "")!;
        }
        return default;
    }

    public async Task Highlight(HighlightConfig highlightConfig)
    {
        if (highlightConfig is null)
        {
            throw new ArgumentNullException(nameof(highlightConfig));
        }

        var request = new HighlightRequest()
        {
            ElementId = Id,
            BorderBrush = Serializer.Serialize(typeof(Brush), highlightConfig.BorderBrush),
            BorderThickness = highlightConfig.BorderThickness,
            OverlayBrush = Serializer.Serialize(typeof(Brush), highlightConfig.OverlayBrush),
            IsVisible = highlightConfig.IsVisible
        };
        if (await Client.HighlightElementAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return;
        }
    }

}
