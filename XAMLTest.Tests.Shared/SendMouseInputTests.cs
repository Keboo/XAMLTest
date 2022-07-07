namespace XamlTest.Tests;

[TestClass]
public class SendMouseInputTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IVisualElement<Grid>? Grid { get; set; }

    [NotNull]
    private static IVisualElement<NativeMenuItem>? TopMenuItem { get; set; }
    
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        var window = await App.CreateWindowWithContent(
            @$"
<Grid x:Name=""Grid"" Background=""Transparent"" Margin=""10"">
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <RowDefinition />
    </Grid.RowDefinitions>
    <Grid.ContextMenu>
        <ContextMenu>
            <MenuItem Header=""Context1"" x:Name=""Context1"" />
        </ContextMenu>
    </Grid.ContextMenu>
    <Menu> 
        <MenuItem Header=""TopLevel"" x:Name=""TopLevel"">
            <MenuItem Header=""SubMenu"" x:Name=""SubMenu"" />
        </MenuItem>
    </Menu>
</Grid>
");
        Grid = await window.GetElement<Grid>("Grid");
        TopMenuItem = await window.GetElement<NativeMenuItem>("TopLevel");
    }

    [ClassCleanup]
    public static async Task TestCleanup()
    {
        if (App is { } app)
        {
            await app.DisposeAsync();
            App = null;
        }
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        await Grid.LeftClick(Position.BottomRight);
    }

    [TestMethod]
    public async Task CanClickThroughMenus()
    {
        await TopMenuItem.LeftClick();
        await Task.Delay(100);
        var nestedMenuItem = await TopMenuItem.GetElement<NativeMenuItem>("SubMenu");
#if WPF
        await using IEventRegistration registration = await nestedMenuItem.RegisterForEvent(nameof(NativeMenuItem.Click));
#elif WIN_UI
        await using IEventRegistration registration = await nestedMenuItem.RegisterForEvent(nameof(NativeMenuItem.Tapped));
#endif
        await nestedMenuItem.LeftClick(clickTime:TimeSpan.FromMilliseconds(100));

        await Wait.For(async () =>
        {
            var invocations = await registration.GetInvocations();
            Assert.AreEqual(1, invocations.Count);
        });
    }

    [TestMethod]
    public async Task LeftClick_WithPositionOffset_OffsetsCursor()
    {
        Rect coordinates = await TopMenuItem.GetCoordinates();
        Point mousePosition = await TopMenuItem.LeftClick(Position.BottomLeft, 15, -5);

        Point expected = Distance.Add(NewPoint(coordinates.Left, coordinates.Bottom), 15, -5);
        Assert.IsTrue(Math.Abs(expected.X - mousePosition.X) <= 1);
        Assert.IsTrue(Math.Abs(expected.Y - mousePosition.Y) <= 1);
    }

    [TestMethod]
    public async Task CanRightClickToShowContextMenu()
    {
        await Grid.RightClick();

#if WPF
        IVisualElement<ContextMenu>? contextMenu = await Grid.GetContextMenu();
#elif WIN_UI
        IVisualElement<FlyoutBase>? contextMenu = await Grid.GetContextFlyout();
#endif
        Assert.IsNotNull(contextMenu);
        var menuItem = await contextMenu.GetElement<NativeMenuItem>("Context1");
        await Task.Delay(100);
        Assert.IsNotNull(menuItem);
#if WPF
        await using IEventRegistration registration = await menuItem.RegisterForEvent(nameof(NativeMenuItem.Click));
#elif WIN_UI
        await using IEventRegistration registration = await menuItem.RegisterForEvent(nameof(NativeMenuItem.Tapped));
#endif
        await menuItem.LeftClick(clickTime: TimeSpan.FromMilliseconds(100));

        await Wait.For(async () =>
        {
            var invocations = await registration.GetInvocations();
            Assert.AreEqual(1, invocations.Count);
        });
    }

    [TestMethod]
    public async Task CanMoveCursorPositions()
    {
        const double tollerance = 1.0;

        Rect coordinates = await Grid.GetCoordinates();
        Point center = NewPoint(
            coordinates.Left + coordinates.Width / 2.0,
            coordinates.Top + coordinates.Height / 2.0);

        Point cursorPosition = await Grid.MoveCursorTo(Position.Center);
        var distance = Distance.Between(center, cursorPosition);
        Assert.IsTrue(distance < tollerance);

        cursorPosition = await Grid.MoveCursorTo(Position.TopLeft);
        Point topLeft = NewPoint(coordinates.Left, coordinates.Top);
        distance = Distance.Between(topLeft, cursorPosition);
        Assert.IsTrue(distance < tollerance);

        cursorPosition = await Grid.MoveCursorTo(Position.TopRight);
        Point topRight = NewPoint(coordinates.Right, coordinates.Top);
        distance = Distance.Between(topRight, cursorPosition);
        Assert.IsTrue(distance < tollerance);

        cursorPosition = await Grid.MoveCursorTo(Position.BottomRight);
        Point bottomRight = NewPoint(coordinates.Right, coordinates.Bottom);
        distance = Distance.Between(bottomRight, cursorPosition);
        Assert.IsTrue(distance < tollerance);

        cursorPosition = await Grid.MoveCursorTo(Position.BottomLeft);
        Point bottomLeft = NewPoint(coordinates.Left, coordinates.Bottom);
        distance = Distance.Between(bottomLeft, cursorPosition);
        Assert.IsTrue(distance < tollerance);
    }

    [TestMethod]
    public async Task CanMoveCursorPositionToRelativePosition()
    {
        const double tollerance = 1.0;

        Rect coordinates = await Grid.GetCoordinates();
        Point center = NewPoint(
            coordinates.Left + coordinates.Width / 2.0,
            coordinates.Top + coordinates.Height / 2.0);

        Point cursorPosition = await Grid.MoveCursorTo(Position.Center, 10, 20);
        var distance = Distance.Between(Distance.Add(center, 10, 20), cursorPosition);
        Assert.IsTrue(distance < tollerance);
    }

    [TestMethod]
    public async Task CanMoveCursorPositionToAbsolutePosition()
    {
        const double tollerance = 1.0;

        Rect coordinates = await Grid.GetCoordinates();
        Point center = NewPoint(
            coordinates.Left + coordinates.Width / 2.0,
            coordinates.Top + coordinates.Height / 2.0);

        Point cursorPosition = await Grid.SendInput(MouseInput.MoveAbsolute((int)center.X, (int)center.Y));
        double distance = Distance.Between(center, cursorPosition);
        Assert.IsTrue(distance < tollerance);
    }

    private static Point NewPoint(double x, double y)
    {
#if WPF
        return new Point(x, y);
#elif WIN_UI
        return new Point((float)x, (float)y);
#endif
    }
}

public static class Distance
{
    public static double Between(Point a, Point b)
    {
#if WPF
        return (a - b).Length;
#elif WIN_UI
        return (a - b).Length();
#endif
    }

    public static Point Add(Point point, double xDelta = 0.0, double yDelta = 0.0)
    {
#if WPF
        return point + new Vector(xDelta, yDelta);
#elif WIN_UI
        return point + new Point((float)xDelta, (float)yDelta);
#endif
    }
}
