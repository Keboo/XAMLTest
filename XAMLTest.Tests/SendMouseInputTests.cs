using System.Windows.Controls;

namespace XamlTest.Tests;

[TestClass]
public class SendMouseInputTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IVisualElement<Grid>? Grid { get; set; }

    [NotNull]
    private static IVisualElement<MenuItem>? TopMenuItem { get; set; }

    [NotNull]
    private static IVisualElement<Button>? Button { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);
        
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
    <Button Content=""Click Me"" Grid.Row=""1"" VerticalAlignment=""Center"" HorizontalAlignment=""Center""/>
</Grid>
");
        Grid = await window.GetElement<Grid>("Grid");
        TopMenuItem = await window.GetElement<MenuItem>("TopLevel");
        Button = await window.GetElement<Button>();
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
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
        await using var recorder = new TestRecorder(App);
        await TopMenuItem.LeftClick();
        await Task.Delay(200);
        var nestedMenuItem = await TopMenuItem.GetElement<MenuItem>("SubMenu");

        await using IEventRegistration registration = await nestedMenuItem.RegisterForEvent(nameof(MenuItem.Click));

        await nestedMenuItem.LeftClick();

        //NB: The click event on MenuItems is deferred until the next render, need a delay to pick it up.
        //https://source.dot.net/#PresentationFramework/System/Windows/Controls/MenuItem.cs,1388
        await Wait.For(async () =>
        {
            await nestedMenuItem.LeftClick(clickTime:TimeSpan.FromMilliseconds(200));
            var invocations = await registration.GetInvocations();
            Assert.IsTrue(invocations.Count > 0);
        });

        recorder.Success();
    }

    [TestMethod]
    public async Task CanDoubleClickOnButton()
    {
        await using var recorder = new TestRecorder(App);

        await using IEventRegistration registration = await Button.RegisterForEvent(nameof(Control.MouseDoubleClick));

        await Wait.For(async () =>
        {
            await Button.LeftDoubleClick();
            var invocations = await registration.GetInvocations();
            Assert.IsTrue(invocations.Count > 1);
        });

        recorder.Success();
    }

    [TestMethod]
    public async Task LeftClick_WithPositionOffset_OffsetsCursor()
    {
        await using var recorder = new TestRecorder(App);
        Rect coordinates = await TopMenuItem.GetCoordinates();
        Point mousePosition = await TopMenuItem.LeftClick(Position.BottomLeft, 15, -5);

        Point expected = coordinates.BottomLeft + new Vector(15, -5);
        Assert.IsTrue(Math.Abs(expected.X - mousePosition.X) <= 1, $"Distance {Math.Abs(expected.X - mousePosition.X)} is greater than tolerance");
        Assert.IsTrue(Math.Abs(expected.Y - mousePosition.Y) <= 1, $"Distance {Math.Abs(expected.Y - mousePosition.Y)} is greater than tolerance");
        recorder.Success();
    }

    [TestMethod]
    public async Task CanRightClickToShowContextMenu()
    {
        await using var recorder = new TestRecorder(App);
        await Grid.RightClick();
        IVisualElement<ContextMenu>? contextMenu = await Grid.GetContextMenu();
        Assert.IsNotNull(contextMenu);
        var menuItem = await contextMenu.GetElement<MenuItem>("Context1");
        await Task.Delay(100);
        Assert.IsNotNull(menuItem);
        await using IEventRegistration registration = await menuItem.RegisterForEvent(nameof(MenuItem.Click));
        await menuItem.LeftClick(clickTime: TimeSpan.FromMilliseconds(100));

        await Wait.For(async () =>
        {
            var invocations = await registration.GetInvocations();
            Assert.AreEqual(1, invocations.Count);
        });
        recorder.Success();
    }

    [TestMethod]
    public async Task CanMoveCursorPositions()
    {
        await using var recorder = new TestRecorder(App);
        const double tolerance = 1.0;

        Rect coordinates = await Grid.GetCoordinates();
        Point center = new(
            coordinates.Left + coordinates.Width / 2.0,
            coordinates.Top + coordinates.Height / 2.0);

        Point cursorPosition = await Grid.MoveCursorTo(Position.Center);
        Vector distance = center - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");

        cursorPosition = await Grid.MoveCursorTo(Position.TopLeft);
        distance = coordinates.TopLeft - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");

        cursorPosition = await Grid.MoveCursorTo(Position.TopRight);
        distance = coordinates.TopRight - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");

        cursorPosition = await Grid.MoveCursorTo(Position.BottomRight);
        distance = coordinates.BottomRight - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");

        cursorPosition = await Grid.MoveCursorTo(Position.BottomLeft);
        distance = coordinates.BottomLeft - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");

        recorder.Success();
    }

    [TestMethod]
    public async Task CanMoveCursorPositionToRelativePosition()
    {
        await using var recorder = new TestRecorder(App);
        const double tolerance = 1.0;

        Rect coordinates = await Grid.GetCoordinates();
        Point center = new(
            coordinates.Left + coordinates.Width / 2.0,
            coordinates.Top + coordinates.Height / 2.0);

        Point cursorPosition = await Grid.MoveCursorTo(Position.Center, 10, 20);
        Vector distance = (center + new Vector(10, 20)) - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");
        recorder.Success();
    }

    [TestMethod]
    public async Task CanMoveCursorPositionToAbsolutePosition()
    {
        await using var recorder = new TestRecorder(App);
        const double tolerance = 1.0;

        Rect coordinates = await Grid.GetCoordinates();
        Point center = new(
            coordinates.Left + coordinates.Width / 2.0,
            coordinates.Top + coordinates.Height / 2.0);

        Point cursorPosition = await Grid.SendInput(MouseInput.MoveAbsolute((int)center.X, (int)center.Y));
        Vector distance = center - cursorPosition;
        Assert.IsTrue(distance.Length < tolerance, $"Distance {distance.Length} is greater than tolerance");
        recorder.Success();
    }
}
