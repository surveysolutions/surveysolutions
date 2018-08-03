using MvvmCross.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

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
