using System.Reactive.Subjects;
using Moonstone.Database;
using Sapphire.Data.Entities;
using Sapphire.Data.Entities.WorkingHours;
using Sapphire.Data.ValueObjects;

namespace Sapphire.Data;

public class ProjectDatabase : Database, IDisposable
{
    private readonly BehaviorSubject<DateTime> _lastSimulation = new(DateTime.MinValue);
    public BehaviorSubject<DateTime> LastSimulation => _lastSimulation;
    
    private readonly BehaviorSubject<bool> _simulationOngoing = new(false);
    public BehaviorSubject<bool> SimulationOngoing => _simulationOngoing;

    private IDisposable? _subscription;
    
    
    public required List<PlannedTodo> PlannedTodos { get; set; }
    public required List<PlannedAllocation> PlannedAllocations { get; set; }
    
    
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(ProjectAggregate) },
        { 1, typeof(TodoAggregate) },
        { 2, typeof(PossibleWorkerAssignment) },
        { 3, typeof(WorkerAggregate) },
        
        { 4, typeof(LeaveDayAggregate) },
        { 5, typeof(OfficeDayAggregate) },
        { 6, typeof(WeeklyWorkDay) },
    };

    public override void Create(string path, string session)
    {
        base.Create(path, session);
        
        // create initial objects
        var project = new ProjectAggregate()
        {
            Name = "Project Name",
            PossibleTags = ["Backend", "Frontend", "Operations"]
        };
        Update(project);
        Update(new TodoAggregate()
        {
            Name = "Todo 1",
            ProjectId = project.Id,
        });
        Update(new TodoAggregate()
        {
            Name = "Todo 2",
            ProjectId = project.Id,
        });
    }

    protected override void OnAfterOpening()
    {
        _subscription = LastUpdate.Subscribe(async t => await RunSimulation(t));
        
        base.OnAfterOpening();
    }

    private async Task RunSimulation(DateTime change)
    {
        await Task.Run(() =>
        {
            // todo: implement skipping of intermediary simulations
            Monitor.Enter(this);
            try
            {
                SimulationOngoing.OnNext(true);

                // simulate
                Thread.Sleep(2000);

                LastSimulation.OnNext(change);
                SimulationOngoing.OnNext(false);
            }
            finally
            {
                Monitor.Exit(this);
            }
        });
    }

    public new void Dispose()
    {
        _lastSimulation.Dispose();
        _simulationOngoing.Dispose();
        _subscription?.Dispose();
        
        base.Dispose();
    }
}