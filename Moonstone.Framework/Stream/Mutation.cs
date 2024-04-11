using System.Diagnostics;
using RT.Comb;

namespace Moonstone.Framework.Stream;

public abstract class Mutation
{
    public Guid Id { get; init; }
    //public DateTime Occurence { get; init; }

    public Mutation()
    {
        var pro = new PostgreSqlCombProvider(new SqlDateTimeStrategy());
        
        Id = pro.Create();
    }
}