using System;

namespace XamlTest.Input
{
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
}
