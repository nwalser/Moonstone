using Microsoft.AspNetCore.Components;
using Sapphire.Data;

namespace Sapphire.App.Components.Components;

public class DatabaseRefresh : ComponentBase, IDisposable
{
    [CascadingParameter] public required ProjectDatabase Database { get; set; }

    private IDisposable? _subscription;
    
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _subscription = Database.LastUpdate.Subscribe(_ =>
            {
                OnLoadData();
                InvokeAsync(StateHasChanged);
            });
        }

        return base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        OnLoadData();
        base.OnParametersSet();
    }

    protected virtual void OnLoadData() { }
    

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}