using Sapphire.Data.Entities;
using Sapphire.Data.Entities.Todo;
using Sapphire.Data.Extensions;
using Sapphire.Data.ValueObjects;

namespace Sapphire.Data.Simulation;

public static class CalendarSimulation
{
    public static void RunSimulation(ProjectDatabase db, DateOnly start, DateOnly stop, Action<double>? progressCallback = default)
    {
        db.PlannedAllocations.Clear();
        
        var days = Enumerable
            .Range(0, stop.DayNumber - start.DayNumber + 1)
            .Select((d, index) => (Day: start.AddDays(d), Index: index))
            .ToList();

        var prioritizedTodos = GetPrioritizedTodos(db)
            .OrderByDescending(t => t.Key)
            .Select(t => t.Value)
            .ToList();

        var workers = db.Enumerate<WorkerAggregate>()
            .ToList();
        
        foreach (var (day, i) in days)
        {
            var possibleTodos = prioritizedTodos
                .Where(t => !t.GetActiveLocks(db, day).Any())
                .ToList();

            foreach (var worker in workers)
            {
                var todosForWorker = possibleTodos
                    .Where(p => p.GetPossibleWorkerIds(db).Contains(worker.Id))
                    .ToList();

                foreach (var todoForWorker in todosForWorker)
                {
                    var availableHours = worker.GetAvailableHours(db, day);
                    if (availableHours <= TimeSpan.Zero)
                        break; // do not pick up work if worker has no available time

                    var assignedWorkers = todoForWorker.GetAssignedWorkers(db);
                    if (assignedWorkers.Any(w => w.Id != worker.Id) && !todoForWorker.Splittable)
                        continue; // do not pick up work if there is any other assigned worker and task is not splittable
                    
                    var project = todoForWorker.GetProject(db);
                    var remainingEffort = todoForWorker.GetRemainingUnplannedEffort(db);
                    var remainingAllocatable = project.GetRemainingAllocatable(db, day, worker.Id);
                    
                    var allocatableTime = TimeSpanExtensions.Min([availableHours, remainingEffort, remainingAllocatable]);
                    if (allocatableTime <= TimeSpan.Zero)
                        continue;

                    db.PlannedAllocations.Add(new PlannedAllocation()
                    {
                        Date = day,
                        PlannedTime = allocatableTime,
                        TodoId = todoForWorker.Id,
                        WorkerId = worker.Id
                    });
                }
            }
            
            progressCallback?.Invoke((double)i / days.Count);
        }
    }


    public static Dictionary<int, TodoAggregate> GetPrioritizedTodos(ProjectDatabase db)
    {
        var currentPriority = 0;
        var prioritized = new Dictionary<int, TodoAggregate>();
        
        var projects = db.Enumerate<ProjectAggregate>();
        
        // walk tree of todos and assign priorities, start with lowest priority first
        foreach (var project in projects.OrderBy(p => p.Priority))
        {
            var rootTodos = project.GetRootTodos(db);
            
            AssignPrioritiesToChildren(db, rootTodos, ref currentPriority, ref prioritized);
        }

        return prioritized;
    }

    private static void AssignPrioritiesToChildren(ProjectDatabase db, IEnumerable<TodoAggregate> todos, ref int currentPriority, ref Dictionary<int, TodoAggregate> prioritized)
    {
        foreach (var todo in todos.OrderByDescending(t => t.Order))
        {
            prioritized.Add(currentPriority++, todo);

            var childTodos = todo.GetChildTodos(db);
            AssignPrioritiesToChildren(db, childTodos, ref currentPriority, ref prioritized);
        }
    }
}