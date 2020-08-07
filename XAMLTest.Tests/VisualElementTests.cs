using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XamlTest.Tests
{
    [TestClass]
    public class VisualElementTests
    {
        private IApp App { get; set; }

        [TestInitialize]
        public async Task TestInitialize()
        {
            App = XamlTest.App.StartRemote();

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            App.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task OnGetResource_ThorwsExceptionWhenNotFound()
        {
            IWindow window = await App.CreateWindowWithContent(@"<Grid x:Name=""MyGrid"" />");
            IVisualElement element = await window.GetElement("MyGrid");

            await element.GetResource("TestResource");
        }

        [TestMethod]
        public async Task OnGetResource_ReturnsFoundResource()
        {
            IWindow window = await App.CreateWindowWithContent(@"<Grid x:Name=""MyGrid"">
  <Grid.Resources>
    <Color x:Key=""TestResource"">Red</Color>
  </Grid.Resources>
</Grid>");
            IVisualElement element = await window.GetElement("MyGrid");

            IResource resource = await element.GetResource("TestResource");

            Assert.AreEqual("TestResource", resource.Key);
            Assert.AreEqual(Colors.Red.ToString(), resource.Value);
            Assert.AreEqual(typeof(Color).AssemblyQualifiedName, resource.ValueType);
        }

        [TestMethod]
        public async Task OnGetCoordinate_ReturnsScreenCoordinatesOfElement()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");
            IVisualElement element = await window.GetElement("MyBorder");

            Rect initialCoordinates = await element.GetCoordinates();
            await element.SetWidth(90);
            await element.SetHeight(80);
            await element.SetMargin(new Thickness(30));

            Rect newCoordinates = await element.GetCoordinates();
            Assert.AreEqual(3.0, Math.Round(newCoordinates.Width / initialCoordinates.Width));
            Assert.AreEqual(2.0, Math.Round(newCoordinates.Height / initialCoordinates.Height));
            Assert.AreEqual(initialCoordinates.Width, Math.Round(newCoordinates.Left - initialCoordinates.Left));
            Assert.AreEqual(initialCoordinates.Width, Math.Round(newCoordinates.Top - initialCoordinates.Top));
        }

        [TestMethod]
        public async Task OnGetEffectiveBackground_ReturnsFirstOpaqueColor()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Border x:Name=""MyBorder"" />",
                background: "Red");
            IVisualElement element = await window.GetElement("MyBorder");

            Color background = await element.GetEffectiveBackground();

            Assert.AreEqual(Colors.Red, background);
        }

        [TestMethod]
        public async Task OnGetEffectiveBackground_ReturnsMergingOfTransparentColors()
        {
            var backgroundParent = Colors.Blue;
            var backgroundChild = Color.FromArgb(0xDD, 0, 0, 0);
            IWindow window = await App.CreateWindowWithContent(
                $@"
<Border Background=""{backgroundParent}"">
  <Border x:Name=""MyBorder"" Background=""{backgroundChild}"" />
</Border>",
                background: "Red");
            IVisualElement element = await window.GetElement("MyBorder");

            Color background = await element.GetEffectiveBackground();

            var expected = backgroundChild.FlattenOnto(backgroundParent);
            Assert.AreEqual(expected, background);
        }

        [TestMethod]
        public async Task OnGetEffectiveBackground_ReturnsOpaquePanelColor()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid Background=""Red"">
    <TextBlock />
</Grid>
",
                background: "Blue");
            IVisualElement element = await window.GetElement("/TextBlock");

            Color background = await element.GetEffectiveBackground();

            Assert.AreEqual(Colors.Red, background);
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveDouble()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" Width=""25"" />");
            IVisualElement element = await window.GetElement("MyGrid");

            Assert.AreEqual(25.0, await element.GetWidth());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveColor()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" Background=""Red"" />");
            IVisualElement element = await window.GetElement("MyGrid");

            Assert.AreEqual(Colors.Red, await element.GetBackgroundColor());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveString()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" Text=""WPF"" />");
            IVisualElement element = await window.GetElement("MyTextblock");

            Assert.AreEqual("WPF", await element.GetText());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveThickness()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" Margin=""2,3,4,5"" />");
            IVisualElement element = await window.GetElement("MyTextblock");

            Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.GetMargin());
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetDouble()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" />");
            IVisualElement element = await window.GetElement("MyGrid");

            Assert.AreEqual(25.0, await element.SetWidth(25));
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetColor()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" />");
            IVisualElement element = await window.GetElement("MyGrid");

            Assert.AreEqual(Colors.Red, await element.SetBackgroundColor(Colors.Red));
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetString()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" />");
            IVisualElement element = await window.GetElement("MyTextblock");

            Assert.AreEqual("WPF", await element.SetText("WPF"));
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetThickness()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" />");
            IVisualElement element = await window.GetElement("MyTextblock");

            Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.SetMargin(new Thickness(2, 3, 4, 5)));
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesItemsByType()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<ListBox MinWidth=""200"">
    <ListBoxItem Content=""Item1"" />
    <ListBoxItem Content=""Item2"" />
    <ListBoxItem Content=""Item3"" />
    <ListBoxItem Content=""Item4"" />
</ListBox>");
            IVisualElement element = await window.GetElement("/ListBoxItem");

            Assert.AreEqual("Item1", await element.GetContent());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesItemsByTypeAndIndex()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<ListBox MinWidth=""200"">
    <ListBoxItem Content=""Item1"" />
    <ListBoxItem Content=""Item2"" />
    <ListBoxItem Content=""Item3"" />
    <ListBoxItem Content=""Item4"" />
</ListBox>");
            IVisualElement element = await window.GetElement("/ListBoxItem[2]");

            Assert.AreEqual("Item3", await element.GetContent());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesItemsByProperty()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");
            IVisualElement element = await window.GetElement("/Border.Child/TextBlock");

            Assert.AreEqual("Text", await element.GetText());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesItemsByName()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" Text=""Text"" />
</Grid>");
            IVisualElement element = await window.GetElement("/Grid~MyTextBox");

            Assert.AreEqual("Text", await element.GetText());
        }

        [TestMethod]
        public async Task OnMoveKeyboardFocus_ItReceivesKeyboardFocus()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" />
</Grid>");
            IVisualElement element = await window.GetElement("/Grid~MyTextBox");

            Assert.IsFalse(await element.GetIsKeyboardFocused());

            await element.MoveKeyboardFocus();

            Assert.IsTrue(await element.GetIsKeyboardFocused());
        }
    }
}
