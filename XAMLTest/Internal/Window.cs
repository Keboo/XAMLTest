using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal
{
    internal class Window : VisualElement, IWindow
    {
        public Window(Protocol.ProtocolClient client, string id)
            : base(client, id)
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
