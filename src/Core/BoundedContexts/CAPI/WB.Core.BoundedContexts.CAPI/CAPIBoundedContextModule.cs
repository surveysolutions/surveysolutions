using Main.Core.View;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Capi
{
    public class CapiBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
               .To<QuestionnaireScreenViewFactory>().InSingletonScope();
        }
    }
}
