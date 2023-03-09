using System.Windows.Input;

namespace XamlTest.Input;

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
