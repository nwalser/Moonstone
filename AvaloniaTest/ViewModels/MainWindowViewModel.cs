namespace AvaloniaTest.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public double Degrees { get; set; } = 0;
    public double Fahrenheit => Degrees * (9d / 5d) + 32;
}