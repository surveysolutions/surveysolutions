using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public virtual void Apply(InterviewReceivedByInterviewer @event)
        {
            this.properties.IsReceivedByInterviewer = true;
        }

        public virtual void Apply(InterviewReceivedBySupervisor @event)
        {
            this.properties.IsReceivedByInterviewer = false;
        }

        public virtual void Apply(InterviewCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        public virtual void Apply(InterviewOnClientCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        public virtual void Apply(InterviewFromPreloadedDataCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        public virtual void Apply(SynchronizationMetadataApplied @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
            this.properties.Status = @event.Status;
        }

        public virtual void Apply(TextQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsText.SetAnswer(TextAnswer.FromString(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateTextAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(QRBarcodeQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsQRBarcode.SetAnswer(QRBarcodeAnswer.FromString(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateQrBarcodeAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(PictureQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsMultimedia.SetAnswer(MultimediaAnswer.FromString(@event.PictureFileName));
            this.ExpressionProcessorStatePrototype.UpdateMediaAnswer(@event.QuestionId, @event.RosterVector, @event.PictureFileName);
        }

        public virtual void Apply(NumericRealQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsDouble.SetAnswer(NumericRealAnswer.FromDecimal(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateNumericRealAnswer(@event.QuestionId, @event.RosterVector, (double)@event.Answer);
        }

        public virtual void Apply(NumericIntegerQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsInteger.SetAnswer(NumericIntegerAnswer.FromInt(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateNumericIntegerAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(DateTimeQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsDateTime.SetAnswer(DateTimeAnswer.FromDateTime(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateDateAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(SingleOptionQuestionAnswered @event)
        {
            var question = this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector));

            question.AsSingleFixedOption?.SetAnswer(CategoricalFixedSingleOptionAnswer.FromDecimal(@event.SelectedValue));
            question.AsSingleLinkedToList?.SetAnswer(CategoricalFixedSingleOptionAnswer.FromDecimal(@event.SelectedValue));

            this.ExpressionProcessorStatePrototype.UpdateSingleOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedValue);
        }

        public virtual void Apply(MultipleOptionsQuestionAnswered @event)
        {
            var question =  this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector));
            question.AsMultiFixedOption?.SetAnswer(CategoricalFixedMultiOptionAnswer.FromDecimalArray(@event.SelectedValues));
            question.AsMultiLinkedToList?.SetAnswer(CategoricalFixedMultiOptionAnswer.FromDecimalArray(@event.SelectedValues));

            this.ExpressionProcessorStatePrototype.UpdateMultiOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedValues);
        }

        public virtual void Apply(YesNoQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsYesNo.SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions(@event.AnsweredOptions));
            this.ExpressionProcessorStatePrototype.UpdateYesNoAnswer(@event.QuestionId, @event.RosterVector, YesNoAnswer.FromAnsweredYesNoOptions(@event.AnsweredOptions).ToYesNoAnswersOnly());
        }

        public virtual void Apply(GeoLocationQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsGps.SetAnswer(GpsAnswer.FromGeoPosition(new GeoPosition(
                    @event.Latitude, @event.Longitude, @event.Accuracy, @event.Altitude, @event.Timestamp)));

            this.ExpressionProcessorStatePrototype.UpdateGeoLocationAnswer(@event.QuestionId, @event.RosterVector, @event.Latitude,
                @event.Longitude, @event.Accuracy, @event.Altitude);
        }

        public virtual void Apply(TextListQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsTextList.SetAnswer(TextListAnswer.FromTupleArray(@event.Answers));
            this.ExpressionProcessorStatePrototype.UpdateTextListAnswer(@event.QuestionId, @event.RosterVector, @event.Answers);
        }

        public virtual void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsSingleLinkedOption.SetAnswer(CategoricalLinkedSingleOptionAnswer.FromRosterVector(@event.SelectedRosterVector));
            this.ExpressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedRosterVector);
        }

        public virtual void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).AsMultiLinkedOption.SetAnswer(CategoricalLinkedMultiOptionAnswer.FromDecimalArrayArray(@event.SelectedRosterVectors));
            this.ExpressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedRosterVectors);
        }

        public virtual void Apply(AnswersDeclaredValid @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity).MarkValid();

            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.Questions);
        }

        public virtual void Apply(AnswersDeclaredInvalid @event)
        {
            if (@event.FailedValidationConditions.Count > 0)
            {
                foreach (var failedValidationCondition in @event.FailedValidationConditions)
                {
                    if (failedValidationCondition.Value?.Count > 0)
                        this.Tree.GetQuestion(failedValidationCondition.Key).MarkInvalid(failedValidationCondition.Value);
                    else
                        this.Tree.GetQuestion(failedValidationCondition.Key).MarkInvalid();
                }

                this.ExpressionProcessorStatePrototype.ApplyFailedValidations(@event.FailedValidationConditions);
            }
            else //handling of old events
            {
                foreach (var invalidQuestionIdentity in @event.Questions)
                    this.Tree.GetQuestion(invalidQuestionIdentity).MarkInvalid();

                this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.Questions);
            }
        }

        public virtual void Apply(StaticTextsDeclaredValid @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity).MarkValid();
            this.ExpressionProcessorStatePrototype.DeclareStaticTextValid(@event.StaticTexts);
        }

        public virtual void Apply(StaticTextsDeclaredInvalid @event)
        {
            var staticTextsConditions = @event.GetFailedValidationConditionsDictionary();

            foreach (var staticTextIdentity in staticTextsConditions.Keys)
                this.Tree.GetStaticText(staticTextIdentity).MarkInvalid(staticTextsConditions[staticTextIdentity]);

            this.ExpressionProcessorStatePrototype.ApplyStaticTextFailedValidations(staticTextsConditions);
        }

        public void Apply(LinkedOptionsChanged @event)
        {
            foreach (var linkedQuestion in @event.ChangedLinkedQuestions)
                this.Tree.GetQuestion(linkedQuestion.QuestionId).AsLinked.SetOptions(linkedQuestion.Options);
        }

        public void Apply(LinkedToListOptionsChanged @event)
        {
            foreach (var linkedQuestion in @event.ChangedLinkedQuestions)
                this.Tree.GetQuestion(linkedQuestion.QuestionId).AsLinkedToList.SetOptions(linkedQuestion.Options);
        }

        public virtual void Apply(GroupsDisabled @event)
        {
            foreach (var groupIdentity in @event.Groups)
                this.Tree.GetGroup(groupIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableGroups(@event.Groups);
        }

        public virtual void Apply(GroupsEnabled @event)
        {
            foreach (var groupIdentity in @event.Groups)
                this.Tree.GetGroup(groupIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableGroups(@event.Groups);
        }

        public virtual void Apply(VariablesDisabled @event)
        {
            foreach (var variableIdentity in @event.Variables)
                this.Tree.GetVariable(variableIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableVariables(@event.Variables);
        }

        public virtual void Apply(VariablesEnabled @event)
        {
            foreach (var variableIdentity in @event.Variables)
                this.Tree.GetVariable(variableIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableVariables(@event.Variables);
        }

        public virtual void Apply(VariablesChanged @event)
        {
            foreach (var changedVariableValueDto in @event.ChangedVariables)
            {
                this.Tree.GetVariable(changedVariableValueDto.Identity).SetValue(changedVariableValueDto.NewValue);
                this.ExpressionProcessorStatePrototype.UpdateVariableValue(changedVariableValueDto.Identity, changedVariableValueDto.NewValue);
            }
        }

        public virtual void Apply(QuestionsDisabled @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.Questions);
        }

        public virtual void Apply(QuestionsEnabled @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableQuestions(@event.Questions);
        }

        public virtual void Apply(StaticTextsEnabled @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity)?.Enable();
            
            this.ExpressionProcessorStatePrototype.EnableStaticTexts(@event.StaticTexts);
        }

        public virtual void Apply(StaticTextsDisabled @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableStaticTexts(@event.StaticTexts);
        }

        public virtual void Apply(AnswerCommented @event)
        {
            var commentByQuestion = Identity.Create(@event.QuestionId, @event.RosterVector);

            var userRole = @event.UserId == this.properties.InterviewerId
                ? UserRoles.Operator
                : @event.UserId == this.properties.SupervisorId ? UserRoles.Supervisor : UserRoles.Headquarter;

            this.Tree.GetQuestion(commentByQuestion).AnswerComments.Add(new AnswerComment(@event.UserId, userRole, @event.CommentTime, @event.Comment,
                commentByQuestion));
        }

        public virtual void Apply(FlagSetToAnswer @event) { }

        public virtual void Apply(TranslationSwitched @event)
        {
            this.Language = @event.Language;

            var questionnaire = this.GetQuestionnaireOrThrow();

            this.UpdateTitlesAndTexts(questionnaire);
            this.Tree.SwitchQuestionnaire(questionnaire);
        }

        public virtual void Apply(FlagRemovedFromAnswer @event) { }

        public virtual void Apply(SubstitutionTitlesChanged @event)
        {
            foreach (var @group in @event.Groups)
                this.Tree.GetGroup(@group).ReplaceSubstitutions();

            foreach (var staticText in @event.StaticTexts)
                this.Tree.GetStaticText(staticText).ReplaceSubstitutions();

            foreach (var question in @event.Questions)
                this.Tree.GetQuestion(question).ReplaceSubstitutions();
        }

        public virtual void Apply(GroupPropagated @event)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            Guid? parentGroupId = questionnaire.GetParentGroup(@event.GroupId);
            if (!parentGroupId.HasValue) return;

            var parentGroupIdentity = Identity.Create(parentGroupId.Value, @event.OuterScopeRosterVector);

            for (int i = 0; i < @event.Count; i++)
            {
                var rosterIdentity = new RosterIdentity(@event.GroupId, @event.OuterScopeRosterVector, i).ToIdentity();

                var addedRoster = this.Tree.GetRosterManager(@event.GroupId)
                        .CreateRoster(parentGroupIdentity, rosterIdentity, i);

                this.Tree.GetGroup(parentGroupIdentity).AddChild(addedRoster);
            }

            //expressionProcessorStatePrototype could also be changed but it's an old code.
        }

        public virtual void Apply(RosterInstancesTitleChanged @event)
        {
            foreach (var changedRosterTitle in @event.ChangedInstances)
                this.Tree.GetRoster(changedRosterTitle.RosterInstance.GetIdentity()).SetRosterTitle(changedRosterTitle.Title);
        }

        public virtual void Apply(RosterInstancesAdded @event)
        {
            foreach (var instance in @event.Instances.OrderBy(x => x.OuterRosterVector.Length))
            {
                var rosterIdentity = new RosterIdentity(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex);
                this.AddRosterToTree(rosterIdentity);
                this.ExpressionProcessorStatePrototype.AddRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex);
            }
        }

        public virtual void Apply(RosterInstancesRemoved @event)
        {
            foreach (var instance in @event.Instances)
            {
                this.Tree.RemoveNode(instance.GetIdentity());
                this.ExpressionProcessorStatePrototype.RemoveRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId);
            }
        }

        public virtual void Apply(InterviewStatusChanged @event)
        {
            this.properties.Status = @event.Status;
        }

        public virtual void Apply(SupervisorAssigned @event)
        {
            this.properties.SupervisorId = @event.SupervisorId;
        }

        public virtual void Apply(InterviewerAssigned @event)
        {
            this.properties.InterviewerId = @event.InterviewerId;
            this.properties.IsReceivedByInterviewer = false;
        }

        public virtual void Apply(InterviewDeleted @event) { }

        public virtual void Apply(InterviewHardDeleted @event)
        {
            this.properties.IsHardDeleted = true;
        }

        public virtual void Apply(InterviewSentToHeadquarters @event) { }

        public virtual void Apply(InterviewRestored @event) { }

        public virtual void Apply(InterviewCompleted @event)
        {
            this.properties.WasCompleted = true;
        }

        public virtual void Apply(InterviewRestarted @event) { }

        public virtual void Apply(InterviewApproved @event) { }

        public virtual void Apply(InterviewApprovedByHQ @event) { }

        public virtual void Apply(UnapprovedByHeadquarters @event) { }

        public virtual void Apply(InterviewRejected @event)
        {
            this.properties.WasCompleted = false;
        }

        public virtual void Apply(InterviewRejectedByHQ @event) { }

        public virtual void Apply(InterviewDeclaredValid @event) { }

        public virtual void Apply(InterviewDeclaredInvalid @event) { }

        public virtual void Apply(AnswersRemoved @event)
        {
            foreach (var identity in @event.Questions)
            {
                // can be removed from removed roster. No need for this event anymore
                this.Tree.GetQuestion(identity)?.RemoveAnswer();
                this.ExpressionProcessorStatePrototype.RemoveAnswer(new Identity(identity.Id, identity.RosterVector));
            }
        }

        public virtual void Apply(AnswerRemoved @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).RemoveAnswer();
            this.ExpressionProcessorStatePrototype.RemoveAnswer(new Identity(@event.QuestionId, @event.RosterVector));
        }
    }
}
