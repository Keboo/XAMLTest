using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XamlTest.Tests.TestControls;

namespace XamlTest.Tests;

[TestClass]
public class PositionTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    public static IVisualElement<MouseClickPositions>? UserControl { get; set; }

    [NotNull]
    public static IVisualElement<TextBlock>? PositionTextElement { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        var window = await App.CreateWindowWithUserControl<MouseClickPositions>(windowSize: new(400, 300));
        UserControl = await window.GetElement<MouseClickPositions>("/MouseClickPositions");
        PositionTextElement = await UserControl.GetElement<TextBlock>("ClickLocation");
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

    [DataTestMethod]
    [DataRow(Position.Center, "182x120")]
    [DataRow(Position.TopLeft, "0x0")]
    [DataRow(Position.TopCenter, "182x0")]
    [DataRow(Position.TopRight, "364x0")]
    [DataRow(Position.RightCenter, "364x120")]
    [DataRow(Position.BottomRight, "364x241")]
    [DataRow(Position.BottomCenter, "182x241")]
    [DataRow(Position.BottomLeft, "0x241")]
    [DataRow(Position.LeftCenter, "0x120")]
    public async Task CanClickAtAllPositions(Position position, string expectedValue)
    {
        await UserControl.LeftClick(position);

        string? clickPositon = await PositionTextElement.GetText();
        Assert.AreEqual(expectedValue, clickPositon);
    }
}
