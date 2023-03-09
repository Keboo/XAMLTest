using System.Windows.Input;

namespace XamlTest.Input;

internal class ModifiersInput : IInput
{
    public ModifierKeys Modifiers { get; }

    public ModifiersInput(ModifierKeys modifiers)
    {
        Modifiers = modifiers;
    }

    public override string ToString()
        => $"Modifiers:{Modifiers}";
}