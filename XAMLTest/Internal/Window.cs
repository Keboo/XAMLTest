using System;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal
{
    internal class Window : VisualElement<Window>, IWindow
    {
        public Window(Protocol.ProtocolClient client, string id, 
            Serializer serializer, Action<string>? logMessage)
            : base(client, id, typeof(System.Windows.Window), serializer, logMessage)
        { }

        public bool Equals([AllowNull] IWindow other)
            => base.Equals(other);

        protected override ElementQuery GetFindElementQuery(string query)
            => new ElementQuery
            {
                WindowId = Id,
                Query = query
            };
    }
}
