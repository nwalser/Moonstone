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

        
        
        
        
        
        Thread.Sleep(2000);
        //throw new NotImplementedException();
        
        // todo: implement time summation of tasks
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

        Console.WriteLine(prioritized.Count);
        return prioritized;
    }

    private static void AssignPrioritiesToChildren(ProjectDatabase db, IEnumerable<TodoAggregate> todos, ref int currentPriority, ref Dictionary<int, TodoAggregate> prioritized)
    {
        foreach (var todo in todos.OrderByDescending(t => t.Order))
        {
            var childTodos = todo.GetChildTodos(db);
            AssignPrioritiesToChildren(db, childTodos, ref currentPriority, ref prioritized);
            
            prioritized.Add(currentPriority++, todo);
        }
    }
}