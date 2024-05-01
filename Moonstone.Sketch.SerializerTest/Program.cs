using MessagePack;
using MessagePack.Formatters;

var p1 = new Parent()
{
    Value1 = "Hello World",
    Child = new ChildObject2()
    {
        Value3 = "Hello there"
    }
};

var item = MessagePackSerializer.Serialize(p1);

var parent = MessagePackSerializer.Typeless.Deserialize(item) as Parent;


Console.WriteLine(parent.Value1);


    
    
    
[MessagePackObject]
public class Parent
{
    [Key(0)]
    public required string Value1 { get; set; }
    
    [Key(1)]
    public ChildObject Child { get; set; }
}

public interface ChildObject
{
    
}

[MessagePackObject]
public class ChildObject2 : ChildObject
{
    [Key(0)]
    public string Value3 { get; set; }
}


[MessagePackObject]
public class Child : Parent
{
    [Key(2)]
    public required string Value2 { get; set; }
}