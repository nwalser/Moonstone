using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sapphire.Data.Project;
using Sapphire.Data.Project.Entities;

namespace Sapphire.Avalonia.ViewModels;

public class TodoListViewModel : ViewModelBase
{
    public TodoListViewModel(IEnumerable<Todo> items)
    {
        ListItems = new ObservableCollection<Todo>(items);
    }

    public ObservableCollection<Todo> ListItems { get; }
}