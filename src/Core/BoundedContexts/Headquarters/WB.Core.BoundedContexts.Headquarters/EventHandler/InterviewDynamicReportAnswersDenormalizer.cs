﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewDynamicReportAnswersDenormalizer :
        FunctionalEventHandlerBase<InterviewSummary>,
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        
        IUpdateHandler<InterviewSummary, QuestionsEnabled>,
        IUpdateHandler<InterviewSummary, QuestionsDisabled>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,

        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,

        IUpdateHandler<InterviewSummary, VariablesChanged>,
        IUpdateHandler<InterviewSummary, VariablesEnabled>,
        IUpdateHandler<InterviewSummary, VariablesDisabled>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IMemoryCache localCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;

        public InterviewDynamicReportAnswersDenormalizer(IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireItems = questionnaireItems;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AnswersRemoved> @event)
        {
            var reportQuestionIdentities = GetExposedEntitiesIdentities(state, @event.Payload.Questions);
            if (reportQuestionIdentities.Count == 0) return state;

            var valuesToRemove = state.IdentifyEntitiesValues
                .Where(g => reportQuestionIdentities.Contains(g.Entity.EntityId) && g.Identifying != true).ToList();

            foreach (var answer in valuesToRemove)
            {
                state.IdentifyEntitiesValues.Remove(answer);
            }
            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsEnabled> @event)
        {
            var reportQuestionIdentities = GetExposedEntitiesIdentities(state, @event.Payload.Questions);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.IdentifyEntitiesValues.Where(g =>
                reportQuestionIdentities.Contains(g.Entity.EntityId) && g.Identifying != true))
            {
                answer.IsEnabled = true;
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QuestionsDisabled> @event)
        {
            var reportQuestionIdentities = GetExposedEntitiesIdentities(state, @event.Payload.Questions);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.IdentifyEntitiesValues.Where(g =>
                reportQuestionIdentities.Contains(g.Entity.EntityId) && g.Identifying != true))
            {
                answer.IsEnabled = false;
            }

            return state;
        }

        private HashSet<Guid> GetDynamicReportEntitiesIds(string questionnaireIdentity)
        {
            var cachedDynamicReportEntitiesIds = this.localCache.GetOrCreate($"idr:{questionnaireIdentity}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);

                return this.questionnaireItems.Query(_ => _
                        .Where(x => x.QuestionnaireIdentity == questionnaireIdentity && x.IncludedInReportingAtUtc != null)
                        .Select(x => x.EntityId))
                    .ToHashSet();
            });

            return cachedDynamicReportEntitiesIds;
        }


        private HashSet<Guid> GetExposedEntitiesIdentities(InterviewSummary interview, IEnumerable<Identity> allEntitiesIdentities)
        {
            return allEntitiesIdentities
                .Where(x => GetDynamicReportEntitiesIds(interview.QuestionnaireIdentity).Contains(x.Id))
                .Select(x => x.Id)
                .ToHashSet();
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntitiesIds(state.QuestionnaireIdentity);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.IdentifyEntitiesValues.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.Value = @event.Payload.Answer;
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);
                var id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId);

                state.IdentifyEntitiesValues.Add(new IdentifyEntityValue
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = id
                    },
                    Value = @event.Payload.Answer,
                    InterviewSummary = state,
                    IsEnabled = true,
                    Position = id
                });
            }

            return state;
        }
        
        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntitiesIds(state.QuestionnaireIdentity);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.IdentifyEntitiesValues.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.AnswerCode = @event.Payload.SelectedValue;
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);
                var id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId);

                state.IdentifyEntitiesValues.Add(new IdentifyEntityValue
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = id
                    },
                    AnswerCode = @event.Payload.SelectedValue,
                    InterviewSummary = state,
                    IsEnabled = true,
                    Position = id
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntitiesIds(state.QuestionnaireIdentity);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.IdentifyEntitiesValues.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.ValueDouble = Convert.ToDouble(@event.Payload.Answer);
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);
                var id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId);

                state.IdentifyEntitiesValues.Add(new IdentifyEntityValue()
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = id
                    },
                    ValueDouble = Convert.ToDouble(@event.Payload.Answer),
                    InterviewSummary = state,
                    IsEnabled = true,
                    Position = id
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntitiesIds(state.QuestionnaireIdentity);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.IdentifyEntitiesValues.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.ValueLong = @event.Payload.Answer;
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);
                var id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId);

                state.IdentifyEntitiesValues.Add(new IdentifyEntityValue()
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = id
                    },
                    ValueLong = @event.Payload.Answer,
                    InterviewSummary = state,
                    IsEnabled = true,
                    Position = id
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntitiesIds(state.QuestionnaireIdentity);
            if (!reportQuestionIdentities.Contains(@event.Payload.QuestionId)) return state;

            var answer = state.IdentifyEntitiesValues.FirstOrDefault(x => x.Entity.EntityId == @event.Payload.QuestionId);

            if (answer != null)
            {
                answer.ValueDate = @event.Payload.Answer;
                answer.IsEnabled = true;
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);
                var id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId);

                state.IdentifyEntitiesValues.Add(new IdentifyEntityValue()
                {
                    Entity = new QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(@event.Payload.QuestionId)
                    },
                    ValueDate = @event.Payload.Answer,
                    InterviewSummary = state,
                    IsEnabled = true,
                    Position = id
                });
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<VariablesChanged> @event)
        {
            var reportQuestionIdentities = GetDynamicReportEntitiesIds(state.QuestionnaireIdentity);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var variable in @event.Payload.ChangedVariables.Where(x=> reportQuestionIdentities.Contains(x.Identity.Id)))
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(QuestionnaireIdentity.Parse(state.QuestionnaireIdentity), null);
                var id = questionnaire.GetEntityIdMapValue(variable.Identity.Id);
                
                var identifyingValue = state.IdentifyEntitiesValues.FirstOrDefault(x => x.Entity.Id == id);
                if (identifyingValue == null)
                {
                    identifyingValue = new IdentifyEntityValue()
                    {
                        Entity = new QuestionnaireCompositeItem
                        {
                            Id = id
                        },
                        InterviewSummary = state,
                        IsEnabled = true,
                        Position = id
                    };
                    state.IdentifyEntitiesValues.Add(identifyingValue);
                }
                
                identifyingValue.IsEnabled = true;
                identifyingValue.Value = variable.NewValue.ToString();
                
                var varType = questionnaire.GetVariableVariableType(variable.Identity.Id);
                switch (varType)
                {
                    case VariableType.Boolean:
                        identifyingValue.ValueBool = Convert.ToBoolean(variable.NewValue);
                        break;
                    case VariableType.DateTime:
                        identifyingValue.ValueDate = variable.NewValue as DateTime?;
                        break;
                    case VariableType.Double:
                        identifyingValue.ValueDouble = Convert.ToDouble(variable.NewValue);
                        break;
                    case VariableType.LongInteger:
                        identifyingValue.ValueLong = Convert.ToInt64(variable.NewValue);
                        break;
                    case VariableType.String:
                        identifyingValue.Value = variable.NewValue as string;
                        break;
                }
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<VariablesEnabled> @event)
        {
            var reportQuestionIdentities = GetExposedEntitiesIdentities(state, @event.Payload.Variables);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.IdentifyEntitiesValues.Where(g =>
                reportQuestionIdentities.Contains(g.Entity.EntityId) && g.Identifying != true))
            {
                answer.IsEnabled = true;
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<VariablesDisabled> @event)
        {
            var reportQuestionIdentities = GetExposedEntitiesIdentities(state, @event.Payload.Variables);
            if (reportQuestionIdentities.Count == 0) return state;

            foreach (var answer in state.IdentifyEntitiesValues.Where(g =>
                reportQuestionIdentities.Contains(g.Entity.EntityId) && g.Identifying != true))
            {
                answer.IsEnabled = false;
            }

            return state;
        }
    }
}
