namespace CRTD;

public class Counter
{
    public int Value { get; set; }
    
    
    public void ProcessCommand(Command command)
    {
        switch (command)
        {
            case Increment increment:
                Value += increment.Value;
                break;
            case Decrement decrement:
                Value -= decrement.Value;
                break;
        }
        
        Console.WriteLine($"{GetHashCode()}: {Value}");
    }
}