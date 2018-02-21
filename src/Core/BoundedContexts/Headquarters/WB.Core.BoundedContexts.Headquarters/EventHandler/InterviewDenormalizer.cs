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
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewDenormalizer : AbstractDenormalizer<InterviewState>,
        IEventHandler,
        IUpdateOrRemoveHandler<InterviewState, GroupPropagated>,
        IUpdateOrRemoveHandler<InterviewState, RosterInstancesAdded>,
        IUpdateOrRemoveHandler<InterviewState, RosterInstancesRemoved>,
        IUpdateOrRemoveHandler<InterviewState, MultipleOptionsQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, NumericRealQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, NumericIntegerQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, TextQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, TextListQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, SingleOptionQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, SingleOptionLinkedQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, DateTimeQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, GeoLocationQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, QRBarcodeQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, PictureQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, YesNoQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, AnswersRemoved>,
        IUpdateOrRemoveHandler<InterviewState, GroupsDisabled>,
        IUpdateOrRemoveHandler<InterviewState, GroupsEnabled>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsEnabled>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDisabled>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDeclaredInvalid>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDeclaredValid>,
        IUpdateOrRemoveHandler<InterviewState, QuestionsDisabled>,
        IUpdateOrRemoveHandler<InterviewState, QuestionsEnabled>,
        IUpdateOrRemoveHandler<InterviewState, AnswersDeclaredInvalid>,
        IUpdateOrRemoveHandler<InterviewState, AnswersDeclaredValid>,
        IUpdateOrRemoveHandler<InterviewState, InterviewHardDeleted>,
        IUpdateOrRemoveHandler<InterviewState, AnswerRemoved>,
        IUpdateOrRemoveHandler<InterviewState, QuestionsMarkedAsReadonly>,
        IUpdateOrRemoveHandler<InterviewState, VariablesChanged>,
        IUpdateOrRemoveHandler<InterviewState, VariablesDisabled>,
        IUpdateOrRemoveHandler<InterviewState, VariablesEnabled>,
        IUpdateOrRemoveHandler<InterviewState, AreaQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, AudioQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, InterviewCreated>,
        IUpdateOrRemoveHandler<InterviewState, InterviewFromPreloadedDataCreated>,
        IUpdateOrRemoveHandler<InterviewState, InterviewOnClientCreated>,
        IUpdateOrRemoveHandler<InterviewState, AnswersDeclaredPlausible>,
        IUpdateOrRemoveHandler<InterviewState, AnswersDeclaredImplausible>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDeclaredPlausible>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDeclaredImplausible>
    {
        public string Name => "Interview detalis";
        public object[] Readers => new object[0];
        public object[] Writers => new object[0];

        private readonly IInterviewFactory repository;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IEntitySerializer<object> entitySerializer;

        public InterviewDenormalizer(IInterviewFactory interviewFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries, IQuestionnaireStorage questionnaireStorage,
            IEntitySerializer<object> entitySerializer)
        {
            this.repository = interviewFactory;
            this.summaries = summaries;
            this.questionnaireStorage = questionnaireStorage;
            this.entitySerializer = entitySerializer;
        }

        private readonly Dictionary<Guid, QuestionnaireIdentity> interviewToQuestionnaire =
            new Dictionary<Guid, QuestionnaireIdentity>();

        public void Update(InterviewState state, IPublishedEvent<GroupPropagated> evnt) =>
            this.SetEnablementInState(state, InterviewStateIdentity.Create(evnt.Payload.GroupId, evnt.Payload.OuterScopeRosterVector), true);

        public void Update(InterviewState state, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            foreach (var rosterInstance in evnt.Payload.Instances)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(rosterInstance.GetIdentity()), true);
        }

        public void Update(InterviewState state, IPublishedEvent<RosterInstancesRemoved> evnt)
        {
            if(!this.interviewToQuestionnaire.TryGetValue(evnt.EventSourceId, out var questionnaireIdentity))
            {
                questionnaireIdentity = this.summaries.GetQuestionnaireIdentity(evnt.EventSourceId);
                this.AddQuestionnaireToDictionary(evnt.EventSourceId, questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);                
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            if (questionnaire == null) return;

            var removeEntities = new List<InterviewStateIdentity>();
            
            foreach (var instance in evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray())
            {
                removeEntities.Add(InterviewStateIdentity.Create(instance));

                var roster = questionnaire.Find<IGroup>(instance.Id);
                
                removeEntities.AddRange(roster.Children.TreeToEnumerable(x => x.Children)
                    .Select(entity => InterviewStateIdentity.Create(entity.PublicKey, instance.RosterVector)));
            }

            foreach (var removedEntity in removeEntities)
            {
                state.Answers.Remove(removedEntity);
                state.Enablement.Remove(removedEntity);
                state.Validity.Remove(removedEntity);
                state.ReadOnly.Remove(removedEntity);

                if (!state.Removed.Contains(removedEntity))
                    state.Removed.Add(removedEntity);
            }
        }

        public void Update(InterviewState state, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsIntArray = evnt.Payload.SelectedValues.Select(Convert.ToInt32).ToArray(),
                });

        public void Update(InterviewState state, IPublishedEvent<NumericRealQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsDouble = Convert.ToDouble(evnt.Payload.Answer)
                });

        public void Update(InterviewState state, IPublishedEvent<NumericIntegerQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsInt = evnt.Payload.Answer
                });

        public void Update(InterviewState state, IPublishedEvent<TextQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsString = evnt.Payload.Answer
                });

        public void Update(InterviewState state, IPublishedEvent<TextListQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsList = this.entitySerializer.Serialize(evnt.Payload.Answers.Select(_ => new InterviewTextListAnswer(_.Item1, _.Item2)).ToArray())
                });

        public void Update(InterviewState state, IPublishedEvent<SingleOptionQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), new InterviewStateAnswer
            {
                Id = evnt.Payload.QuestionId,
                RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                AsInt = Convert.ToInt32(evnt.Payload.SelectedValue)
            });

        public void Update(InterviewState state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsIntArray = evnt.Payload.SelectedRosterVector.Select(Convert.ToInt32).ToArray()
                });

        public void Update(InterviewState state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsIntMatrix = this.entitySerializer.Serialize(evnt.Payload.SelectedRosterVectors.Select(_ => _.Select(Convert.ToInt32).ToArray()).ToArray())
                });

        public void Update(InterviewState state, IPublishedEvent<DateTimeQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsDatetime = evnt.Payload.Answer
                });

        public void Update(InterviewState state, IPublishedEvent<GeoLocationQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsGps = this.entitySerializer.Serialize(new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy,
                        evnt.Payload.Altitude, evnt.Payload.Timestamp))
                });

        public void Update(InterviewState state, IPublishedEvent<QRBarcodeQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsString = evnt.Payload.Answer
                });

        public void Update(InterviewState state, IPublishedEvent<PictureQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsString = evnt.Payload.PictureFileName
                });

        public void Update(InterviewState state, IPublishedEvent<YesNoQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsYesNo = this.entitySerializer.Serialize(evnt.Payload.AnsweredOptions)
                });

        public void Update(InterviewState state, IPublishedEvent<AreaQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsArea = this.entitySerializer.Serialize(new Area(
                        evnt.Payload.Geometry, evnt.Payload.MapName, evnt.Payload.AreaSize,
                        evnt.Payload.Length, evnt.Payload.Coordinates, evnt.Payload.DistanceToEditor))
                });

        public void Update(InterviewState state, IPublishedEvent<AudioQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray(),
                    AsAudio = this.entitySerializer.Serialize(AudioAnswer.FromString(evnt.Payload.FileName, evnt.Payload.Length))
                });

        public void Update(InterviewState state, IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                var identity = InterviewStateIdentity.Create(question);
                if (!state.Removed.Contains(identity))
                {
                    this.SetAnswerInState(state, identity, new InterviewStateAnswer
                    {
                        Id = question.Id,
                        RosterVector = question.RosterVector
                    });
                }
            }
        }

        public void Update(InterviewState state, IPublishedEvent<GroupsDisabled> evnt)
        {
            foreach (var section in evnt.Payload.Groups)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(section), false);
        }

        public void Update(InterviewState state, IPublishedEvent<GroupsEnabled> evnt)
        {
            foreach (var section in evnt.Payload.Groups)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(section), true);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsEnabled> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(staticText), true);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDisabled> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(staticText), false);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDeclaredInvalid> evnt)
        {
            foreach (var staticText in evnt.Payload.FailedValidationConditions)
                this.SetValidityInState(state, InterviewStateIdentity.Create(staticText.Key), staticText.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDeclaredValid> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.SetValidityInState(state, InterviewStateIdentity.Create(staticText), null);
        }

        public void Update(InterviewState state, IPublishedEvent<QuestionsDisabled> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(question), false);
        }

        public void Update(InterviewState state, IPublishedEvent<QuestionsEnabled> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(question), true);
        }

        public void Update(InterviewState state, IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            foreach (var question in evnt.Payload.FailedValidationConditions)
                this.SetValidityInState(state, InterviewStateIdentity.Create(question.Key), question.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(InterviewState state, IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetValidityInState(state, InterviewStateIdentity.Create(question), null);
        }

        public void Update(InterviewState state, IPublishedEvent<InterviewHardDeleted> evnt) =>
            this.repository.RemoveInterview(evnt.EventSourceId);

        public void Update(InterviewState state, IPublishedEvent<AnswerRemoved> evnt) =>
            this.SetAnswerInState(state, InterviewStateIdentity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new InterviewStateAnswer
                {
                    Id = evnt.Payload.QuestionId,
                    RosterVector = evnt.Payload.RosterVector.ToIntArray()
                });

        public void Update(InterviewState state, IPublishedEvent<QuestionsMarkedAsReadonly> evnt)
        {
            foreach (var readOnlyQuestion in evnt.Payload.Questions)
                this.SetReadOnlyInState(state, InterviewStateIdentity.Create(readOnlyQuestion));
        }

        public void Update(InterviewState state, IPublishedEvent<VariablesChanged> evnt)
        {
            foreach (var variable in evnt.Payload.ChangedVariables)
                this.SetAnswerInState(state, InterviewStateIdentity.Create(variable.Identity), new InterviewStateAnswer
                {
                    Id = variable.Identity.Id,
                    RosterVector = variable.Identity.RosterVector,
                    AsString = variable.NewValue as string,
                    AsDouble = variable.NewValue as double?,
                    AsDatetime = variable.NewValue as DateTime?,
                    AsLong = variable.NewValue as long?,
                    AsBool = variable.NewValue as bool?
                });
        }

        public void Update(InterviewState state, IPublishedEvent<VariablesDisabled> evnt)
        {
            foreach (var variable in evnt.Payload.Variables)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(variable), false);
        }

        public void Update(InterviewState state, IPublishedEvent<VariablesEnabled> evnt)
        {
            foreach (var variable in evnt.Payload.Variables)
                this.SetEnablementInState(state, InterviewStateIdentity.Create(variable), true);
        }

        public void Update(InterviewState state, IPublishedEvent<InterviewCreated> evnt) => 
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        public void Update(InterviewState state, IPublishedEvent<InterviewFromPreloadedDataCreated> evnt) => 
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        public void Update(InterviewState state, IPublishedEvent<InterviewOnClientCreated> evnt) =>
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        private void SetEnablementInState(InterviewState state, InterviewStateIdentity entityId, bool isEnabled)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Enablement[entityId] = isEnabled;
        }

        private void SetValidityInState(InterviewState state, InterviewStateIdentity entityId, int[] invalidValidations)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Validity[entityId] = new InterviewStateValidation
            {
                Id = entityId.Id,
                RosterVector = entityId.RosterVector,
                Validations = invalidValidations,
            };
        }

        private void SetWarningInState(InterviewState state, InterviewStateIdentity entityId, int[] invalidValidations)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Warnings[entityId] = new InterviewStateValidation
            {
                Id = entityId.Id,
                RosterVector = entityId.RosterVector,
                Validations = invalidValidations,
            };
        }

        private void SetAnswerInState(InterviewState state, InterviewStateIdentity entityId, InterviewStateAnswer answer)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Answers[entityId] = answer;
        }

        private void SetReadOnlyInState(InterviewState state, InterviewStateIdentity entityId)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            if (!state.ReadOnly.Contains(entityId))
                state.ReadOnly.Add(entityId);
        }

        private void AddQuestionnaireToDictionary(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
            => this.interviewToQuestionnaire[interviewId] = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

        protected override void SaveState(InterviewState state) =>
            this.repository.Save(state);

        public void Update(InterviewState state, IPublishedEvent<AnswersDeclaredPlausible> @event)
        {
            foreach (var question in @event.Payload.Questions)
                this.SetWarningInState(state, InterviewStateIdentity.Create(question), null);
        }

        public void Update(InterviewState state, IPublishedEvent<AnswersDeclaredImplausible> @event)
        {
            foreach (var condition in @event.Payload.FailedValidationConditions)
                this.SetWarningInState(state, InterviewStateIdentity.Create(condition.Key), condition.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDeclaredPlausible> @event)
        {
            foreach (var staticText in @event.Payload.StaticTexts)
                this.SetWarningInState(state, InterviewStateIdentity.Create(staticText), null);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDeclaredImplausible> @event)
        {
            foreach (var condition in @event.Payload.FailedValidationConditions)
                this.SetWarningInState(state, InterviewStateIdentity.Create(condition.Key), condition.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }
    }
}
