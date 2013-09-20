using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class DummyEventHandler :
        IEventHandler<AnswerCommented>,
        IEventHandler<FlagRemovedFromAnswer>,
        IEventHandler<FlagSetToAnswer>,
        IEventHandler<AnswerDeclaredValid>, IEventHandler<GroupPropagated>, IEventHandler<QuestionEnabled>,
        IEventHandler<AnswerDeclaredInvalid>, IEventHandler<QuestionDisabled>, IEventHandler<GroupDisabled>,
        IEventHandler<GroupEnabled>, IEventHandler<InterviewSynchronized>,
        IEventHandler<InterviewCompleted>, IEventHandler<InterviewDeclaredValid>, IEventHandler<SingleOptionLinkedQuestionAnswered>, IEventHandler
    {


        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt) {}

        public void Handle(IPublishedEvent<GroupPropagated> evnt) {}

        public void Handle(IPublishedEvent<QuestionEnabled> evnt) {}

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt) {}

        public void Handle(IPublishedEvent<QuestionDisabled> evnt) {}

        public void Handle(IPublishedEvent<GroupDisabled> evnt) {}

        public void Handle(IPublishedEvent<InterviewCompleted> evnt) {}

        public void Handle(IPublishedEvent<AnswerCommented> evnt) {}

        public void Handle(IPublishedEvent<FlagRemovedFromAnswer> evnt) {}

        public void Handle(IPublishedEvent<FlagSetToAnswer> evnt) {}

        public void Handle(IPublishedEvent<GroupEnabled> evnt) {}

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt) {}

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt) {}

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) { }

        public string Name {
            get { return "Dummy event handler"; }
        }
        public Type[] UsesViews {
            get { return  new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[0]; }
        }
    }
}
