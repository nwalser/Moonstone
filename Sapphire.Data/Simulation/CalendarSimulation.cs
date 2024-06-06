using Sapphire.Data.Entities;

namespace Sapphire.Data.Simulation;

public static class CalendarSimulation
{
    public static void RunSimulation(ProjectDatabase db, DateOnly start, DateOnly stop, Action<double>? progressCallback = default)
    {
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
                    .Where(p => p.PossibleWorkerIds.Contains(worker.Id));

                foreach (var todoForWorker in todosForWorker)
                {
                    var availableHours = worker.PlannedHours(db, day);

                    if (availableHours <= TimeSpan.Zero)
                        break;

                    // todo: do not pickup work if other is assigned to task and todo is not splittable
                    
                    
                    
                    
                }
            }
        }
        
        
        
        
        Thread.Sleep(2000);
        //throw new NotImplementedException();
    }


    public static Dictionary<int, TodoAggregate> GetPrioritizedTodos(ProjectDatabase db)
    {
        var currentPriority = 0;
        var prioritized = new Dictionary<int, TodoAggregate>();
        
        var projects = db.Enumerate<ProjectAggregate>();
        
        // walk tree of todos and assign priorities, start with lowest priority first
        foreach (var project in projects.OrderByDescending(p => p.Priority))
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