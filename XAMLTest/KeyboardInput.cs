using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace XamlTest
{
    public sealed class KeyboardInput
    {
        internal IReadOnlyList<IInput> Inputs { get; }

        internal KeyboardInput(params IInput[] inputs)
        {
            Inputs = inputs;
        }

        public KeyboardInput(string text)
            : this(new TextInput(text))
        { }

        public KeyboardInput(params Key[] keys)
            : this(new KeysInput(keys))
        { }

        public override string ToString() => $"{{{string.Join(",", Inputs)}}}";
    }

    internal interface IInput
    { }

    internal class TextInput : IInput
    {
        public string Text { get; }

        public TextInput(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override string ToString()
            => $"Text:{Text}";
    }

    internal class KeysInput : IInput
    {
        public IReadOnlyList<Key> Keys { get; }

        public KeysInput(IEnumerable<Key> keys)
        {
            Keys = keys.ToArray();
        }

        public KeysInput(params Key[] keys)
        {
            Keys = keys;
        }

        public override string ToString()
            => $"Keys:{string.Join(",", Keys)}";
    }
}
