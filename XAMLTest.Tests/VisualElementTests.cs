using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using XamlTest.Tests.TestControls;

namespace XamlTest.Tests
{
    [TestClass]
    public class VisualElementTests
    {
        public TestContext TestContext { get; set; } = null!;

        [NotNull]
        private IApp? App { get; set; }

        [TestInitialize]
        public async Task TestInitialize()
        {
            App = XamlTest.App.StartRemote(logMessage: msg => TestContext.WriteLine(msg));

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
            Assert.AreEqual(Colors.Red, resource.GetAs<Color>());
            Assert.AreEqual(typeof(Color).AssemblyQualifiedName, resource.ValueType);
        }

        [TestMethod]
        public async Task OnGetCoordinate_ReturnsScreenCoordinatesOfElement()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");
            IVisualElement<Border> element = await window.GetElement<Border>("MyBorder");

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
        public async Task OnGetCoordinate_ReturnsFractionalCoordinatesOfElement()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");
            IVisualElement<Border> element = await window.GetElement<Border>("MyBorder");

            Rect initialCoordinates = await element.GetCoordinates();
            await element.SetWidth(30.7);
            await element.SetHeight(40.3);
            await element.SetMargin(new Thickness(0.1));

            Rect newCoordinates = await element.GetCoordinates();
            Assert.AreEqual(30.7, Math.Round(newCoordinates.Width, 5));
            Assert.AreEqual(40.3, Math.Round(newCoordinates.Height, 5));
            Assert.AreEqual(0.1, Math.Round(newCoordinates.Left - initialCoordinates.Left, 5));
            Assert.AreEqual(0.1, Math.Round(newCoordinates.Top - initialCoordinates.Top, 5));
        }

        [TestMethod]
        public async Task OnGetCoordinate_ReturnsRotatedElementLocation()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Border x:Name=""MyBorder"" Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left"">
    <Border.LayoutTransform>
        <RotateTransform Angle=""90"" />
    </Border.LayoutTransform>
</Border>
");
            IVisualElement element = await window.GetElement("MyBorder");

            Rect coordinates = await element.GetCoordinates();
            Assert.AreEqual(40, Math.Round(coordinates.Width, 5));
            Assert.AreEqual(30, Math.Round(coordinates.Height, 5));
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
        public async Task OnGetEffectiveBackground_StopsProcessingAtDefinedParent()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid Background=""#DDFF0000"">
    <TextBlock />
</Grid>
",
                background: "Blue");
            IVisualElement child = await window.GetElement("/TextBlock");
            IVisualElement parent = await window.GetElement("/Grid");

            Color background = await child.GetEffectiveBackground(parent);

            Assert.AreEqual(Color.FromArgb(0xDD, 0xFF, 0x00, 0x00), background);
        }

        [TestMethod]
        public async Task OnGetEffectiveBackground_AppliesOpacityFromParents()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid Background=""Red"" Opacity=""0.5"" x:Name=""RedGrid"">
    <Grid Background=""Blue"" x:Name=""BlueGrid"">
        <TextBlock />
    </Grid>
</Grid>
",
                background: "Lime");
            IVisualElement child = await window.GetElement("/TextBlock");
            IVisualElement parent = await window.GetElement("BlueGrid");

            Color background = await child.GetEffectiveBackground(parent);

            Assert.AreEqual(Color.FromArgb(127, 0x00, 0x00, 0xFF), background);
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveDouble()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" Width=""25"" />");
            IVisualElement<Grid> element = await window.GetElement<Grid>("MyGrid");

            Assert.AreEqual(25.0, await element.GetWidth());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveColor()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" Background=""Red"" />");
            IVisualElement<Grid> element = await window.GetElement<Grid>("MyGrid");

            Assert.AreEqual(Colors.Red, await element.GetBackgroundColor());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveBrush()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid x:Name=""MyGrid"">
    <Grid.Background>
        <SolidColorBrush Color=""Red"" Opacity=""0.5"" />
    </Grid.Background>
</Grid>");
            IVisualElement element = await window.GetElement("MyGrid");

            var brush = await element.GetProperty<SolidColorBrush>("Background");
            Assert.AreEqual(Colors.Red, brush?.Color);
            Assert.AreEqual(0.5, brush?.Opacity);
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveString()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" Text=""WPF"" />");
            IVisualElement<TextBlock> element = await window.GetElement<TextBlock>("MyTextblock");

            Assert.AreEqual("WPF", await element.GetText());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveThickness()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" Margin=""2,3,4,5"" />");
            IVisualElement<TextBlock> element = await window.GetElement<TextBlock>("MyTextblock");

            Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.GetMargin());
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveNull()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<DatePicker/>");
            IVisualElement element = await window.GetElement("/DatePicker");

            Assert.IsNull(await element.GetProperty<DateTime?>(nameof(DatePicker.SelectedDate)));
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveNullableDateTime()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<DatePicker SelectedDate=""1234-05-06T00:00:00""/>");
            IVisualElement element = await window.GetElement("/DatePicker");
            Assert.AreEqual(new DateTime(1234, 5, 6), await element.GetProperty<DateTime?>(nameof(DatePicker.SelectedDate)));
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveAttachedPropertyValue()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" Grid.Row=""3"" />");
            IVisualElement element = await window.GetElement("MyTextblock");

            Assert.AreEqual(3, await element.GetProperty<int>(Grid.RowProperty));
        }

        [TestMethod]
        public async Task OnGetProperty_CanRetrieveCustomAttachedPropertyValue()
        {
            IWindow window = await App.CreateWindowWithUserControl<TextBlock_AttachedProperty>();
            IVisualElement element = await window.GetElement("/TextBlock");

            Assert.AreEqual("Bar", await element.GetProperty<string>(TextBlock_AttachedProperty.MyCustomPropertyProperty));
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetDouble()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<Grid x:Name=""MyGrid"" />");
            IVisualElement<Grid> element = await window.GetElement<Grid>("MyGrid");

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
            IVisualElement<TextBlock> element = await window.GetElement<TextBlock>("MyTextblock");

            Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.SetMargin(new Thickness(2, 3, 4, 5)));
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetNull()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<DatePicker/>");
            IVisualElement element = await window.GetElement("/DatePicker");

            Assert.IsNull(await element.SetProperty<DateTime?>(nameof(DatePicker.SelectedDate), null));
        }

        [TestMethod]
        public async Task OnSetProperty_CanSetNullableDateTime()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<DatePicker/>");
            IVisualElement element = await window.GetElement("/DatePicker");

            Assert.AreEqual(new DateTime(1234, 5, 6), await element.SetProperty<DateTime?>(nameof(DatePicker.SelectedDate), new DateTime(1234, 5, 6)));
        }

        [TestMethod]
        public async Task OnSetProperty_CanAssignAttachedPropertyValue()
        {
            IWindow window = await App.CreateWindowWithContent(
                @"<TextBlock x:Name=""MyTextblock"" />");
            IVisualElement element = await window.GetElement("MyTextblock");

            Assert.AreEqual(2, await element.SetProperty(Grid.RowProperty, 2));
        }

        [TestMethod]
        public async Task OnSetProperty_CanAssignCustomAttachedPropertyValue()
        {
            IWindow window = await App.CreateWindowWithUserControl<TextBlock_AttachedProperty>();
            IVisualElement element = await window.GetElement("/TextBlock");

            Assert.AreEqual("New value", await element.SetProperty(TextBlock_AttachedProperty.MyCustomPropertyProperty, "New value"));
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
            IVisualElement<ListBoxItem> element = await window.GetElement<ListBoxItem>("/ListBoxItem");

            Assert.AreEqual("Item1", await element.GetContent());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesNestedItemsByType()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid x:Name=""Parent"">
    <Grid x:Name=""Child"">
        <TextBlock />
    </Grid>
</Grid>
");
            IVisualElement child = await window.GetElement("Child");

            IVisualElement nestedElement = await window.GetElement("/Grid/Grid");

            Assert.AreEqual(child, nestedElement);
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
            IVisualElement<ListBoxItem> element = await window.GetElement<ListBoxItem>("/ListBoxItem[2]");

            Assert.AreEqual("Item3", await element.GetContent());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesItemsByProperty()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");
            IVisualElement<TextBlock> element = await window.GetElement<TextBlock>("/Border.Child/TextBlock");

            Assert.AreEqual("Text", await element.GetText());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesItemsByName()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" Text=""Text"" />
</Grid>");
            IVisualElement<TextBox> element = await window.GetElement<TextBox>("/Grid~MyTextBox");

            Assert.AreEqual("Text", await element.GetText());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrieveElementFromAdornerLayer()
        {
            IWindow window = await App.CreateWindowWithUserControl<TextBox_ValidationError>();
            IVisualElement<TextBox> textBox = await window.GetElement<TextBox>("/TextBox");

            IVisualElement<TextBlock> validationMessage = await textBox.GetElement<TextBlock>("ErrorMessageText");

            Assert.IsTrue(await validationMessage.GetIsVisible());
        }

        [TestMethod]
        public async Task OnGetElement_ItRetrievesElementByAutomationIdValue()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" Text=""Text"" AutomationProperties.AutomationId=""TextBoxId""/>
</Grid>");
            IVisualElement<TextBox> element = await window.GetElement<TextBox>("[AutomationProperties.AutomationId=\"TextBoxId\"]");

            Assert.AreEqual("MyTextBox", await element.GetName());
        }

        [TestMethod]
        public async Task OnMoveKeyboardFocus_ItReceivesKeyboardFocus()
        {
            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" />
</Grid>");
            IVisualElement<TextBox> element = await window.GetElement<TextBox>("/Grid~MyTextBox");
            Assert.IsFalse(await element.GetIsKeyboardFocused());

            await element.MoveKeyboardFocus();

            Assert.IsTrue(await element.GetIsKeyboardFocused());
        }

        [TestMethod]
        public async Task OnSendTextInput_TextIsChanged()
        {
            await using TestRecorder recorder = new(App);

            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" VerticalAlignment=""Center"" Margin=""40"" />
</Grid>");
            IVisualElement<TextBox> element = await window.GetElement<TextBox>("/Grid~MyTextBox");
            await element.MoveKeyboardFocus();

            await element.SendInput($"Test Text!");

            Assert.AreEqual("Test Text!", await element.GetText());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnSendTextInput_ExplicitKeyIsSent()
        {
            await using TestRecorder recorder = new(App);

            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" AcceptsReturn=""True"" MinWidth=""280"" Height=""80"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");
            IVisualElement<TextBox> element = await window.GetElement<TextBox>("/Grid~MyTextBox");
            await element.MoveKeyboardFocus();

            await element.SendInput($"First Line{Key.Enter}Second Line");

            Assert.AreEqual($"First Line{Environment.NewLine}Second Line", await element.GetText());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnRegisterForEvent_EventsAreRaised()
        {
            // Arrange
            await using TestRecorder recorder = new(App);

            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <Button x:Name=""MyButton"" Content=""Click Event"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");
            IVisualElement element = await window.GetElement("MyButton");
            await using IEventRegistration clickEvent = await element.RegisterForEvent(nameof(Button.Click));

            // Act
            await element.Click();
            await Task.Delay(500);

            //Assert
            var invocations = await Wait.For(async () =>
            {
                var i = await clickEvent.GetInvocations();
                Assert.AreEqual(1, i.Count);
                return i;
            });

            Assert.AreEqual(2, invocations[0].Parameters.Count);
            Assert.AreEqual($"{typeof(Button).FullName}: Click Event", invocations[0].Parameters[0].ToString());
            Assert.AreEqual(typeof(RoutedEventArgs).FullName, invocations[0].Parameters[1].ToString());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetTypedElement_GetsTypedElement()
        {
            // Arrange
            await using TestRecorder recorder = new(App);

            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <Button x:Name=""MyButton"" IsDefault=""True"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");

            //Act
            IVisualElement<Button> button = await window.GetElement<Button>("MyButton");
            //Assert
            Assert.IsNotNull(button);

            Assert.IsTrue(await button.GetActualWidth() > 0);
            Assert.IsTrue(await button.GetIsDefault());
            Assert.IsFalse(await button.GetIsPressed());
            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetTypedElement_GetsTypedElementByBaseType()
        {
            // Arrange
            await using TestRecorder recorder = new(App);

            IWindow window = await App.CreateWindowWithContent(@"
<Grid>
  <Button x:Name=""MyButton"" IsDefault=""True"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");

            //Act
            IVisualElement<ButtonBase> button = await window.GetElement<ButtonBase>("MyButton");

            //Assert
            Assert.IsNotNull(button);
            Assert.IsTrue(await button.GetActualWidth() > 0);
            recorder.Success();
        }
    }
}
