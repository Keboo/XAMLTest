using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace XamlTest.Internal
{
    internal class VisualElement : IVisualElement
    {
        public VisualElement(Protocol.ProtocolClient client, string id, 
            Serializer serializer, Action<string>? logMessage)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            LogMessage = logMessage;
        }

        private Serializer Serializer { get; }
        private Protocol.ProtocolClient Client { get; }

        protected string Id { get; }
        public Action<string>? LogMessage { get; }

        public async Task<IVisualElement> GetElement(string query)
        {
            ElementQuery elementQuery = GetFindElementQuery(query);
            LogMessage?.Invoke($"{nameof(GetElement)}({query})");
            if (await Client.GetElementAsync(elementQuery) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                if (reply.ElementIds.Count == 1)
                {
                    return new VisualElement(Client, reply.ElementIds[0], Serializer, LogMessage);
                }
                throw new Exception($"Found {reply.ElementIds.Count} elements");
            }

            throw new Exception("Failed to receive a reply");
        }

        public async Task<IValue> GetProperty(string name, string? ownerType)
        {
            PropertyQuery propertyQuery = new()
            {
                ElementId = Id,
                Name = name,
                OwnerType = ownerType ?? ""
            };
            LogMessage?.Invoke($"{nameof(GetProperty)}({name},{ownerType})");
            if (await Client.GetPropertyAsync(propertyQuery) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                if (reply.PropertyType is { } propertyType)
                {
                    return new Property(propertyType, reply.ValueType, reply.Value, Serializer);
                }
                throw new Exception("Property does not have a type specified");
            }
            throw new Exception("Failed to receive a reply");
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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                if (reply.PropertyType is { } propertyType)
                {
                    return new Property(propertyType, reply.ValueType, reply.Value, Serializer);
                }
                throw new Exception("Property reply does not have a type specified");
            }
            throw new Exception("Failed to receive a reply");
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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                if (!string.IsNullOrWhiteSpace(reply.ValueType))
                {
                    return new Resource(reply.Key, reply.ValueType, reply.Value, Serializer);
                }
                throw new Exception($"Resource with key '{reply.Key}' not found");
            }

            throw new Exception("Failed to receive a reply");
        }

        public async Task<Color> GetEffectiveBackground(IVisualElement? toElement)
        {
            string? toElementId = (toElement as VisualElement)?.Id;

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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return Color.FromArgb((byte)reply.Alpha, (byte)reply.Red, (byte)reply.Green, (byte)reply.Blue);
            }
            throw new Exception("Failed to receive a reply");
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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new Rect(reply.Left, reply.Top, reply.Right - reply.Left, reply.Bottom - reply.Top);
            }

            throw new Exception("Failed to receive a reply");
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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }

            throw new Exception("Failed to receive a reply");
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
                switch(i)
                {
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
                    foreach(var message in reply.LogMessages)
                    {
                        logMessage(message);
                    }
                }
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }

            throw new Exception("Failed to receive a reply");
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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new EventRegistration(Client, reply.EventId, name, Serializer, LogMessage);
            }

            throw new Exception("Failed to receive a reply");
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
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }

            throw new Exception("Failed to receive a reply");
        }

        protected virtual ElementQuery GetFindElementQuery(string query)
            => new ElementQuery
            {
                ParentId = Id,
                Query = query
            };

        public bool Equals([AllowNull] IVisualElement other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other is VisualElement visualElement)
            {
                return Id == visualElement.Id;
            }
            return false;
        }

        public override bool Equals([AllowNull] object other)
            => Equals(other as IVisualElement);

        public override int GetHashCode()
            => Id.GetHashCode();
        
    }
}
