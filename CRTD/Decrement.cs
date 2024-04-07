namespace CRTD;

public class Decrement(int value) : Command
{
    public int Value { get; set; } = value;
}