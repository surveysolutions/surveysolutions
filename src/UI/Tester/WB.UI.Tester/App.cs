using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            //fix for Thai calendar (KP-6403)
            var thai = new System.Globalization.ThaiBuddhistCalendar();

            RegisterAppStart(new AppStart(this, Mvx.Resolve<IPrincipal>(), Mvx.Resolve<IViewModelNavigationService>()));
        }
    }
}
