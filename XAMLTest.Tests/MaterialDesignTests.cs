using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Media;
using XamlTest;

[assembly: GenerateHelpers(typeof(ComboBoxPopup))]

namespace XamlTest.Tests;

[TestClass]
public class MaterialDesignTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);
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
    [Ignore]
    [Description("We are simply testing that the generator generates extension methods for nullable brush properties. Compilation is all we need.")]
    public async Task ComboBox_UsesDropDownBackgroundResource_WhenBackgroundIsNotSet()
    {
        var stackPanel = await LoadXaml<StackPanel>($$"""
             <StackPanel>
               <ComboBox Width="200">
                 <ComboBox.Resources>
                   <SolidColorBrush x:Key="MaterialDesign.Brush.ComboBox.DropDown.Background"
                                    Color="#CC336699" />
                 </ComboBox.Resources>
                 <ComboBoxItem Content="Android" />
                 <ComboBoxItem Content="iOS" />
                 <ComboBoxItem Content="Linux" />
               </ComboBox>
             </StackPanel>
             """);

        var comboBox = await stackPanel.GetElement<ComboBox>();
        await comboBox.LeftClick(Position.RightCenter);

        var popup = await Wait.For(async () => await comboBox.GetElement<ComboBoxPopup>("PART_Popup"));
        Brush? background = await popup.GetBackground();
        Color? backgroundColor = await popup.GetBackgroundColor();
    }

    private static async Task InitializeWithMaterialDesign(IApp app,
        BaseTheme baseTheme = BaseTheme.Light,
        PrimaryColor primary = PrimaryColor.DeepPurple,
        SecondaryColor secondary = SecondaryColor.Lime,
        ColorAdjustment? colorAdjustment = null)
    {
        string colorAdjustString = "";
        if (colorAdjustment != null)
        {
            colorAdjustString = "ColorAdjustment=\"{materialDesign:ColorAdjustment}\"";
        }

        string applicationResourceXaml = $@"<ResourceDictionary 
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"">
    <ResourceDictionary.MergedDictionaries>
        <materialDesign:BundledTheme BaseTheme=""{baseTheme}"" PrimaryColor=""{primary}"" SecondaryColor=""{secondary}"" {colorAdjustString}/>

        <ResourceDictionary Source=""pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/Generic.xaml"" />
        <ResourceDictionary Source=""pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"" />
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>";

        await app.Initialize(applicationResourceXaml,
            Path.GetFullPath("MaterialDesignColors.dll"),
            Path.GetFullPath("MaterialDesignThemes.Wpf.dll"),
            Assembly.GetExecutingAssembly().Location);
    }

    private async Task<IVisualElement<T>> LoadXaml<T>(string xaml, params (string namespacePrefix, Type type)[] additionalNamespaceDeclarations)
    {
        await InitializeWithMaterialDesign(App);
        return await CreateWindowWith<T>(App, xaml, additionalNamespaceDeclarations);
    }

    private static async Task<IVisualElement<T>> CreateWindowWith<T>(IApp app, string xaml, params (string namespacePrefix, Type type)[] additionalNamespaceDeclarations)
    {
        var extraNamespaceDeclarations = new StringBuilder("");
        foreach ((string namespacePrefix, Type type) in additionalNamespaceDeclarations)
        {
            extraNamespaceDeclarations.AppendLine($@"xmlns:{namespacePrefix}=""clr-namespace:{type.Namespace};assembly={type.Assembly.GetName().Name}""");
        }

        string windowXaml = @$"<Window
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
        xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
        xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes""
        {extraNamespaceDeclarations}
        mc:Ignorable=""d""
        Height=""800"" Width=""1100""
        TextElement.Foreground=""{{DynamicResource MaterialDesignBody}}""
        TextElement.FontWeight=""Regular""
        TextElement.FontSize=""13""
        TextOptions.TextFormattingMode=""Ideal"" 
        TextOptions.TextRenderingMode=""Auto""
        Background=""{{DynamicResource MaterialDesignPaper}}""
        FontFamily=""{{materialDesign:MaterialDesignFont}}"" 
        Title=""Test Window""
        Topmost=""True""
        WindowStartupLocation=""CenterScreen"">
        {xaml}
</Window>";
        IWindow window = await app.CreateWindow(windowXaml);
        return await window.GetElement<T>(".Content");
    }
}
