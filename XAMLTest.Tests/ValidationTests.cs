using System.Collections.ObjectModel;
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
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"");
    }

    [ClassCleanup(Microsoft.VisualStudio.TestTools.UnitTesting.InheritanceBehavior.BeforeEachDerivedClass)]
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
        
        IVisualElement<TextBox_ValidationError> userControl = await Window.SetXamlContentFromUserControl<TextBox_ValidationError>();
        IVisualElement<TextBox> textBox = await userControl.GetElement<TextBox>();

        //Act
        ReadOnlyObservableCollection<ValidationError>? errors = await textBox.GetProperty<ReadOnlyObservableCollection<ValidationError>>(System.Windows.Controls.Validation.ErrorsProperty);

        //Assert
        Assert.IsNotNull(errors);
        Assert.HasCount(1, errors);
        Assert.IsInstanceOfType(errors[0].RuleInError, typeof(NotEmptyValidationRule));
        Assert.AreEqual("Field is required.", errors[0].ErrorContent);

        recorder.Success();
    }

    private class CustomValidationRule : ValidationRule
    {
        public const string ErrorObject = "Custom Validation Rule Failure";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => new(false, ErrorObject);
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
            var tokens = value.Split(SeparatorChar);

            var ruleInError = rootSerializer.Deserialize(typeof(NotEmptyValidationRule), tokens[0], rootSerializer) as ValidationRule;
            var errorContent = tokens[1];
            return new ValidationError(ruleInError, new object(), errorContent, null);
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
                var result = string.Join(SeparatorChar, collection.Select(e => rootSerializer.Serialize(e.GetType(), e, rootSerializer)));
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
