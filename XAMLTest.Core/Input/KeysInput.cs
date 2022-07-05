namespace XamlTest.Input;

internal class KeysInput : IInput
{
    public IReadOnlyList<int> Keys { get; }
    
    public KeysInput(IEnumerable<int> keys)
    {
        Keys = keys.ToArray();
    }

    public KeysInput(params int[] keys)
    {
        Keys = keys;
    }

    public override string ToString()
        => $"Keys:{string.Join(",", Keys)}";
}
