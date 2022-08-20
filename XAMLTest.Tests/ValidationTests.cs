using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace XamlTest.Tests;

[TestClass]
public class ValidationTests
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
    public async Task MarkInvalid_WhenPropertyIsDependencyObjectWithoutExistingBinding_SetsValidationError()
    {
        // Arrange 
        await using TestRecorder recorder = new(App);

        const string expectedErrorMessage = "Custom validation error";
        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox />");
        IValidation<TextBox> validation = textBox.Validation();

        //Act (set validation)
        await validation.SetValidationError(TextBox.TextProperty, expectedErrorMessage);
        var result1 = await textBox.GetProperty<bool>(System.Windows.Controls.Validation.HasErrorProperty);
        var validationError1 = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Act (clear validation) 
        await validation.ClearValidationError(TextBox.TextProperty);
        var result2 = await textBox.GetProperty<bool>(System.Windows.Controls.Validation.HasErrorProperty);
        var validationError2 = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.AreEqual(true, result1);
        Assert.AreEqual(expectedErrorMessage, validationError1);
        Assert.AreEqual(false, result2);
        Assert.IsNull(validationError2);

        recorder.Success();
    }

    [TestMethod]
    public async Task MarkInvalid_WhenPropertyIsDependencyObjectWithExistingBinding_SetsValidationError()
    {
        // Arrange 
        await using TestRecorder recorder = new(App);

        const string expectedErrorMessage = "Custom validation error";
        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox Text=""{Binding RelativeSource={RelativeSource Self}, Path=Tag}"" />");
        IValidation<TextBox> validation = textBox.Validation();

        //Act (set validation) 
        await validation.SetValidationError(TextBox.TextProperty, expectedErrorMessage);
        var result1 = await textBox.GetProperty<bool>(System.Windows.Controls.Validation.HasErrorProperty);
        var validationError1 = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Act (clear validation) 
        await validation.ClearValidationError(TextBox.TextProperty);
        var result2 = await textBox.GetProperty<bool>(System.Windows.Controls.Validation.HasErrorProperty);
        var validationError2 = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.AreEqual(true, result1);
        Assert.AreEqual(expectedErrorMessage, validationError1);
        Assert.AreEqual(false, result2);
        Assert.IsNull(validationError2);

        recorder.Success();
    }
}
