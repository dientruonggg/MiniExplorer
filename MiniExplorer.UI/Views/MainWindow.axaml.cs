using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MiniExplorer.UI.ViewModels;

namespace MiniExplorer.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DoubleTappedEvent, OnDoubleTapped, RoutingStrategies.Bubble);
    }
    
    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;
        
        // Check if we double-clicked on a file item
        if (vm.Explorer.SelectedItem != null)
        {
            vm.Explorer.ItemDoubleClickCommand.Execute(vm.Explorer.SelectedItem);
        }
    }
}