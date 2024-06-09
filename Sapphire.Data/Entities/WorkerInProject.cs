using Moonstone.Database;
using Sapphire.Data.Entities.MaximalAllocation;

namespace Sapphire.Data.Entities;

public class WorkerInProject : Document
{
    public Guid WorkerId { get; set; }
    public Guid ProjectId { get; set; }
    
    
    public IEnumerable<WeeklyAllocationRule> GetWeeklyAllocations(ProjectDatabase db)
    {
        return db.Enumerate<WeeklyAllocationRule>()
            .Where(w => w.ProjectId == ProjectId)
            .Where(w => w.WorkerId == WorkerId);
    }
    
    public IEnumerable<DailyAllocationRule> GetDailyAllocations(ProjectDatabase db)
    {
        return db.Enumerate<DailyAllocationRule>()
            .Where(w => w.ProjectId == ProjectId)
            .Where(w => w.WorkerId == WorkerId);
    }
}