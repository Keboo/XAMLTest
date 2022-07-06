namespace XamlTest;

public static partial class VisualElementMixins
{
    public static async Task<Location> MoveCursorTo(this IVisualElement element,
        Position position = Position.Center,
        int xOffset = 0,
        int yOffset = 0)
    {
        List<MouseInput> inputs = new();
        inputs.Add(MouseInput.MoveToElement(position));
        if (xOffset != 0 || yOffset != 0)
        {
            inputs.Add(MouseInput.MoveRelative(xOffset, yOffset));
        }
        return await element.SendInput(new MouseInput(inputs.ToArray()));
    }

    public static async Task<Location> LeftClick(this IVisualElement element,
        Position position = Position.Center,
        int xOffset = 0, int yOffset = 0,
        TimeSpan? clickTime = null)
    {
        return await SendClick(element,
            MouseInput.LeftDown(),
            MouseInput.LeftUp(),
            position,
            xOffset,
            yOffset,
            clickTime);
    }

    public static async Task<Location> RightClick(this IVisualElement element,
        Position position = Position.Center,
        int xOffset = 0, int yOffset = 0,
        TimeSpan? clickTime = null)
    {
        return await SendClick(element,
            MouseInput.RightDown(),
            MouseInput.RightUp(),
            position,
            xOffset,
            yOffset,
            clickTime);
    }

    public static async Task<Location> SendClick(IVisualElement element,
        MouseInput down,
        MouseInput up,
        Position position,
        int xOffset,
        int yOffset,
        TimeSpan? clickTime)
    {
        List<MouseInput> inputs = new();
        inputs.Add(MouseInput.MoveToElement(position));
        if (xOffset != 0 || yOffset != 0)
        {
            inputs.Add(MouseInput.MoveRelative(xOffset, yOffset));
        }
        inputs.Add(down);
        if (clickTime != null)
        {
            inputs.Add(MouseInput.Delay(clickTime.Value));
        }
        inputs.Add(up);

        return await element.SendInput(new MouseInput(inputs.ToArray()));
    }
}
