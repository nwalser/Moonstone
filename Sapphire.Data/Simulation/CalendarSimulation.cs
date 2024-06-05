namespace Sapphire.Data.Simulation;

public static class CalendarSimulation
{
    public static void RunSimulation(ProjectDatabase db, DateOnly start, DateOnly stop)
    {
        var days = Enumerable
            .Range(0, stop.DayNumber - start.DayNumber + 1)
            .Select((d, index) => (Day: start.AddDays(d), Index: index))
            .ToList();

        Thread.Sleep(2000);
        //throw new NotImplementedException();
    }
}