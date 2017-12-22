using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewDenormalizer : AbstractDenormalizer<InterviewEntity>,
        IEventHandler,
        IUpdateOrRemoveHandler<InterviewEntity, GroupPropagated>,
        IUpdateOrRemoveHandler<InterviewEntity, RosterInstancesAdded>,
        IUpdateOrRemoveHandler<InterviewEntity, RosterInstancesRemoved>,
        IUpdateOrRemoveHandler<InterviewEntity, MultipleOptionsQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, NumericRealQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, NumericIntegerQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, TextQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, TextListQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, SingleOptionQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, SingleOptionLinkedQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, DateTimeQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, GeoLocationQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, QRBarcodeQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, PictureQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, YesNoQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, AnswersRemoved>,
        IUpdateOrRemoveHandler<InterviewEntity, GroupsDisabled>,
        IUpdateOrRemoveHandler<InterviewEntity, GroupsEnabled>,
        IUpdateOrRemoveHandler<InterviewEntity, StaticTextsEnabled>,
        IUpdateOrRemoveHandler<InterviewEntity, StaticTextsDisabled>,
        IUpdateOrRemoveHandler<InterviewEntity, StaticTextsDeclaredInvalid>,
        IUpdateOrRemoveHandler<InterviewEntity, StaticTextsDeclaredValid>,
        IUpdateOrRemoveHandler<InterviewEntity, QuestionsDisabled>,
        IUpdateOrRemoveHandler<InterviewEntity, QuestionsEnabled>,
        IUpdateOrRemoveHandler<InterviewEntity, AnswersDeclaredInvalid>,
        IUpdateOrRemoveHandler<InterviewEntity, AnswersDeclaredValid>,
        IUpdateOrRemoveHandler<InterviewEntity, InterviewHardDeleted>,
        IUpdateOrRemoveHandler<InterviewEntity, AnswerRemoved>,
        IUpdateOrRemoveHandler<InterviewEntity, QuestionsMarkedAsReadonly>,
        IUpdateOrRemoveHandler<InterviewEntity, VariablesChanged>,
        IUpdateOrRemoveHandler<InterviewEntity, VariablesDisabled>,
        IUpdateOrRemoveHandler<InterviewEntity, VariablesEnabled>,
        IUpdateOrRemoveHandler<InterviewEntity, AreaQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, AudioQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewEntity, InterviewCreated>,
        IUpdateOrRemoveHandler<InterviewEntity, InterviewFromPreloadedDataCreated>,
        IUpdateOrRemoveHandler<InterviewEntity, InterviewOnClientCreated>
    {
        public string Name => "Interview detalis";
        public object[] Readers => new object[0];
        public object[] Writers => new object[0];

        private readonly IInterviewFactory repository;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewDenormalizer(IInterviewFactory interviewFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries, IQuestionnaireStorage questionnaireStorage)
        {
            this.repository = interviewFactory;
            this.summaries = summaries;
            this.questionnaireStorage = questionnaireStorage;
        }

        private readonly Dictionary<Guid, QuestionnaireIdentity> interviewToQuestionnaire =
            new Dictionary<Guid, QuestionnaireIdentity>();

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<GroupPropagated> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId, Identity.Create(evnt.Payload.GroupId, evnt.Payload.OuterScopeRosterVector), EntityType.Section);

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            foreach (var rosterInstance in evnt.Payload.Instances)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, rosterInstance.GetIdentity(), EntityType.Section);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<RosterInstancesRemoved> evnt)
        {
            if(!this.interviewToQuestionnaire.TryGetValue(evnt.EventSourceId, out var questionnaireIdentity))
            {
                questionnaireIdentity = this.summaries.GetQuestionnaireIdentity(evnt.EventSourceId);
                this.AddQuestionnaireToDictionary(evnt.EventSourceId, questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);                
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            if (questionnaire == null) return;
            
            foreach (var instance in evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray())
            {
                var roster = questionnaire.Find<IGroup>(instance.Id);

                this.RemoveEntityInState(state, evnt.EventSourceId, Identity.Create(roster.PublicKey, instance.RosterVector));
                
                foreach (var entity in roster.Children.TreeToEnumerable(x => x.Children))
                    this.RemoveEntityInState(state, evnt.EventSourceId, Identity.Create(entity.PublicKey, instance.RosterVector));
            }
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question,
                x =>
                {
                    x.AsIntArray = evnt.Payload.SelectedValues.Select(Convert.ToInt32).ToArray();
                    x.AnswerType = AnswerType.IntArray;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<NumericRealQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsDouble = Convert.ToDouble(evnt.Payload.Answer);
                    x.AnswerType = AnswerType.Double;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<NumericIntegerQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsInt = evnt.Payload.Answer;
                    x.AnswerType = AnswerType.Int;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<TextQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsString = evnt.Payload.Answer;
                    x.AnswerType = AnswerType.String;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<TextListQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsList = evnt.Payload.Answers.Select(_ => new InterviewTextListAnswer(_.Item1, _.Item2)).ToArray();
                    x.AnswerType = AnswerType.TextList;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<SingleOptionQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsInt = Convert.ToInt32(evnt.Payload.SelectedValue);
                    x.AnswerType = AnswerType.Int;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsIntArray = evnt.Payload.SelectedRosterVector.Select(Convert.ToInt32).ToArray();
                    x.AnswerType = AnswerType.IntArray;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsIntMatrix = evnt.Payload.SelectedRosterVectors.Select(_ => _.Select(Convert.ToInt32).ToArray()).ToArray();
                    x.AnswerType = AnswerType.IntMatrix;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<DateTimeQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsDateTime = evnt.Payload.Answer;
                    x.AnswerType = AnswerType.Datetime;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<GeoLocationQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsGps = new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy,
                        evnt.Payload.Altitude, evnt.Payload.Timestamp);
                    x.AnswerType = AnswerType.Gps;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<QRBarcodeQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsString = evnt.Payload.Answer;
                    x.AnswerType = AnswerType.String;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<PictureQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsString = evnt.Payload.PictureFileName;
                    x.AnswerType = AnswerType.String;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<YesNoQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsYesNo = evnt.Payload.AnsweredOptions;
                    x.AnswerType = AnswerType.YesNoList;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<AreaQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question,
                x =>
                {
                    x.AsArea = new Area(evnt.Payload.Geometry, evnt.Payload.MapName, evnt.Payload.AreaSize,
                        evnt.Payload.Length, evnt.Payload.Coordinates, evnt.Payload.DistanceToEditor);
                    x.AnswerType = AnswerType.Area;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<AudioQuestionAnswered> evnt) =>
            this.AddOrUpdateEntityInState(state, evnt.EventSourceId,
                Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question, 
                x =>
                {
                    x.AsAudio = AudioAnswer.FromString(evnt.Payload.FileName, evnt.Payload.Length);
                    x.AnswerType = AnswerType.Audio;
                });

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.RemoveAnswerInState(state, evnt.EventSourceId, question, EntityType.Question);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<GroupsDisabled> evnt)
        {
            foreach (var section in evnt.Payload.Groups)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, section, EntityType.Section, x => x.IsEnabled = false);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<GroupsEnabled> evnt)
        {
            foreach (var section in evnt.Payload.Groups)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, section, EntityType.Section, x => x.IsEnabled = true);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<StaticTextsEnabled> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, staticText, EntityType.StaticText, x => x.IsEnabled = true);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<StaticTextsDisabled> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, staticText, EntityType.StaticText, x => x.IsEnabled = false);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<StaticTextsDeclaredInvalid> evnt)
        {
            foreach (var staticText in evnt.Payload.FailedValidationConditions)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, staticText.Key,
                    EntityType.StaticText, x => x.InvalidValidations = staticText.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<StaticTextsDeclaredValid> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, staticText, EntityType.StaticText, x => x.InvalidValidations = null);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<QuestionsDisabled> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, question, EntityType.Question, x => x.IsEnabled = false);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<QuestionsEnabled> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, question, EntityType.Question, x => x.IsEnabled = true);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            foreach (var question in evnt.Payload.FailedValidationConditions)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, question.Key,
                    EntityType.Question, x => x.InvalidValidations = question.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, question, EntityType.Question, x => x.InvalidValidations = null);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<InterviewHardDeleted> evnt) =>
            this.repository.RemoveInterview(evnt.EventSourceId);

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<AnswerRemoved> evnt) => 
            this.RemoveAnswerInState(state, evnt.EventSourceId, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), EntityType.Question);

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<QuestionsMarkedAsReadonly> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, question, EntityType.Question, x => x.IsReadonly = true);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<VariablesChanged> evnt)
        {
            foreach (var variable in evnt.Payload.ChangedVariables)
            {
                if (variable.NewValue == null)
                    this.RemoveAnswerInState(state, evnt.EventSourceId, variable.Identity, EntityType.Variable);
                else
                {
                    this.AddOrUpdateEntityInState(state, evnt.EventSourceId, variable.Identity,
                        EntityType.Variable, x =>
                        {
                            x.AsDateTime = variable.NewValue as DateTime?;
                            x.AsString = variable.NewValue as string;
                            x.AsBool = variable.NewValue as bool?;
                            x.AsDouble = variable.NewValue as double?;
                            x.AsLong = variable.NewValue as long?;

                            x.AnswerType = InterviewEntity.GetAnswerType(variable.NewValue);
                        });
                }
            }
        }
            

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<VariablesDisabled> evnt)
        {
            foreach (var variable in evnt.Payload.Variables)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, variable, EntityType.Variable, x => x.IsEnabled = false);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<VariablesEnabled> evnt)
        {
            foreach (var variable in evnt.Payload.Variables)
                this.AddOrUpdateEntityInState(state, evnt.EventSourceId, variable, EntityType.Variable, x => x.IsEnabled = true);
        }

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<InterviewCreated> evnt) => 
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<InterviewFromPreloadedDataCreated> evnt) => 
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        public void Update(EntitiesState<InterviewEntity> state, IPublishedEvent<InterviewOnClientCreated> evnt) =>
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        private void AddOrUpdateEntityInState(EntitiesState<InterviewEntity> state, Guid interviewId, Identity entityId,
            EntityType entityType, Action<InterviewEntity> update = null)
        {
            var entity = state.AddedOrUpdated.Find(x => x.InterviewId == interviewId && x.Identity == entityId);
            if (entity == null)
            {
                entity = new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = entityId,
                    EntityType = entityType,
                    IsEnabled = true
                };
                state.AddedOrUpdated.Add(entity);
            }

            if (state.Removed.Contains(entity))
                state.Removed.Remove(entity);

            update?.Invoke(entity);
        }

        private void RemoveEntityInState(EntitiesState<InterviewEntity> state, Guid interviewId, Identity entityId)
        {
            var entity = state.Removed.Find(x => x.InterviewId == interviewId && x.Identity == entityId);
            if (entity == null)
            {
                entity = new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = entityId
                };
                state.Removed.Add(entity);
            }

            if (state.AddedOrUpdated.Contains(entity))
                state.AddedOrUpdated.Remove(entity);
        }

        private void RemoveAnswerInState(EntitiesState<InterviewEntity> state, Guid interviewId, Identity entityId, EntityType entityType) =>
            this.AddOrUpdateEntityInState(state, interviewId, entityId, entityType, x =>
            {
                x.AsDouble = null;
                x.AsArea = null;
                x.AsAudio = null;
                x.AsBool = null;
                x.AsDateTime = null;
                x.AsGps = null;
                x.AsInt = null;
                x.AsIntArray = null;
                x.AsIntMatrix = null;
                x.AsList = null;
                x.AsLong = null;
                x.AsYesNo = null;
                x.AsString = null;
            });

        private void AddQuestionnaireToDictionary(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
            => this.interviewToQuestionnaire[interviewId] = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

        protected override void SaveState(EntitiesState<InterviewEntity> state) =>
            this.repository.Save(state.AddedOrUpdated.ToArray(), state.Removed.ToArray());
    }
}
