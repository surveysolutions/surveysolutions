using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Capi.Syncronization;

namespace WB.UI.Shared.Android
{
    public class CapiBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>().To<QuestionnaireScreenViewFactory>().InSingletonScope();

            this.Bind<ICapiCleanUpService>().To<CapiCleanUpService>();
        }
    }
}
