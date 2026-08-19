using LKBConvertor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LKBConvertor
{
    public partial class App : Application
    {
        private readonly IServiceProvider _sp;

        public App(IServiceProvider sp)
        {
            InitializeComponent();
            _sp = sp;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var home = _sp.GetRequiredService<HomePage>();
            return new Window(new NavigationPage(home));
        }
    }
}
