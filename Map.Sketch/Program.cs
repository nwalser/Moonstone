using Map.Sketch.Domain;
using Map.Sketch.Store;


var tableMap = new Dictionary<int, Type>()
{
    { 0, typeof(Todo) }
};

var database = new Dictionary<int, Dictionary<Guid, object>>();

var todo = new Todo()
{
    Id = Guid.NewGuid(),
    Name = "Todo 1"
};

var mutation = new Mutation()
{
    Table = 0,
    Row = Guid.NewGuid(),
    Column = 0,
    Tick = DateTime.UtcNow.Ticks,
    Value = BitConverter.GetBytes(10)
};









