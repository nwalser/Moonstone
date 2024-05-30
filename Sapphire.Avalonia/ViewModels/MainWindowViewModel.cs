using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Moonstone.Database;
using ReactiveUI;
using Sapphire.Data.Project;
using Sapphire.Data.Project.Entities;

namespace Sapphire.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _contentViewModel;
    public IDatabase Database { get; }
    public TodoListViewModel TodoListViewModel { get; }
    
    public ObservableCollection<TabItemViewModel> TabItems { get; } = new();
    public Func<object> NewItemFactory => AddTab;

    
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
    
    
    public MainWindowViewModel()
    {
        var path = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace17";
        var session = "session1";

        var typeMap = new Dictionary<int, Type>
        {
            { 0, typeof(Todo) }
        };
        
        var database = new Database(typeMap, session, path);
        Database = database;
        database.Open();
        
        TodoListViewModel = new TodoListViewModel(Database.Enumerate<Todo>());
        _contentViewModel = TodoListViewModel;
    }

    public void AddItem()
    {
        AddItemViewModel addItemViewModel = new();

        Observable.Merge(
                addItemViewModel.OkCommand,
                addItemViewModel.CancelCommand.Select(_ => (Todo?)null))
            .Take(1)
            .Subscribe(newItem =>
            {
                if (newItem != null)
                {
                    Database.Update(newItem);
                }
                ContentViewModel = new TodoListViewModel(Database.Enumerate<Todo>());;
            });
        
        ContentViewModel = addItemViewModel;
    }
    
    private object AddTab()
    {
        return new TabItemViewModel
        {
            Header = $"Tab",
            SimpleContent = $"Tab content"
        };
    }
}