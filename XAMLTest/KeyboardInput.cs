using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace XamlTest
{
    public sealed class KeyboardInput
    {
        public string Text { get; }
        public IReadOnlyList<Key> Keys { get; }

        public KeyboardInput(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Keys = Array.Empty<Key>();
        }

        public KeyboardInput(params Key[] keys)
        {
            Text = "";
            Keys = keys;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Text))
            {
                return $"{{Keys:{string.Join(",", Keys)}}}";
            }
            return $"{{Text:{Text}}}";
        }
    }
}
