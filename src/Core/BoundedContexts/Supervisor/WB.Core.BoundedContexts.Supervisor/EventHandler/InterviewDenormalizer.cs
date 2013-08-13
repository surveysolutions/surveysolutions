using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewDenormalizer : IEventHandler,
                                         IEventHandler<InterviewCreated>,
                                         IEventHandler<InterviewStatusChanged>,
                                         IEventHandler<SupervisorAssigned>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<InterviewData> interviews;

        public InterviewDenormalizer(IReadSideRepositoryWriter<UserDocument> users, IReadSideRepositoryWriter<InterviewData> interviews)
        {
            this.users = users;
            this.interviews = interviews;
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            var responsible = this.users.GetById(evnt.Payload.UserId);
            var interview = new InterviewData()
            {
                InterviewId = evnt.EventSourceId,
                UpdateDate = evnt.EventTimeStamp,
                QuestionnaireId = evnt.Payload.QuestionnaireId,
                QuestionnaireVersion = evnt.Payload.QuestionnaireVersion,
                ResponsibleId = evnt.Payload.UserId, // Creator is responsible
                ResponsibleRole = responsible.Roles.FirstOrDefault()
            };
            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.Status = evnt.Payload.Status;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.ResponsibleId = evnt.Payload.SupervisorId;
            interview.ResponsibleRole = UserRoles.Supervisor;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new [] { typeof(UserDocument) }; }
        }
        public Type[] BuildsViews
        {
            get { return new [] { typeof(InterviewData) }; }
        }
    }
}
