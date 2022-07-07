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
        App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

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
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" Width=""25"" />");

        IVisualElement<Grid> element = await Window.GetElement<Grid>("MyGrid");
        Assert.AreEqual(25.0, await element.GetWidth());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveColor()
    {
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" Background=""Red"" />");

        IVisualElement<Grid> element = await Window.GetElement<Grid>("MyGrid");
        
        Assert.AreEqual(Colors.Red, await element.GetBackgroundColor());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveBrush()
    {
        await Window.SetXamlContent(@"
<Grid x:Name=""MyGrid"">
    <Grid.Background>
        <SolidColorBrush Color=""Red"" Opacity=""0.5"" />
    </Grid.Background>
</Grid>");
        IVisualElement element = await Window.GetElement("MyGrid");

        var brush = await element.GetProperty<SolidColorBrush>("Background");
        Assert.AreEqual(Colors.Red, brush?.Color);
        Assert.AreEqual(0.5, brush?.Opacity);
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveString()
    {
        await Window.SetXamlContent(@"<TextBlock x:Name=""MyTextblock"" Text=""WPF"" />");
        
        IVisualElement<TextBlock> element = await Window.GetElement<TextBlock>("MyTextblock");

        Assert.AreEqual("WPF", await element.GetText());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveThickness()
    {
        await Window.SetXamlContent(@"<TextBlock x:Name=""MyTextblock"" Margin=""2,3,4,5"" />");

        IVisualElement<TextBlock> element = await Window.GetElement<TextBlock>("MyTextblock");

        Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.GetMargin());
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveNull()
    {
        await Window.SetXamlContent(@"<DatePicker/>");

        IVisualElement element = await Window.GetElement("/DatePicker");

        Assert.IsNull(await element.GetProperty<DateTime?>(nameof(DatePicker.SelectedDate)));
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveNullableDateTime()
    {
        await Window.SetXamlContent(@"<DatePicker SelectedDate=""1234-05-06T00:00:00""/>");
        
        IVisualElement element = await Window.GetElement("/DatePicker");
        
        Assert.AreEqual(new DateTime(1234, 5, 6), await element.GetProperty<DateTime?>(nameof(DatePicker.SelectedDate)));
    }

    [TestMethod]
    public async Task OnGetProperty_CanRetrieveAttachedPropertyValue()
    {
        await Window.SetXamlContent(
            @"<TextBlock x:Name=""MyTextblock"" Grid.Row=""3"" />");

        IVisualElement element = await Window.GetElement("MyTextblock");

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
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" />");

        IVisualElement<Grid> element = await Window.GetElement<Grid>("MyGrid");

        Assert.AreEqual(25.0, await element.SetWidth(25));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetColor()
    {
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" />");

        var element = await Window.GetElement<Grid>("MyGrid");

        Assert.AreEqual(Colors.Red, await element.SetBackgroundColor(Colors.Red));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetString()
    {
        await Window.SetXamlContent(@"<TextBlock x:Name=""MyTextblock"" />");

        var element = await Window.GetElement<TextBlock>("MyTextblock");

        Assert.AreEqual("WPF", await element.SetText("WPF"));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetThickness()
    {
        await Window.SetXamlContent(@"<TextBlock x:Name=""MyTextblock"" />");

        IVisualElement<TextBlock> element = await Window.GetElement<TextBlock>("MyTextblock");

        Assert.AreEqual(new Thickness(2, 3, 4, 5), await element.SetMargin(new Thickness(2, 3, 4, 5)));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetNull()
    {
        await Window.SetXamlContent(@"<DatePicker/>");

        IVisualElement element = await Window.GetElement("/DatePicker");

        Assert.IsNull(await element.SetProperty<DateTime?>(nameof(DatePicker.SelectedDate), null));
    }

    [TestMethod]
    public async Task OnSetProperty_CanSetNullableDateTime()
    {
        await Window.SetXamlContent(@"<DatePicker/>");

        IVisualElement element = await Window.GetElement("/DatePicker");

        Assert.AreEqual(new DateTime(1234, 5, 6), await element.SetProperty<DateTime?>(nameof(DatePicker.SelectedDate), new DateTime(1234, 5, 6)));
    }

    [TestMethod]
    public async Task OnSetProperty_CanAssignAttachedPropertyValue()
    {
        await Window.SetXamlContent(
            @"<TextBlock x:Name=""MyTextblock"" />");

        IVisualElement element = await Window.GetElement("MyTextblock");

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

#if WPF
        Assert.IsFalse(await element.GetIsKeyboardFocused());
#elif WIN_UI
        Assert.AreEqual(FocusState.Unfocused, await element.GetFocusState());
#endif
        await element.MoveKeyboardFocus();

#if WPF
        Assert.IsTrue(await element.GetIsKeyboardFocused());
#elif WIN_UI
        Assert.AreNotEqual(FocusState.Unfocused, await element.GetFocusState());
#endif
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

#if WPF
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
#endif

    [TestMethod]
    public async Task OnGetProperty_WhenPropertyIsDependencyObject_GetVisualElement()
    {
        // Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<StackPanel x:Name=""Panel"">
    <StackPanel.ContextMenu>
         <ContextMenu x:Name=""TestContextMenu""/>
    </StackPanel.ContextMenu>
</StackPanel>");
        IVisualElement<StackPanel> stackPanel = await Window.GetElement<StackPanel>("Panel");

        //Act
#if WPF
        IVisualElement<ContextMenu>? contextMenu = await stackPanel.GetContextMenu();
#elif WIN_UI
        IVisualElement<FlyoutBase>? contextMenu = await stackPanel.GetContextFlyout();
#endif
        //Assert
        Assert.IsNotNull(contextMenu);
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
