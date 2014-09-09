using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class HardDeletedInterviewEventHandler : AbstractFunctionalEventHandler<HardDeletedInterview>, ICreateHandler<HardDeletedInterview, InterviewHardDeleted>
    {
        public HardDeletedInterviewEventHandler(IReadSideRepositoryWriter<HardDeletedInterview> readsideRepositoryWriter)
            : base(readsideRepositoryWriter)
        {
        }

        public HardDeletedInterview Create(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return new HardDeletedInterview() { InterviewId = evnt.EventSourceId };
        }
    }
}
