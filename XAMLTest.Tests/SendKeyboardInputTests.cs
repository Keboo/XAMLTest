using System.Windows.Controls;
using System.Windows.Input;

namespace XamlTest.Tests;

[TestClass]
public class SendKeyboardInputTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IVisualElement<TabControl>? TabControl { get; set; }

    [NotNull]
    private static IVisualElement<TextBox>? TextBox1 { get; set; }

    [NotNull]
    private static IVisualElement<TextBox>? TextBox2 { get; set; }

    [NotNull]
    private static IVisualElement<TextBox>? TextBox3 { get; set; }

    [NotNull]
    private static IVisualElement<ListBox>? ListBox { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem1 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem2 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem3 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem4 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem5 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem6 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem7 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem8 { get; set; }
    [NotNull]
    private static IVisualElement<ListBoxItem>? ListBoxItem9 { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        var window = await App.CreateWindowWithContent("""
            <TabControl x:Name="TabControl">
              <TabItem Header="Tab1">
                <StackPanel Orientation="Vertical">
                  <TextBox x:Name="TestTextBox1" />
                  <TextBox x:Name="TestTextBox2" />
                  <TextBox x:Name="TestTextBox3" />
                  <ListBox x:Name="ListBox" SelectionMode="Extended">
                    <ListBoxItem x:Name="ListBoxItem1">Item 1</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem2">Item 2</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem3">Item 3</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem4">Item 4</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem5">Item 5</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem6">Item 6</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem7">Item 7</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem8">Item 8</ListBoxItem>
                    <ListBoxItem x:Name="ListBoxItem9">Item 9</ListBoxItem>
                  </ListBox>
                </StackPanel>
              </TabItem>
              <TabItem Header="Tab2">
                <TextBlock Text="Tab 2 content" />
              </TabItem>
            </TabControl>
            """);
        TabControl = await window.GetElement<TabControl>("TabControl");
        TextBox1 = await window.GetElement<TextBox>("TestTextBox1");
        TextBox2 = await window.GetElement<TextBox>("TestTextBox2");
        TextBox3 = await window.GetElement<TextBox>("TestTextBox3");
        ListBox = await window.GetElement<ListBox>("ListBox");
        ListBoxItem1 = await ListBox.GetElement<ListBoxItem>("ListBoxItem1");
        ListBoxItem2 = await ListBox.GetElement<ListBoxItem>("ListBoxItem2");
        ListBoxItem3 = await ListBox.GetElement<ListBoxItem>("ListBoxItem3");
        ListBoxItem4 = await ListBox.GetElement<ListBoxItem>("ListBoxItem4");
        ListBoxItem5 = await ListBox.GetElement<ListBoxItem>("ListBoxItem5");
        ListBoxItem6 = await ListBox.GetElement<ListBoxItem>("ListBoxItem6");
        ListBoxItem7 = await ListBox.GetElement<ListBoxItem>("ListBoxItem7");
        ListBoxItem8 = await ListBox.GetElement<ListBoxItem>("ListBoxItem8");
        ListBoxItem9 = await ListBox.GetElement<ListBoxItem>("ListBoxItem9");
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
        await TabControl.SetSelectedIndex(0);
        await TextBox1.SetText("");
        await TextBox2.SetText("");
        await TextBox3.SetText("");
        await ListBox.RemoteExecute(ClearListBox);

        static void ClearListBox(ListBox listBox)
        {
            listBox.UnselectAll();
        }
    }

    [TestMethod]
    public async Task SendInput_WithStringInput_SetsText()
    {
        await TextBox1.SendInput(new KeyboardInput("Some Text"));

        Assert.AreEqual("Some Text", await TextBox1.GetText());
    }

    [TestMethod]
    public async Task SendInput_WithFormattableStringInput_SetsText()
    {
        await TextBox1.SendKeyboardInput($"Some Text");

        Assert.AreEqual("Some Text", await TextBox1.GetText());
    }

    [TestMethod]
    public async Task SendInput_WithFormattableStringWithKeys_SetsText()
    {
        await TextBox1.SendKeyboardInput($"Some{Key.Space}Text");

        Assert.AreEqual("Some Text", await TextBox1.GetText());
    }

    [TestMethod]
    public async Task SendInput_WithTabKey_MovesFocusForward()
    {
        await TextBox1.MoveKeyboardFocus();
        await TextBox1.SendKeyboardInput($"{Key.Tab}");

        Assert.IsTrue(await TextBox2.GetIsKeyboardFocusWithin());
    }

    [TestMethod]
    public async Task SendInput_WithTabKeyAndShiftModifier_MovesFocusBackwards()
    {
        await TextBox2.MoveKeyboardFocus();
        await TextBox2.SendKeyboardInput($"{ModifierKeys.Shift}{Key.Tab}{ModifierKeys.None}");

        Assert.IsTrue(await TextBox1.GetIsKeyboardFocusWithin());
    }

    [TestMethod]
    public async Task SendInput_WithCopyPasteModifiers_CopyPasteViaClipboardWorks()
    {
        await TextBox1.MoveKeyboardFocus();
        await TextBox1.SendKeyboardInput($"test input");
        await TextBox1.SendKeyboardInput($"{ModifierKeys.Control}{Key.A}{Key.C}{ModifierKeys.None}{Key.Tab}");
        await TextBox2.SendKeyboardInput($"{ModifierKeys.Control}{Key.V}{ModifierKeys.None}");

        Assert.AreEqual("test input", await TextBox2.GetText());
    }

    [TestMethod]
    public async Task SendInput_WithTabKeyAndControlModifier_ChangesSelectedTab()
    {
        Assert.AreEqual(0, await TabControl.GetSelectedIndex());
        await TabControl.SendKeyboardInput($"{ModifierKeys.Control}{Key.Tab}{ModifierKeys.None}");
        Assert.AreEqual(1, await TabControl.GetSelectedIndex());
    }

    [TestMethod]
    public async Task SendInput_WithControlAndShiftModifiers_AllowsForMultiRangeSelectionInListBox()
    {
        await AssertSelection(Array.Empty<IVisualElement<ListBoxItem>>());

        // Select items 2 through 4
        await ListBoxItem2.LeftClick();
        await ListBox.SendKeyboardInput($"{ModifierKeys.Control | ModifierKeys.Shift}");
        await ListBoxItem4.LeftClick();

        // Extend selection with item 7
        await ListBox.SendKeyboardInput($"{ModifierKeys.Control}");
        await ListBoxItem7.LeftClick();

        // Release modifiers
        await ListBox.SendKeyboardInput($"{ModifierKeys.None}");

        await AssertSelection(ListBoxItem2, ListBoxItem3, ListBoxItem4, ListBoxItem7);

        async Task AssertSelection(params IVisualElement<ListBoxItem>[] selectedItems)
        {
            IVisualElement<ListBoxItem>[] allListBoxItems =
            {
                ListBoxItem1,
                ListBoxItem2,
                ListBoxItem3,
                ListBoxItem4,
                ListBoxItem5,
                ListBoxItem6,
                ListBoxItem7,
                ListBoxItem8,
                ListBoxItem9
            };

            foreach (IVisualElement<ListBoxItem> listBoxItem in allListBoxItems)
            {
                bool selected = await listBoxItem.GetIsSelected();
                string? name = await listBoxItem.GetName();
                if (selectedItems.Contains(listBoxItem))
                {
                    Assert.IsTrue(selected, $"ListBoxItem (Name={name}) was not selected)");
                }
                else
                {
                    Assert.IsFalse(selected, $"ListBoxItem (Name={name}) was selected when it should not be)");
                }
            }
        }
    }
}
