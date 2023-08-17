using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XamlTest.Tests.TestControls;

namespace XamlTest.Tests;

[TestClass]
public class GetElementTests
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

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static async Task TestCleanup()
    {
        if (App is { } app)
        {
            await app.DisposeAsync();
            App = null;
        }
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsByType()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            """
            <ListBox MinWidth="200">
              <ListBoxItem Content="Item1" />
              <ListBoxItem Content="Item2" />
              <ListBoxItem Content="Item3" />
              <ListBoxItem Content="Item4" />
            </ListBox>
            """);

        //Act
        IVisualElement<ListBoxItem> element = await Window.GetElement<ListBoxItem>();

        //Assert
        Assert.AreEqual("Item1", await element.GetContent());
        recorder.Success();
    }

    [TestMethod]
    [Description("Issue 27")]
    public async Task OnGetElement_ItRetrievesItemsByBaseType()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"<TextBox x:Name=""TestName""/>");

        //Act
        IVisualElement<TextBoxBase> element = await Window.GetElement<TextBoxBase>("/TextBoxBase");

        //Assert
        Assert.AreEqual("TestName", await element.GetName());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesNestedItemsByType()
    {
        //Assert
        await using TestRecorder recorder = new(App);
        await Window.SetXamlContent(@"
<Grid x:Name=""Parent"">
    <Grid x:Name=""Child"">
        <TextBlock />
    </Grid>
</Grid>
");
        IVisualElement child = await Window.GetElement("Child");

        //Act
        IVisualElement nestedElement = await Window.GetElement("/Grid/Grid");

        //Assert
        Assert.AreEqual(child, nestedElement);
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsByTypeAndIndex()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            @"<ListBox MinWidth=""200"">
    <ListBoxItem Content=""Item1"" />
    <ListBoxItem Content=""Item2"" />
    <ListBoxItem Content=""Item3"" />
    <ListBoxItem Content=""Item4"" />
</ListBox>");

        //Act
        IVisualElement<ListBoxItem> element = await Window.GetElement<ListBoxItem>("/ListBoxItem[2]");

        //Assert
        Assert.AreEqual("Item3", await element.GetContent());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsByProperty()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");

        //Act
        IVisualElement<TextBlock> element = await Window.GetElement<TextBlock>("/Border.Child/TextBlock");

        //Assert
        Assert.AreEqual("Text", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesNestedItemsByName()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" Text=""Text"" />
</Grid>");

        //Act
        IVisualElement<TextBox> element = await Window.GetElement<TextBox>("/Grid~MyTextBox");

        //Assert
        Assert.AreEqual("Text", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrieveElementFromAdornerLayer()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        IWindow window = await App.CreateWindowWithUserControl<TextBox_ValidationError>();
        IVisualElement<TextBox> textBox = await window.GetElement<TextBox>("/TextBox");

        //Act
        IVisualElement<TextBlock> validationMessage = await textBox.GetElement<TextBlock>("ErrorMessageText");

        //Assert
        Assert.IsTrue(await validationMessage.GetIsVisible());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesElementByAutomationIdValue()
    {
        //Arrage
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Grid>
  <TextBox x:Name=""MyTextBox"" Text=""Text"" AutomationProperties.AutomationId=""TextBoxId""/>
</Grid>");

        //Act
        IVisualElement<TextBox> element = await Window.GetElement<TextBox>("[AutomationProperties.AutomationId=\"TextBoxId\"]");

        //Assert
        Assert.AreEqual("MyTextBox", await element.GetName());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_WithNonGenericReference_CanCastToGeneric()
    {
        // Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"<StackPanel x:Name=""Panel"" />");
        IVisualElement panel = await Window.GetElement("Panel");

        //Act
        IVisualElement<StackPanel> stackPanel = panel.As<StackPanel>();

        //Assert
        Assert.AreEqual("Panel", await stackPanel.GetName());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_WithNonGenericReference_CanCastToGenericBaseType()
    {
        // Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"<StackPanel x:Name=""Panel"" />");
        IVisualElement panel = await Window.GetElement("Panel");

        //Act
        IVisualElement<FrameworkElement> frameworkElement = panel.As<FrameworkElement>();

        //Assert
        Assert.AreEqual("Panel", await frameworkElement.GetName());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetTypedElement_GetsTypedElement()
    {
        // Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Grid>
  <Button x:Name=""MyButton"" IsDefault=""True"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");

        //Act
        IVisualElement<Button> button = await Window.GetElement<Button>("MyButton");

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

        await Window.SetXamlContent(@"
<Grid>
  <Button x:Name=""MyButton"" IsDefault=""True"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
</Grid>");

        //Act
        IVisualElement<ButtonBase> button = await Window.GetElement<ButtonBase>("MyButton");

        //Assert
        Assert.IsNotNull(button);
        Assert.IsTrue(await button.GetActualWidth() > 0);
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemWithQuerySyntax()
    {
        // Arrange
        await using TestRecorder recorder = new(App);
        await Window.SetXamlContent(@"<Grid x:Name=""Grid"" />");

        //Act
        IVisualElement<Grid> grid1 = await Window.GetElement(ElementQuery.OfType<Grid>());
        IVisualElement<Grid> grid2 = await Window.GetElement(ElementQuery.WithName<Grid>("Grid"));

        //Assert
        Assert.AreEqual(grid1, grid2);
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsByPropertyQuery()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");

        //Act
        IVisualElement<TextBlock> element = await Window.GetElement(
            ElementQuery.OfType<Border>()
                        .Property<UIElement>("Child")
                        .ChildOfType<TextBlock>());

        //Assert
        Assert.AreEqual("Text", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsPropertyByPropertyQuery()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");

        IVisualElement<Border> border = await Window.GetElement<Border>();

        //Act
        IVisualElement<TextBlock> element = await border.GetElement(
            ElementQuery.Property<UIElement>("Child")
                        .ChildOfType<TextBlock>());

        //Assert
        Assert.AreEqual("Text", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsPropertyByPropertyQueryExpression()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");

        IVisualElement<Border> border = await Window.GetElement<Border>();

        //Act
        IVisualElement<TextBlock> element = await border.GetElement(
            ElementQuery.Property((Border b) => b.Child)
                        .ChildOfType<TextBlock>());

        //Assert
        Assert.AreEqual("Text", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesElementsWithPropertyMatching()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<StackPanel>
  <TextBlock Text=""Text1"" Tag=""1""/>
  <TextBlock Text=""Text2"" Tag=""2""/>
  <TextBlock Text=""Text3"" Tag=""3""/>
</StackPanel>");

        var stackPanel = await Window.GetElement<StackPanel>();

        //Act
        IVisualElement<TextBlock> element = await stackPanel.GetElement(
            ElementQuery.PropertyExpression<TextBlock>("Tag", "2"));

        //Assert
        Assert.AreEqual("Text2", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesElementsWithPropertyMatchingExpression()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            """
            <StackPanel>
              <TextBlock Text="Text1" Tag="1"/>
              <TextBlock Text="Text2" Tag="2"/>
              <TextBlock Text="Text3" Tag="3"/>
            </StackPanel>
            """);

        var stackPanel = await Window.GetElement<StackPanel>();

        //Act
        IVisualElement<TextBlock> element = await stackPanel.GetElement(
            ElementQuery.PropertyExpression<TextBlock>(tb => tb.Tag, "2"));

        //Assert
        Assert.AreEqual("Text2", await element.GetText());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetElement_ItRetrievesItemsByTypeAndIndexQuery()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            @"<ListBox MinWidth=""200"">
    <ListBoxItem Content=""Item1"" />
    <ListBoxItem Content=""Item2"" />
    <ListBoxItem Content=""Item3"" />
    <ListBoxItem Content=""Item4"" />
</ListBox>");

        //Act
        IVisualElement<ListBoxItem> element = await Window.GetElement(ElementQuery.OfType<ListBoxItem>().AtIndex(2));

        //Assert
        Assert.AreEqual("Item3", await element.GetContent());
        recorder.Success();
    }

    [TestMethod]
    public async Task OnFindElement_WhenChildTypeQueryDoesNotMatch_ItReturnsNull()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            """
            <ListBox MinWidth="200">
              <ListBoxItem Content="Item1" />
              <ListBoxItem Content="Item2" />
            </ListBox>
            """);

        //Act
        IVisualElement<TextBox>? element = await Window.FindElement<TextBox>();

        //Assert
        Assert.IsNull(element);
        recorder.Success();
    }

    [TestMethod]
    public async Task OnFindElement_WhenElementNameQueryDoesNotMatch_ItReturnsNull()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            """
            <ListBox MinWidth="200">
              <ListBoxItem Content="Item1" />
              <ListBoxItem Content="Item2" />
            </ListBox>
            """);

        //Act
        IVisualElement<ListBox>? element = await Window.FindElement(ElementQuery.WithName<ListBox>("BadName"));

        //Assert
        Assert.IsNull(element);
        recorder.Success();
    }

    [TestMethod]
    public async Task OnFindElement_WhenPropertyQueryDoesNotMatch_ItThrowsError()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(@"
<Border>
  <TextBlock Text=""Text"" />
</Border>");

        //Act
        XamlTestException exception = await Assert.ThrowsExceptionAsync<XamlTestException>(() => Window.FindElement<TextBlock>("/Border.ChildFoo/TextBlock"));

        //Assert
        Assert.IsTrue(exception.Message.Contains("ChildFoo"));
        recorder.Success();
    }

    [TestMethod]
    public async Task OnFindElement_WhenPropertyNameInPropertyExpressionQueryDoesNotMatch_ItThrowsError()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            """
            <StackPanel>
              <TextBlock Text="Text1" Tag="1"/>
              <TextBlock Text="Text2" Tag="2"/>
              <TextBlock Text="Text3" Tag="3"/>
            </StackPanel>
            """);

        var stackPanel = await Window.GetElement<StackPanel>();

        //Act
        XamlTestException exception = await Assert.ThrowsExceptionAsync<XamlTestException>(() => stackPanel.FindElement(
            ElementQuery.PropertyExpression<TextBlock>("BadProp", "2")));

        //Assert
        Assert.IsTrue(exception.Message.Contains("BadProp"));
        recorder.Success();
    }

    [TestMethod]
    public async Task OnFindElement_WhenPropertyValueInPropertyExpressionQueryDoesNotMatch_ItReturnsNull()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await Window.SetXamlContent(
            """
            <StackPanel>
              <TextBlock Text="Text1" Tag="1"/>
              <TextBlock Text="Text2" Tag="2"/>
              <TextBlock Text="Text3" Tag="3"/>
            </StackPanel>
            """);

        var stackPanel = await Window.GetElement<StackPanel>();

        //Act
        IVisualElement<TextBlock>? element = await stackPanel.FindElement(
            ElementQuery.PropertyExpression<TextBlock>(tb => tb.Tag, "4"));

        //Assert
        Assert.IsNull(element);
        recorder.Success();
    }
}
