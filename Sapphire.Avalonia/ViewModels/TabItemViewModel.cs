using CommunityToolkit.Mvvm.ComponentModel;

namespace Sapphire.Avalonia.ViewModels;

public class TabItemViewModel : ObservableObject
{
    public string Header { get; set; }
        
    public string SimpleContent { get; set; }


    public override string ToString() => Header;
}