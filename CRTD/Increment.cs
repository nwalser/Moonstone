namespace CRTD;

public class Increment(int value) : Command
{
    public int Value { get; set; } = value;
}