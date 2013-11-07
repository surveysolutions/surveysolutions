using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using Ninject.Modules;
using WB.Core.BoundedContexts.CAPI.Views.InterviewDetails;
using WB.Core.BoundedContexts.CAPI.Views.Statistics;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.CAPI
{
    public class CAPIBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
               .To<QuestionnaireScreenViewFactory>().InSingletonScope();


            this.Bind<IViewFactory<StatisticsInput, StatisticsViewModel>>().To<StatisticsViewFactory>();
        }
    }
}
