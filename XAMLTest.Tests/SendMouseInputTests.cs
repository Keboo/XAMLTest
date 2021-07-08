using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XamlTest.Tests
{
    [TestClass]
    public class SendMouseInputTests
    {
        [NotNull]
        private static IApp? App { get; set; }

        [NotNull]
        private static IVisualElement<Grid>? Grid { get; set; }

        [NotNull]
        private static IVisualElement<MenuItem>? TopMenuItem { get; set; }
        
        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            App = XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

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
            TopMenuItem = await window.GetElement<MenuItem>("TopLevel");
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            App.Dispose();
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
            var nestedMenuItem = await TopMenuItem.GetElement<MenuItem>("SubMenu");
            await using IEventRegistration registration = await nestedMenuItem.RegisterForEvent(nameof(MenuItem.Click));
            await nestedMenuItem.LeftClick(clickTime:TimeSpan.FromMilliseconds(100));

            await Wait.For(async () =>
            {
                var invocations = await registration.GetInvocations();
                Assert.AreEqual(1, invocations.Count);
            });
        }

        [TestMethod]
        public async Task CanRightClickToShowContextMenu()
        {
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
        }

        [TestMethod]
        public async Task CanMoveCursorPositions()
        {
            const double tollerance = 1.0;

            Rect coordinates = await Grid.GetCoordinates();
            Point center = new(
                coordinates.Left + coordinates.Width / 2.0,
                coordinates.Top + coordinates.Height / 2.0);
            
            Point cursorPosition = await Grid.MoveCurosrTo(Position.Center);
            Vector distance = center - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);

            cursorPosition = await Grid.MoveCurosrTo(Position.TopLeft);
            distance = coordinates.TopLeft - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);

            cursorPosition = await Grid.MoveCurosrTo(Position.TopRight);
            distance = coordinates.TopRight - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);

            cursorPosition = await Grid.MoveCurosrTo(Position.BottomRight);
            distance = coordinates.BottomRight - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);

            cursorPosition = await Grid.MoveCurosrTo(Position.BottomLeft);
            distance = coordinates.BottomLeft - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);
        }

        [TestMethod]
        public async Task CanMoveCursorPositionToRelativePosition()
        {
            const double tollerance = 1.0;

            Rect coordinates = await Grid.GetCoordinates();
            Point center = new(
                coordinates.Left + coordinates.Width / 2.0,
                coordinates.Top + coordinates.Height / 2.0);

            Point cursorPosition = await Grid.MoveCurosrTo(Position.Center, 10, 20);
            Vector distance = (center + new Vector(10, 20)) - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);
        }

        [TestMethod]
        public async Task CanMoveCursorPositionToAbsolutePosition()
        {
            const double tollerance = 1.0;

            Rect coordinates = await Grid.GetCoordinates();
            Point center = new(
                coordinates.Left + coordinates.Width / 2.0,
                coordinates.Top + coordinates.Height / 2.0);

            Point cursorPosition = await Grid.SendInput(MouseInput.MoveAbsolute((int)center.X, (int)center.Y));
            Vector distance = center - cursorPosition;
            Assert.IsTrue(distance.Length < tollerance);
        }
    }
}
