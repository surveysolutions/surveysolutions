using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Eventing.ServiceModel.Bus;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public interface IInterviewStatisticsReportDenormalizer : ICompositeFunctionalPartEventHandler<InterviewSummary,
        IReadSideRepositoryWriter<InterviewSummary>>  { }

    internal class InterviewStatisticsReportDenormalizer :
        IInterviewStatisticsReportDenormalizer,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,
        IUpdateHandler<InterviewSummary, QuestionsDisabled>,
        IUpdateHandler<InterviewSummary, QuestionsEnabled>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewSummary, RosterInstancesRemoved>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewStatisticsReportDenormalizer(IUnitOfWork unitOfWork,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.unitOfWork = unitOfWork;
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            var questionId = @event.Payload.QuestionId;
            var rosterVector = new RosterVector(@event.Payload.RosterVector);

            UpdateReportStatisticsAnswer(state, questionId, rosterVector, StatisticsReportType.Categorical,
                @event.Payload.SelectedValue);

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var questionnaire =
                questionnaireStorage.GetQuestionnaireDocument(state.QuestionnaireId, state.QuestionnaireVersion);

            List<Identity> questions = @event.Payload.Questions
                .Where(q => IsEligibleQuestion(questionnaire.Find<IQuestion>(q.Id)))
                .ToList();

            foreach (var identity in questions)
            {
                unitOfWork.Session.Query<InterviewStatisticsReportRow>()
                    .Where(s => s.InterviewId == state.Id
                                && s.RosterVector == identity.RosterVector.AsString()
                                && s.EntityId == questionnaire.EntitiesIdMap[identity.Id])
                    .Delete();
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<RosterInstancesRemoved> @event)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(state.QuestionnaireId, state.QuestionnaireVersion);

            foreach (var instance in @event.Payload.Instances)
            {
                var rosterVector = new RosterVector(instance.OuterRosterVector) .ExtendWithOneCoordinate((int) instance.RosterInstanceId).AsString();
                var roster = questionnaire.Find<Group>(instance.GroupId);
                var questions = roster.Children.OfType<IQuestion>().Where(IsEligibleQuestion).ToArray();

                foreach (var question in questions)
                {
                    unitOfWork.Session.Query<InterviewStatisticsReportRow>()
                        .Where(s => s.InterviewId == state.Id
                                    && s.RosterVector == rosterVector
                                    && s.EntityId == questionnaire.EntitiesIdMap[question.PublicKey])
                        .Delete();
                }
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            UpdateReportStatisticsAnswer(state, @event.Payload.QuestionId,
                new RosterVector(@event.Payload.RosterVector),
                StatisticsReportType.Categorical, @event.Payload.SelectedValues);

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsDisabled> @event)
        {
            UpdateQuestionEnablement(state, false, @event.Payload.Questions);
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsEnabled> @event)
        {
            UpdateQuestionEnablement(state, true, @event.Payload.Questions);
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            UpdateReportStatisticsAnswer(state, @event.Payload.QuestionId,
                new RosterVector(@event.Payload.RosterVector),
                StatisticsReportType.Numeric, @event.Payload.Answer);

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            UpdateReportStatisticsAnswer(state, @event.Payload.QuestionId,
                new RosterVector(@event.Payload.RosterVector),
                StatisticsReportType.Numeric, @event.Payload.Answer);

            return state;
        }

        private bool IsEligibleQuestion(IQuestion question)
        {
            if (question.CascadeFromQuestionId != null) return false;
            if (question.LinkedToQuestionId != null || question.LinkedToRosterId != null) return false;
            if (question.IsFilteredCombobox == true) return false;
            if (question is SingleQuestion || question is NumericQuestion) return true;
            return false;
        }

        private void UpdateReportStatisticsAnswer(InterviewSummary state,
            Guid questionId, RosterVector rv, StatisticsReportType type = StatisticsReportType.Categorical,
            params decimal[] answer)
        {
            var questionnaire =
                questionnaireStorage.GetQuestionnaireDocument(state.QuestionnaireId, state.QuestionnaireVersion);

            var question = questionnaire.Find<IQuestion>(questionId);

            if (!IsEligibleQuestion(question)) return;
            
            (int interviewId, string rosterVector, int entityId) key =
                (state.Id, rv.AsString(), questionnaire.EntitiesIdMap[questionId]);

            var entity = this.unitOfWork.Session.Query<InterviewStatisticsReportRow>()
                .SingleOrDefault(x => x.InterviewId == key.interviewId
                                      && x.RosterVector == key.rosterVector
                                      && x.EntityId == key.entityId);

            if (entity == null)
            {
                entity = new InterviewStatisticsReportRow
                {
                    InterviewId = key.interviewId,
                    RosterVector = key.rosterVector,
                    EntityId = key.entityId,
                    Type = type,
                    IsEnabled = true,
                    Answer = answer.Select(a => (long)a).ToArray()
                };

                this.unitOfWork.Session.Save(entity);
            }
            else
            {
                entity.Answer = answer.Select(a => (long)a).ToArray();
                this.unitOfWork.Session.Update(entity);
            }
        }

        private void UpdateQuestionEnablement(InterviewSummary summary, bool enabled, Identity[] questionIds)
        {
            var questionnaire =
                questionnaireStorage.GetQuestionnaireDocument(summary.QuestionnaireId, summary.QuestionnaireVersion);

            List<Identity> questions = questionIds
                .Where(q => IsEligibleQuestion(questionnaire.Find<IQuestion>(q.Id)))
                .ToList();
            
            foreach (var identity in questions)
            {
                this.unitOfWork.Session
                    .Query<InterviewStatisticsReportRow>()
                    .Where(x => x.InterviewId == summary.Id
                                && x.RosterVector == identity.RosterVector.AsString()
                                && x.EntityId == questionnaire.EntitiesIdMap[identity.Id])
                    .UpdateBuilder()
                    .Set(x => x.IsEnabled, enabled)
                    .Update();
            }
        }

    }
}
