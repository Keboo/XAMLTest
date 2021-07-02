using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
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
<Grid x:Name=""Grid"">
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <RowDefinition />
    </Grid.RowDefinitions>
    <Grid.ContextMenu>

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
            var nestedMenuItem = await TopMenuItem.GetElement<MenuItem>("SubMenu");
            await using IEventRegistration registration = await nestedMenuItem.RegisterForEvent(nameof(MenuItem.Click));
            await nestedMenuItem.LeftClick(clickTime:TimeSpan.FromMilliseconds(100));

            var invocations = await registration.GetInvocations();
            Assert.AreEqual(1, invocations.Count);
        }

        //[TestMethod]
        //public async Task CanRightClickToShowContextMenu()
        //{
        //    await TopMenuItem.LeftClick();
        //    var nestedMenuItem = await TopMenuItem.GetElement<MenuItem>("SubMenu");
        //    await using IEventRegistration registration = await nestedMenuItem.RegisterForEvent(nameof(MenuItem.Click));
        //    await nestedMenuItem.LeftClick(clickTime: TimeSpan.FromMilliseconds(100));

        //    var invocations = await registration.GetInvocations();
        //    Assert.AreEqual(1, invocations.Count);
        //}
    }
}
