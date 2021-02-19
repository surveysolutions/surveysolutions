using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewDynamicReportAnswersDenormalizer :
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        
        IUpdateHandler<InterviewSummary, QuestionsEnabled>,
        IUpdateHandler<InterviewSummary, QuestionsDisabled>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,

        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,

        IUpdateHandler<InterviewSummary, VariablesChanged>

    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        //TODO: add cache
        private IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;

        public InterviewDynamicReportAnswersDenormalizer(IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireItems = questionnaireItems;
        }


        static readonly ConcurrentDictionary<Type, MethodInfo> MethodsCache = new ConcurrentDictionary<Type, MethodInfo>();

        public void Handle(InterviewSummary state, IEnumerable<IPublishableEvent> evts)
        {

            var newState = state;

            foreach (var evt in evts)
            {
                var payloadType = evt.Payload.GetType();

                var updateMethod = MethodsCache.GetOrAdd(payloadType, t =>
                {
                    var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

                    return this
                        .GetType()
                        .GetMethod("Update", new[] { typeof(InterviewSummary), eventType });
                });

                if (updateMethod == null)
                    continue;

                newState = (InterviewSummary)updateMethod
                    .Invoke(this, new object[] { newState, this.CreatePublishedEvent(evt) });

            }
        }

        private PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var reportQuestionIdentities = GetQuestionsForReportsIdentities(state, @event.Payload.Questions);
            if (reportQuestionIdentities.Count == 0) return state;
            
            foreach (var answer in state.ReportAnswers.Where(g =>
                reportQuestionIdentities.Contains((g.Entity.EntityId))))
            {
                state.ReportAnswers.Remove(answer);
            }
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsEnabled> @event)
        {
            var reportQuestionIdentities = GetQuestionsForReportsIdentities(state, @event.Payload.Questions);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.ReportAnswers.Where(g =>
                reportQuestionIdentities.Contains((g.Entity.EntityId))))
            {
                answer.IsEnabled = true;
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsDisabled> @event)
        {
            var reportQuestionIdentities = GetQuestionsForReportsIdentities(state, @event.Payload.Questions);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.ReportAnswers.Where(g =>
                reportQuestionIdentities.Contains((g.Entity.EntityId))))
            {
                answer.IsEnabled = false;
            }

            return state;
        }

        private HashSet<Guid> GetDynamicReportEntityIds(InterviewSummary interview)
        {
            return this.questionnaireItems.Query(_ => _
                .Where(x => x.QuestionnaireIdentity == interview.QuestionnaireIdentity && x.UsedInReporting == true)
                .Select(x => x.EntityId)
                .ToHashSet());
        }

        private HashSet<Guid> GetQuestionsForReportsIdentities(InterviewSummary interview, IEnumerable<Identity> allQuestionIdentities)
        {
            return allQuestionIdentities
                .Where(x => GetDynamicReportEntityIds(interview).Contains(x.Id))
                .Select(x => x.Id)
                .ToHashSet();
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntityIds(state);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.ReportAnswers.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.Value = @event.Payload.Answer;
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);

                state.ReportAnswers.Add(new InterviewReportAnswer
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId)
                    },
                    Value = @event.Payload.Answer,
                    InterviewSummary = state,
                    IsEnabled = true
                });
            }

            return state;
        }
        
        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntityIds(state);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.ReportAnswers.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.AnswerCode = @event.Payload.SelectedValue;
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);

                state.ReportAnswers.Add(new InterviewReportAnswer
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId)
                    },
                    AnswerCode = @event.Payload.SelectedValue,
                    InterviewSummary = state,
                    IsEnabled = true
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntityIds(state);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.ReportAnswers.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.Value = @event.Payload.Answer.ToString(CultureInfo.InvariantCulture);
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);

                state.ReportAnswers.Add(new InterviewReportAnswer
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId)
                    },
                    Value = @event.Payload.Answer.ToString(CultureInfo.InvariantCulture),
                    InterviewSummary = state,
                    IsEnabled = true
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntityIds(state);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.ReportAnswers.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.Value = @event.Payload.Answer.ToString(DateTimeFormat.DateFormat);
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);

                state.ReportAnswers.Add(new InterviewReportAnswer
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId)
                    },
                    Value = @event.Payload.Answer.ToString(),
                    InterviewSummary = state,
                    IsEnabled = true
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntityIds(state);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.ReportAnswers.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            //TODO: support datetime operations

            if (answer != null)
            {
                answer.Value = @event.Payload.Answer.ToString(DateTimeFormat.DateFormat);
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);

                state.ReportAnswers.Add(new InterviewReportAnswer
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId)
                    },
                    Value = @event.Payload.Answer.ToString(DateTimeFormat.DateFormat),
                    InterviewSummary = state,
                    IsEnabled = true
                });
            }

            return state;
        }


        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<VariablesChanged> @event)
        {
            var reportQuestionIdentities = GetQuestionsForReportsIdentities(state, @event.Payload.ChangedVariables.Select(x=>x.Identity));
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.ReportAnswers.Where(g =>
                reportQuestionIdentities.Contains((g.Entity.EntityId))))
            {
                answer.IsEnabled = true;
                answer.Value = @event.Payload.ChangedVariables.Select(x => x.Identity.Id == answer.Entity.EntityId)
                    .ToString();
            }

            return state;
        }
    }
}
