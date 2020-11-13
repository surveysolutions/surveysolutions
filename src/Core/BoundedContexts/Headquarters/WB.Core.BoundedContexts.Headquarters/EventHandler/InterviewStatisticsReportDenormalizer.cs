using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public interface IInterviewStatisticsReportDenormalizer : ICompositeFunctionalPartEventHandler<InterviewSummary,
        IReadSideRepositoryWriter<InterviewSummary>>
    { }

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
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewStatisticsReportDenormalizer(IQuestionnaireStorage questionnaireStorage)
        {
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
            var questionnaire = GetQuestionnaire(state.QuestionnaireId, state.QuestionnaireVersion);

            List<Identity> questions = @event.Payload.Questions
                .Where(q => IsEligibleQuestion(questionnaire, q.Id))
                .ToList();

            List<InterviewStatisticsReportRow> delete = new List<InterviewStatisticsReportRow>();

            foreach (var identity in questions)
            {
                if (state.StatisticsReportCache.TryGetValue(
                    (questionnaire.GetEntityIdMapValue(identity.Id), identity.RosterVector.AsString()), out var item))
                {
                    delete.Add(item);
                }
            }

            foreach (var item in delete)
            {
                state.StatisticsReport.Remove(item);
            }

            state.RefreshStatisticsReportCache();
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<RosterInstancesRemoved> @event)
        {
            var questionnaire = GetQuestionnaire(state.QuestionnaireId, state.QuestionnaireVersion);

            IEnumerable<(string rv, int entityId)> ToDelete()
            {
                foreach (var instance in @event.Payload.Instances)
                {
                    var rosterVector = new RosterVector(instance.OuterRosterVector).ExtendWithOneCoordinate((int)instance.RosterInstanceId).AsString();
                    var questionsIds = questionnaire.GetChildQuestions(instance.GroupId);
                    var eligibleQuestionIds = questionsIds.Where(id => IsEligibleQuestion(questionnaire, id)).ToArray();

                    foreach (var questionId in eligibleQuestionIds)
                    {
                        yield return (rosterVector, questionnaire.GetEntityIdMapValue(questionId));
                    }
                }
            }

            var toDelete = ToDelete().ToList();
            var entitiesToDeleteLookup = toDelete.ToLookup(d => d.entityId, d => d.rv);

            foreach (var entity in state.StatisticsReport.ToList())
            {
                var rosterVectors = entitiesToDeleteLookup[entity.EntityId];

                foreach (var rosterVector in rosterVectors)
                {
                    if (entity.RosterVector == rosterVector)
                    {
                        state.StatisticsReport.Remove(entity);
                    }
                }
            }

            state.RefreshStatisticsReportCache();
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

        private bool IsEligibleQuestion(IQuestionnaire questionnaire, Guid questionId)
        {
            if (questionnaire.IsQuestionCascading(questionId)) return false;
            if (questionnaire.IsQuestionLinked(questionId) || questionnaire.IsQuestionLinkedToRoster(questionId)) return false;
            if (questionnaire.IsQuestionFilteredCombobox(questionId)) return false;

            var questionType = questionnaire.GetQuestionType(questionId);
            if (questionType == QuestionType.SingleOption || questionType == QuestionType.Numeric || questionType == QuestionType.MultyOption) return true;
            return false;
        }

        private void UpdateReportStatisticsAnswer(InterviewSummary state,
            Guid questionId, RosterVector rv, StatisticsReportType type = StatisticsReportType.Categorical,
            params decimal[] answer)
        {
            var questionnaire = GetQuestionnaire(state.QuestionnaireId, state.QuestionnaireVersion);

            if (!IsEligibleQuestion(questionnaire, questionId)) return;

            (int entityId, string rosterVector) key = (questionnaire.GetEntityIdMapValue(questionId), rv.AsString());
            
            if (!state.StatisticsReportCache.TryGetValue(key, out var entity))
            {
                entity = new InterviewStatisticsReportRow
                {
                    InterviewSummary = state,
                    RosterVector = key.rosterVector,
                    EntityId = key.entityId,
                    Type = type,
                    IsEnabled = true,
                    Answer = answer.Select(a => (long)a).ToArray()
                };

                state.StatisticsReport.Add(entity);
                if (state.StatisticsReportCache == null) 
                    state.RefreshStatisticsReportCache();
                else
                    state.StatisticsReportCache[key] = entity;
            }
            else
            {
                entity.Answer = answer.Select(a => (long)a).ToArray();
            }
        }

        private void UpdateQuestionEnablement(InterviewSummary summary, bool enabled, Identity[] questionIds)
        {
            var questionnaire = GetQuestionnaire(summary.QuestionnaireId, summary.QuestionnaireVersion);

            List<Identity> questions = questionIds
                .Where(q => IsEligibleQuestion(questionnaire, q.Id))
                .ToList();

            if (summary.StatisticsReportCache == null)
            {
                summary.StatisticsReportCache = summary.StatisticsReport.ToDictionary(s => (s.EntityId, s.RosterVector));
            }

            foreach (var identity in questions)
            {
                if (summary.StatisticsReportCache.TryGetValue(
                    (questionnaire.GetEntityIdMapValue(identity.Id), identity.RosterVector.ToString()),
                    out var entity))
                {
                    entity.IsEnabled = enabled;
                }
            }
        }

        private IQuestionnaire GetQuestionnaire(Guid questionnaireId, long version)
        {
            return questionnaireStorage.GetQuestionnaire(new QuestionnaireIdentity(questionnaireId, version), null);
        }
    }
}
