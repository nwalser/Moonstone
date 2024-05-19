using CommunityToolkit.Maui.Extensions;

namespace Amber.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            if (window != null)
            {
                window.Title = "Sapphire";
            }

            return window;
        }
    }
}
