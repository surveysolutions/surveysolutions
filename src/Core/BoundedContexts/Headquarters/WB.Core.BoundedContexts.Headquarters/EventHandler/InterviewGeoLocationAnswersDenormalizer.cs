using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewGeoLocationAnswersDenormalizer : BaseDenormalizer,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<RosterInstancesRemoved>
    {
        private readonly IUnitOfWork sessionProvider;

        public InterviewGeoLocationAnswersDenormalizer(IUnitOfWork sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public override object[] Writers => new object[] { };

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> @event)
            => this.sessionProvider.Session.Save(new InterviewGps
            {
                InterviewId = @event.EventSourceId.ToString("N"),
                QuestionId = @event.Payload.QuestionId,
                RosterVector = RosterVector.Convert(@event.Payload.RosterVector).ToString().Trim('_'),
                Latitude = @event.Payload.Latitude,
                Longitude = @event.Payload.Longitude,
                Timestamp = @event.Payload.Timestamp.DateTime,
                IsEnabled = true
            });

        public void Handle(IPublishedEvent<AnswersRemoved> @event)
        {
            var questionIdentities = @event.Payload.Questions.Select(x => x.Id.ToString() + x.RosterVector.ToString().Trim('_'));
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Where(x => x.InterviewId == @event.EventSourceId.ToString("N") && questionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .Delete();
        }


        public void Handle(IPublishedEvent<QuestionsEnabled> @event)
        {
            var questionIdentities = @event.Payload.Questions.Select(x => x.Id.ToString() + x.RosterVector.ToString().Trim('_'));
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Where(x => x.InterviewId == @event.EventSourceId.ToString("N") && questionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .UpdateBuilder()
                .Set(x => x.IsEnabled, x => true)
                .Update();
        }
            

        public void Handle(IPublishedEvent<QuestionsDisabled> @event)
        {
            var questionIdentities = @event.Payload.Questions.Select(x => x.Id.ToString() + x.RosterVector.ToString().Trim('_'));
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Where(x => x.InterviewId == @event.EventSourceId.ToString("N") && questionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .UpdateBuilder()
                .Set(x => x.IsEnabled, x => false)
                .Update();
        }

        public void Handle(IPublishedEvent<RosterInstancesRemoved> @event)
        {
            var interviewId = @event.EventSourceId.ToString("N");

            var removedRosterInstances = @event.Payload.Instances
                .Select(x => $"{x.GroupId}{x.GetIdentity().RosterVector.ToString().Trim('_')}")
                .ToArray();

            var deletedQuestionIdentities = this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<InterviewSummary>(),
                    gps => gps.InterviewId,
                    interview => interview.SummaryId,
                    (gps, interview) => new {gps.InterviewId, gps.QuestionId, gps.RosterVector, interview.QuestionnaireIdentity})
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    gps => new {gps.QuestionnaireIdentity, gps.QuestionId},
                    questionnaireItem => new {questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId},
                    (gps, questionnaireItem) => new
                    {
                        gps.InterviewId, gps.QuestionId, gps.RosterVector,
                        ParentIdentity = questionnaireItem.ParentId.ToString() + gps.RosterVector
                    })
                .Where(x => x.InterviewId == interviewId  && removedRosterInstances.Contains(x.ParentIdentity))
                .Select(x => x.QuestionId.ToString() + x.RosterVector)
                .ToArray();

            this.sessionProvider.Session.Query<InterviewGps>()
                .Where(x => x.InterviewId == interviewId && deletedQuestionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .Delete();
        }

    }
}
