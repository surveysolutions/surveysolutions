using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Capi.Views.Statistics;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Capi
{
    public class CapiBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
               .To<QuestionnaireScreenViewFactory>().InSingletonScope();


            this.Bind<IViewFactory<StatisticsInput, StatisticsViewModel>>().To<StatisticsViewFactory>();
        }
    }
}
