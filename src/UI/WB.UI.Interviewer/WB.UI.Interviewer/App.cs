using Cirrious.CrossCore.IoC;
using WB.Core.BoundedContexts.Capi.ViewModel;

namespace WB.UI.Interviewer
{
    public class App : Cirrious.MvvmCross.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            RegisterAppStart<InterviewViewModel>();
        }
    }
}