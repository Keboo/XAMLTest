using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public async Task SetValidationError_WithoutBinding_SetsValidationError()
    {
        //Arrange 
        await using TestRecorder recorder = new(App);

        const string expectedErrorMessage = "Custom validation error";
        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox />");
        IValidation<TextBox> validation = textBox.Validation();

        //Act
        await validation.SetValidationError(TextBox.TextProperty, expectedErrorMessage);
        var hasError = await validation.GetHasError();
        var validationError = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.IsTrue(hasError);
        Assert.AreEqual(expectedErrorMessage, validationError);

        recorder.Success();
    }

    [TestMethod]
    public async Task SetValidationError_WithExistingBinding_SetsValidationError()
    {
        //Arrange 
        await using TestRecorder recorder = new(App);

        const string expectedErrorMessage = "Custom validation error";
        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox Text=""{Binding RelativeSource={RelativeSource Self}, Path=Tag}"" />");
        IValidation<TextBox> validation = textBox.Validation();

        //Act
        await validation.SetValidationError(TextBox.TextProperty, expectedErrorMessage);
        var hasError = await validation.GetHasError();
        var validationError = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.IsTrue(hasError);
        Assert.AreEqual(expectedErrorMessage, validationError);

        recorder.Success();
    }

    [TestMethod]
    public async Task ClearValidationError_ClearsValidationError()
    {
        //Arrange 
        await using TestRecorder recorder = new(App);

        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox />");
        IValidation<TextBox> validation = textBox.Validation();
        await validation.SetValidationError(TextBox.TextProperty, "Some error");

        //Act
        await validation.ClearValidationError(TextBox.TextProperty);
        var hasError = await validation.GetHasError();
        var validationError = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.IsFalse(hasError);
        Assert.IsNull(validationError);

        recorder.Success();
    }

    [TestMethod]
    public async Task SetValidationRule_WithoutBinding_AppliesValidationRule()
    {
        //Arrange 
        await using TestRecorder recorder = new(App);

        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox />");
        IValidation<TextBox> validation = textBox.Validation();

        //Act
        await validation.SetValidationRule<CustomValidationRule>(TextBox.TextProperty);
        var hasError = await validation.GetHasError();
        var validationError = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.IsTrue(hasError);
        Assert.AreEqual(CustomValidationRule.ErrorObject, validationError);

        recorder.Success();
    }

    [TestMethod]
    public async Task SetValidationRule_WithExistingBinding_AppliesValidationRule()
    {
        //Arrange 
        await using TestRecorder recorder = new(App);

        IVisualElement<TextBox> textBox = await Window.SetXamlContent<TextBox>(@"<TextBox Text=""{Binding RelativeSource={RelativeSource Self}, Path=Tag}""/>");
        IValidation<TextBox> validation = textBox.Validation();

        //Act
        await validation.SetValidationRule<CustomValidationRule>(TextBox.TextProperty);
        await textBox.SetTag("foo");
        var hasError = await validation.GetHasError();
        var validationError = await validation.GetValidationError<string>(TextBox.TextProperty);

        //Assert 
        Assert.IsTrue(hasError);
        Assert.AreEqual(CustomValidationRule.ErrorObject, validationError);

        recorder.Success();
    }

    private class CustomValidationRule : ValidationRule
    {
        public const string ErrorObject = "Custom Validation Rule Failure";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => new ValidationResult(false, ErrorObject);
    }
}
