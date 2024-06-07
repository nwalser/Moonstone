using System.Reactive.Subjects;
using Moonstone.Database;
using Sapphire.Data.Entities;
using Sapphire.Data.Entities.WorkingHours;
using Sapphire.Data.Simulation;
using Sapphire.Data.ValueObjects;

namespace Sapphire.Data;

public class ProjectDatabase : Database, IDisposable
{
    public BehaviorSubject<DateTime> LastSimulation { get; private set; } = new(DateTime.MinValue);
    public BehaviorSubject<bool> SimulationOngoing { get; private set; } = new(false);
    public BehaviorSubject<double> SimulationProgress { get; private set; } = new(0);
    
    public List<PlannedTodo> PlannedTodos { get; set; } = [];
    public List<PlannedAllocation> PlannedAllocations { get; set; } = [];
    
    
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

    public async Task RunSimulation(DateTime change)
    {
        await Task.Run(() =>
        {
            // todo: implement skipping of intermediary simulations
            Monitor.Enter(this);
            try
            {
                SimulationOngoing.OnNext(true);

                var start = DateOnly.FromDateTime(DateTime.UtcNow);
                var end = start.AddDays(2 * 365);
                
                // simulate
                CalendarSimulation.RunSimulation(this, start, end, progress => SimulationProgress.OnNext(progress));

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
        LastSimulation.Dispose();
        SimulationOngoing.Dispose();
        SimulationProgress.Dispose();
        
        base.Dispose();
    }
}