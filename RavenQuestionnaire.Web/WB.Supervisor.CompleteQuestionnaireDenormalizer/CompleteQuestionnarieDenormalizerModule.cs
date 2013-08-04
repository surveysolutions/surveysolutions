using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject.Modules;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Supervisor.CompleteQuestionnaireDenormalizer
{
    public class CompleteQuestionnarieDenormalizerModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>, IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>()
             .To<RavenReadSideRepositoryWriterWithCacheAndZip>().InSingletonScope();
            this.Bind(typeof (IEventHandler<>)).To<InterviewSynchronizationEventHandler>();
        }
    }
}
