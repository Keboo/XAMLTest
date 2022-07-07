using XamlTest.Input;

namespace XamlTest;

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

#if WPF
    public KeyboardInput(params Key[] keys)
        : this(new KeysInput(keys))
    { }
#endif

    public override string ToString() => $"{{{string.Join(";", Inputs)}}}";
}
