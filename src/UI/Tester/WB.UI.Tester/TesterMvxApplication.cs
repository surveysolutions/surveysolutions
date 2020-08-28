using MvvmCross.ViewModels;

namespace WB.UI.Tester
{
    public class TesterMvxApplication : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            RegisterCustomAppStart<TesterAppStart>();
        }
    }
}
