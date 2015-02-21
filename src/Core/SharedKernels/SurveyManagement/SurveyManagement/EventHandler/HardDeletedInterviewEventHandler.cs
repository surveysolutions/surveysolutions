using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class HardDeletedInterviewEventHandler : AbstractFunctionalEventHandler<HardDeletedInterview, IReadSideKeyValueStorage<HardDeletedInterview>>, IUpdateHandler<HardDeletedInterview, InterviewHardDeleted>
    {
        public HardDeletedInterviewEventHandler(IReadSideKeyValueStorage<HardDeletedInterview> readSideStorage)
            : base(readSideStorage)
        {
        }

        public HardDeletedInterview Update(HardDeletedInterview currentState, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return new HardDeletedInterview() { InterviewId = evnt.EventSourceId };
        }
    }
}
