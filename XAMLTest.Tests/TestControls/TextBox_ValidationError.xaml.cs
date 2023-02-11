using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace XamlTest.Tests.TestControls;

/// <summary>
/// Interaction logic for TextBox_ValidationError.xaml
/// </summary>
public partial class TextBox_ValidationError : UserControl
{
    public TextBox_ValidationError()
    {
        InitializeComponent();
        DataContext = new ViewModel();
    }

    public class ViewModel : ObservableObject
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}

public class NotEmptyValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrWhiteSpace((value ?? "").ToString())
            ? new ValidationResult(false, "Field is required.")
            : ValidationResult.ValidResult;
    }
}
