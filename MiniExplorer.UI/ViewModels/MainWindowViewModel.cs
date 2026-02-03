namespace MiniExplorer.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ExplorerViewModel Explorer { get; }
    
    public MainWindowViewModel()
    {
        Explorer = new ExplorerViewModel();
    }
}
