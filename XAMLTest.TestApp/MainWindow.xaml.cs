using System.Windows;

namespace XAMLTest.TestApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string CommandLine => Environment.NewLine;

    public MainWindow()
    {
        InitializeComponent();
    }
}
