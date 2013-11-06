using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Utility;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;

namespace WB.Core.BoundedContexts.Designer
{
    public class DesignerBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IJsonExportService>().To<JsonExportService>().InSingletonScope();
            this.Bind<IQuestionFactory>().To<QuestionFactory>().InSingletonScope();

            ReqistyHelper.RegisterDenormalizer<QuestionnaireDenormalizer>(this.Kernel);
        }
    }
}
