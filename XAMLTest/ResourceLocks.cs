namespace XamlTest;

[Flags]
public enum ResourceLocks
{
    None = 0,
    Keyboard = 1 << 0,
    Mouse = 1 << 1,
    Focus = 1 << 2,

    Input = Keyboard | Mouse,
    All = Keyboard | Mouse | Focus
}
