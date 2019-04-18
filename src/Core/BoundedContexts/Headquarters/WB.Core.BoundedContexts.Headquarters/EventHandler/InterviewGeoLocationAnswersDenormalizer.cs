using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewGeoLocationAnswersDenormalizer :
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, QuestionsEnabled>,
        IUpdateHandler<InterviewSummary, QuestionsDisabled>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,
        IUpdateHandler<InterviewSummary, RosterInstancesRemoved>
    {
        private readonly IUnitOfWork sessionProvider;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewGeoLocationAnswersDenormalizer(IUnitOfWork sessionProvider, IQuestionnaireStorage questionnaireStorage)
        {
            this.sessionProvider = sessionProvider;
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            var interviewId = @event.EventSourceId.ToString("N");
            var questionId = @event.Payload.QuestionId;
            var rosterVector = RosterVector.Convert(@event.Payload.RosterVector).ToString().Trim('_');

            var answer = this.sessionProvider.Session.Query<InterviewGps>().FirstOrDefault(x =>
                x.InterviewId == interviewId && x.QuestionId == @questionId && x.RosterVector == rosterVector);

            if (answer != null)
            {
                answer.Latitude = @event.Payload.Latitude;
                answer.Longitude = @event.Payload.Longitude;
                answer.Timestamp = @event.Payload.Timestamp;
                answer.IsEnabled = true;
                this.sessionProvider.Session.Update(answer);
            }
            else
            {
                this.sessionProvider.Session.Save(new InterviewGps
                {
                    InterviewId = interviewId,
                    QuestionId = @event.Payload.QuestionId,
                    RosterVector = rosterVector,
                    Latitude = @event.Payload.Latitude,
                    Longitude = @event.Payload.Longitude,
                    Timestamp = @event.Payload.Timestamp,
                    IsEnabled = true
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var gpsQuestionIdentities = GetGpsIdentities(state, @event.Payload.Questions);
            if (!gpsQuestionIdentities.Any()) return state;

            var questionIdentities = @event.Payload.Questions.Select(x => x.Id.ToString() + x.RosterVector.ToString().Trim('_'));
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Where(x => x.InterviewId == @event.EventSourceId.ToString("N") && questionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .Delete();

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsEnabled> @event)
        {
            var gpsQuestionIdentities = GetGpsIdentities(state, @event.Payload.Questions);
            if (!gpsQuestionIdentities.Any()) return state;

            var questionIdentities = @event.Payload.Questions.Select(x => x.Id.ToString() + x.RosterVector.ToString().Trim('_'));
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Where(x => x.InterviewId == @event.EventSourceId.ToString("N") && questionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .UpdateBuilder()
                .Set(x => x.IsEnabled, x => true)
                .Update();

            return state;
        }
            

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsDisabled> @event)
        {
            var gpsQuestionIdentities = GetGpsIdentities(state, @event.Payload.Questions);
            if (!gpsQuestionIdentities.Any()) return state;

            var questionIdentities = @event.Payload.Questions.Select(x => x.Id.ToString() + x.RosterVector.ToString().Trim('_'));
            this.sessionProvider.Session
                .Query<InterviewGps>()
                .Where(x => x.InterviewId == @event.EventSourceId.ToString("N") && questionIdentities.Contains(x.QuestionId.ToString() + x.RosterVector))
                .UpdateBuilder()
                .Set(x => x.IsEnabled, x => false)
                .Update();

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<RosterInstancesRemoved> @event)
        {
            var interviewId = @event.EventSourceId.ToString("N");

            var removedRosterInstances = @event.Payload.Instances
                .Select(x => $"{x.GroupId}{x.GetIdentity().RosterVector.ToString().Trim('_')}")
                .ToArray();

            var deletedQuestionIdentities = this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    gps => new {state.QuestionnaireIdentity, gps.QuestionId},
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

            return state;
        }

        private Identity[] GetGpsIdentities(InterviewSummary interview, Identity[] allQuestionIdentities)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(interview.QuestionnaireIdentity), null);

            return allQuestionIdentities
                .Where(x => questionnaire.GetQuestionType(x.Id) == QuestionType.GpsCoordinates)
                .ToArray();
        }

    }
}
