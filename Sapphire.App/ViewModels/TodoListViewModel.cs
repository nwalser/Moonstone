using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sapphire.Domain;

namespace Sapphire.App.ViewModels;

public class TodoListViewModel : ViewModelBase
{
    public TodoListViewModel(IEnumerable<Todo> items)
    {
        ListItems = new ObservableCollection<Todo>(items);
    }

    public ObservableCollection<Todo> ListItems { get; }
}