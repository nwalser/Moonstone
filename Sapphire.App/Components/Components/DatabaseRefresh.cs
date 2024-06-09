using Microsoft.AspNetCore.Components;
using Sapphire.Data;

namespace Sapphire.App.Components.Components;

public class DatabaseRefresh : ComponentBase, IDisposable
{
    [CascadingParameter] public required ProjectDatabase Database { get; set; }

    private IDisposable? _lastUpdateSubscription;
    private IDisposable? _lastSimulationSubscription;
    
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _lastUpdateSubscription = Database.LastUpdate.Subscribe(_ =>
            {
                OnLoadData();
                InvokeAsync(StateHasChanged);
            });
            
            _lastSimulationSubscription = Database.LastSimulation.Subscribe(_ =>
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
        _lastUpdateSubscription?.Dispose();
        _lastSimulationSubscription?.Dispose();
    }
}