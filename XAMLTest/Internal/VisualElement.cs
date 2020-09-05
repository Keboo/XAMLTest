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
        public VisualElement(Protocol.ProtocolClient client, string id, Action<string>? logMessage)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            LogMessage = logMessage;
        }

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
                    return new VisualElement(Client, reply.ElementIds[0], LogMessage);
                }
                throw new Exception($"Found {reply.ElementIds.Count} elements");
            }

            throw new Exception("Failed to receive a reply");
        }

        public async Task<IValue> GetProperty(string name, string? ownerType)
        {
            var propertyQuery = new PropertyQuery
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
                    return new Property(propertyType, reply.ValueType, reply.Value);
                }
                throw new Exception("Property does not have a type specified");
            }
            throw new Exception("Failed to receive a reply");
        }
        
        public async Task<IValue> SetProperty(string name, string value, string? valueType, string? ownerType)
        {
            var query = new SetPropertyRequest
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
                    return new Property(propertyType, reply.ValueType, reply.Value);
                }
                throw new Exception("Property reply does not have a type specified");
            }
            throw new Exception("Failed to receive a reply");
        }
        
        public async Task<IResource> GetResource(string key)
        {
            var query = new ResourceQuery
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
                    return new Resource(reply.Key, reply.ValueType, reply.Value);
                }
                throw new Exception($"Resource with key '{reply.Key}' not found");
            }

            throw new Exception("Failed to receive a reply");
        }

        public async Task<Color> GetEffectiveBackground(IVisualElement? toElement)
        {
            string? toElementId = (toElement as VisualElement)?.Id;

            var propertyQuery = new EffectiveBackgroundQuery
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
            var query = new CoordinatesQuery
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
            var request = new KeyboardFocusRequest
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

            var request = new InputRequest
            {
                ElementId = Id,
                TextInput = keyboardInput.Text
            };
            request.Keys.AddRange(keyboardInput.Keys.Cast<int>());
            LogMessage?.Invoke($"{nameof(SendInput)}({keyboardInput})");
            if (await Client.SendInputAsync(request) is { } reply)
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

        public async Task<IImage> GetBitmap()
        {
            var imageQuery = new ImageQuery
            {
                ElementId = Id
            };
            LogMessage?.Invoke($"{nameof(GetBitmap)}()");
            if (await Client.GetImageAsync(imageQuery) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new Exception(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new BitmapImage(reply.Data);
            }
            throw new Exception("Failed to receive a reply");
        }

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
