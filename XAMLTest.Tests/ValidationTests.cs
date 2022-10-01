using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using XamlTest.Tests.TestControls;

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

    [TestMethod]
    public async Task GetProperty_TextBoxWithValidationError_ReturnsListOfValidationErrors()
    {
        //Arrange
        await using TestRecorder recorder = new(App);

        await App.RegisterSerializer<NotEmptyValidationRuleSerializer>();
        await App.RegisterSerializer<ValidationErrorSerializer>();
        await App.RegisterSerializer<ValidationErrorReadOnlyObservableCollectionSerializer>();
        IWindow window = await App.CreateWindowWithUserControl<TextBox_ValidationError>();
        IVisualElement<TextBox> textBox = await window.GetElement<TextBox>("/TextBox");

        //Act
        ReadOnlyObservableCollection<ValidationError>? errors = await textBox.GetProperty<ReadOnlyObservableCollection<ValidationError>>(System.Windows.Controls.Validation.ErrorsProperty);

        //Assert
        Assert.IsNotNull(errors);
        var errorList = errors.ToList();
        Assert.IsInstanceOfType(errorList[0].RuleInError, typeof(NotEmptyValidationRule));
        Assert.AreEqual("Field is required.", errorList[0].ErrorContent);

        recorder.Success();
    }

    private class CustomValidationRule : ValidationRule
    {
        public const string ErrorObject = "Custom Validation Rule Failure";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => new ValidationResult(false, ErrorObject);
    }


    private class NotEmptyValidationRuleSerializer : ISerializer
    {
        public bool CanSerialize(Type type, ISerializer rootSerializer) => typeof(NotEmptyValidationRule).IsAssignableFrom(type);

        public string Serialize(Type type, object? value, ISerializer rootSerializer) => "*";   // No actual properties to serialize

        public object? Deserialize(Type type, string value, ISerializer rootSerializer) => new NotEmptyValidationRule();
    }

    private class ValidationErrorSerializer : ISerializer
    {

        private static char SeparatorChar = ';';

        public bool CanSerialize(Type type, ISerializer rootSerializer) => typeof(ValidationError).IsAssignableFrom(type);

        public string Serialize(Type type, object? value, ISerializer rootSerializer)
        {
            if (value is ValidationError { RuleInError: NotEmptyValidationRule rule } error)
            {
                return rootSerializer.Serialize(typeof(NotEmptyValidationRule), rule, rootSerializer) + SeparatorChar + error.ErrorContent;
            }
            return string.Empty;
        }

        public object? Deserialize(Type type, string value, ISerializer rootSerializer)
        {
            // Create uninitialized version of ValidationError because I don't really care about the Binding at this time - and don't want to add, yet another, serializer for it :)
            var error = (ValidationError)FormatterServices.GetSafeUninitializedObject(type);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var tokens = value.Split(SeparatorChar);

            error.RuleInError = rootSerializer.Deserialize(typeof(CustomValidationRule), tokens[0], rootSerializer) as ValidationRule;
            error.ErrorContent = tokens[1];

            return error;
        }
    }

    private class ValidationErrorReadOnlyObservableCollectionSerializer : ISerializer
    {
        private static char SeparatorChar = '&';

        public bool CanSerialize(Type type, ISerializer rootSerializer) => typeof(ReadOnlyObservableCollection<ValidationError>).IsAssignableFrom(type);

        public string Serialize(Type type, object? value, ISerializer rootSerializer)
        {
            if (value is ReadOnlyObservableCollection<ValidationError> collection)
            {
                var result = string.Join(SeparatorChar, collection.Select(e => rootSerializer.Serialize(type, e, rootSerializer)));
                return result;
            }
            return string.Empty;
        }

        public object? Deserialize(Type type, string value, ISerializer rootSerializer)
        {
            var collection = new ObservableCollection<ValidationError>();

            if (!string.IsNullOrWhiteSpace(value))
            {
                var tokens = value.Split(SeparatorChar);
                foreach (var errorString in tokens)
                {
                    if (rootSerializer.Deserialize(typeof(ValidationError), errorString, rootSerializer) is ValidationError error)
                    {
                        collection.Add(error);
                    }
                }
            }
            return new ReadOnlyObservableCollection<ValidationError>(collection);
        }
    }

}
