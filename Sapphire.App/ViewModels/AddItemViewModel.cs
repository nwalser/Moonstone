﻿using System;
using System.Reactive;
using ReactiveUI;
using Sapphire.Domain;

namespace Sapphire.App.ViewModels;

public class AddItemViewModel : ViewModelBase
{
    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
    
    public ReactiveCommand<Unit, Todo> OkCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        
    public AddItemViewModel()
    {
        var isValidObservable = this.WhenAnyValue(
            x => x.Description,
            x => !string.IsNullOrWhiteSpace(x));

        OkCommand = ReactiveCommand.Create(() => new Todo()
        {
            Id = Guid.NewGuid(),
            LastWrite = DateTime.UtcNow,
            IsChecked = false,
            Description = Description,
        }, isValidObservable);
        CancelCommand = ReactiveCommand.Create(() => { });
    }
}