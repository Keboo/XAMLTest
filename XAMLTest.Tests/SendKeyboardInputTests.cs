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
        await TabControl.SetSelectedIndex(0);
        await TextBox1.SetText("");
        await TextBox2.SetText("");
        await TextBox3.SetText("");
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
    public async Task SendInput_WithCopyPasteModifiers_CopyPasteViaClipboardWorks() {
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
}
