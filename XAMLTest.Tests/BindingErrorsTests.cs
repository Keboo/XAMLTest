namespace XamlTest.Tests;

[TestClass]
public class BindingErrorsTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IWindow? Window { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(new AppOptions
        {
            LogMessage = msg => context.WriteLine(msg),
            AllowVisualStudioDebuggerAttach = true
        });

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"");
    }
    
    [TestMethod]
    public async Task WhenBindingErrorsExists_ItReturnsThem()
    {
        await Window.SetXamlContent(@"<TextBlock Text=""{Binding NotThere, RelativeSource={RelativeSource Self}}"" />");
        
        IReadOnlyList<string> bindingErrors = await App.GetBindingErrors();

        Assert.AreEqual(1, bindingErrors.Count);
        Assert.IsTrue(bindingErrors[0].Contains("BindingExpression path error: 'NotThere' property not found on 'object' ''TextBlock'"));
    }
}
