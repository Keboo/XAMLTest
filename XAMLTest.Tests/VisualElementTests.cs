using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XamlTest.Tests.TestControls;

namespace XamlTest.Tests;

[TestClass]
public class VisualElementTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IWindow? Window { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"");
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

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveDouble()
    {
        IVisualElement<Grid> element = await Window.SetXamlContent<Grid>(@"<Grid Width=""25"" />");

        Assert.AreEqual(25.0, await element.GetWidth());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveColor()
    {
        IVisualElement<Grid> element = await Window.SetXamlContent<Grid>(@"<Grid Background=""Red"" />");

        Assert.AreEqual(Colors.Red, await element.GetBackgroundColor());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveBrush()
    {
        IVisualElement<Grid> element = await Window.SetXamlContent<Grid>(@"
<Grid>
    <Grid.Background>
        <SolidColorBrush Color=""Red"" Opacity=""0.5"" />
    </Grid.Background>
</Grid>");

        var brush = await element.GetProperty<SolidColorBrush>("Background");
        Assert.AreEqual(Colors.Red, brush?.Color);
        Assert.AreEqual(0.5, brush?.Opacity);
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveString()
    {
        IVisualElement<TextBlock> element = await Window.SetXamlContent<TextBlock>(@"<TextBlock Text=""WPF"" />");

        Assert.AreEqual("WPF", await element.GetText());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveThickness()
    {
        IVisualElement<TextBlock> element = await Window.SetXamlContent<TextBlock>(@"<TextBlock Margin=""2,3,4,5"" />");
        
        Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.GetMargin());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveNull()
    {
        IVisualElement<DatePicker> element = await Window.SetXamlContent<DatePicker>(@"<DatePicker/>");
        
        Assert.IsNull(await element.GetSelectedDate());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveNullableDateTime()
    {
        IVisualElement<DatePicker> element = await Window.SetXamlContent<DatePicker>(@"<DatePicker SelectedDate=""1234-05-06T00:00:00""/>");
        
        Assert.AreEqual(new DateTime(1234, 5, 6), await element.GetSelectedDate());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveAttachedPropertyValue()
    {
        IVisualElement<TextBlock> element = await Window.SetXamlContent<TextBlock>(
            @"<TextBlock Grid.Row=""3"" />");

        Assert.AreEqual(3, await element.GetProperty<int>(Grid.RowProperty));
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveCustomAttachedPropertyValue()
    {
        IWindow window = await App.CreateWindowWithUserControl<TextBlock_AttachedProperty>();
        IVisualElement<TextBlock> element = await window.GetElement<TextBlock>();

        Assert.AreEqual("Bar", await element.GetProperty<string>(TextBlock_AttachedProperty.MyCustomPropertyProperty));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetDouble()
    {
        IVisualElement<Grid> element = await Window.SetXamlContent<Grid>(@"<Grid />");

        Assert.AreEqual(25.0, await element.SetWidth(25));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetColor()
    {
        var element = await Window.SetXamlContent<Grid>(@"<Grid />");

        Assert.AreEqual(Colors.Red, await element.SetBackgroundColor(Colors.Red));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetString()
    {
        var element = await Window.SetXamlContent<TextBlock>(@"<TextBlock />");

        Assert.AreEqual("WPF", await element.SetText("WPF"));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetThickness()
    {
        IVisualElement<TextBlock> element = await Window.SetXamlContent<TextBlock>(@"<TextBlock />");

        Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.SetMargin(new Thickness(2, 3, 4, 5)));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetNull()
    {
        IVisualElement<DatePicker> element = await Window.SetXamlContent<DatePicker>(@"<DatePicker/>");

        Assert.IsNull(await element.SetSelectedDate(null));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetNullableDateTime()
    {
        IVisualElement<DatePicker> element = await Window.SetXamlContent<DatePicker>(@"<DatePicker/>");

        Assert.AreEqual(new DateTime(1234, 5, 6), await element.SetSelectedDate(new DateTime(1234, 5, 6)));
    }

    [TestMethod]
    public async Task OnSetProperty_CanAssignAttachedPropertyValue()
    {
        var element = await Window.SetXamlContent<TextBlock>(@"<TextBlock />");

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
    public async Task OnMoveKeyboardFocus_ItReceivesKeyboardFocus()
    {
        await Window.SetXamlContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" />
</Grid>");
        IVisualElement<TextBox> element = await Window.GetElement<TextBox>("/Grid~MyTextBox");
        Assert.IsFalse(await element.GetIsKeyboardFocused());

        await element.MoveKeyboardFocus();

        Assert.IsTrue(await element.GetIsKeyboardFocused());
    }

    [TestMethod]
    public async Task OnSendTextInput_TextIsChanged()
    {
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" VerticalAlignment=""Center"" Margin=""40"" />
</Grid>");
        IVisualElement<TextBox> element = await Window.GetElement<TextBox>("/Grid~MyTextBox");
        await element.MoveKeyboardFocus();

        await element.SendKeyboardInput($"Test Text!");

        Assert.AreEqual("Test Text!", await element.GetText());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnSendTextInput_ExplicitKeyIsSent()
    {
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" AcceptsReturn=""True"" MinWidth=""280"" Height=""80"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");
        IVisualElement<TextBox> element = await Window.GetElement<TextBox>("/Grid~MyTextBox");
        await element.MoveKeyboardFocus();

        await element.SendKeyboardInput($"First Line{Key.Enter}Second Line");

        Assert.AreEqual($"First Line{Environment.NewLine}Second Line", await element.GetText());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetProperty_WhenPropertyIsDependencyObject_GetVisualElement()
    {
        // Arrange
        await using TestRecorder recorder = new(App);

        IVisualElement<StackPanel> stackPanel = await Window.SetXamlContent<StackPanel>(@"
<StackPanel x:Name=""Panel"">
    <StackPanel.ContextMenu>
         <ContextMenu x:Name=""TestContextMenu""/>
    </StackPanel.ContextMenu>
</StackPanel>");

        //Act
        IVisualElement<ContextMenu>? contextMenu = await stackPanel.GetContextMenu();

        //Assert
        Assert.IsNotNull(contextMenu);
        Assert.AreEqual("TestContextMenu", await contextMenu.GetName());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnRemoteExecute_CanExecuteRemoteCode()
    {
        // Arrange
        await using TestRecorder recorder = new(App);

        //Act
        await Window.RemoteExecute(ChangeTitle);

        //Assert
        Assert.AreEqual("Test Title", await Window.GetTitle());
        recorder.Success();

        static void ChangeTitle(Window window)
        {
            window.Title = "Test Title";
        }
    }
}
