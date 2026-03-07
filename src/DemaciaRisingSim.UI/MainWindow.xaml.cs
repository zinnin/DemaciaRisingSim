using System.Windows;

namespace DemaciaRisingSim.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private void LoadDefault_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadDefault();
    }

    private async void Optimize_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.OptimizeAsync();
    }
}