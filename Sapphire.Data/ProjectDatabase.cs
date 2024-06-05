using System.Reactive.Subjects;
using Moonstone.Database;
using Sapphire.Data.Entities;
using Sapphire.Data.Entities.WorkingHours;
using Sapphire.Data.Extensions;

namespace Sapphire.Data;

public class ProjectDatabase : Database
{
    private readonly BehaviorSubject<DateTime> _lastSimulation = new(DateTime.MinValue);
    public BehaviorSubject<DateTime> LastSimulation => _lastSimulation;
    
    private readonly BehaviorSubject<bool> _simulationOngoing = new(false);
    public BehaviorSubject<bool> SimulationOngoing => _simulationOngoing;
    
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
        LastUpdate.SubscribeWithoutOverlap(RunSimulation);
        base.OnAfterOpening();
    }

    private void RunSimulation(DateTime change)
    {
        SimulationOngoing.OnNext(true);
        
        // simulate
        Thread.Sleep(2000);
        
        LastSimulation.OnNext(change);
        SimulationOngoing.OnNext(false);
    }
}