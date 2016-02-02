using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    internal class ChaptersInfoViewDenormalizer :
        AbstractFunctionalEventHandler<GroupInfoView, IReadSideKeyValueStorage<GroupInfoView>>,
        IUpdateHandler<GroupInfoView, NewQuestionnaireCreated>,
        IUpdateHandler<GroupInfoView, TemplateImported>,
        IUpdateHandler<GroupInfoView, QuestionnaireCloned>,
        IUpdateHandler<GroupInfoView, NewGroupAdded>,
        IUpdateHandler<GroupInfoView, GroupCloned>,
        IUpdateHandler<GroupInfoView, GroupUpdated>,
        IUpdateHandler<GroupInfoView, GroupDeleted>,
        IUpdateHandler<GroupInfoView, NewQuestionAdded>,
        IUpdateHandler<GroupInfoView, QuestionChanged>,
        IUpdateHandler<GroupInfoView, QuestionCloned>,
        IUpdateHandler<GroupInfoView, QuestionDeleted>,
        IUpdateHandler<GroupInfoView, NumericQuestionAdded>,
        IUpdateHandler<GroupInfoView, NumericQuestionChanged>,
        IUpdateHandler<GroupInfoView, NumericQuestionCloned>,
        IUpdateHandler<GroupInfoView, TextListQuestionAdded>,
        IUpdateHandler<GroupInfoView, TextListQuestionChanged>,
        IUpdateHandler<GroupInfoView, TextListQuestionCloned>,
        IUpdateHandler<GroupInfoView, QRBarcodeQuestionAdded>,
        IUpdateHandler<GroupInfoView, QRBarcodeQuestionUpdated>,
        IUpdateHandler<GroupInfoView, QRBarcodeQuestionCloned>,
        IUpdateHandler<GroupInfoView, MultimediaQuestionUpdated>,
        IUpdateHandler<GroupInfoView, StaticTextAdded>,
        IUpdateHandler<GroupInfoView, StaticTextUpdated>,
        IUpdateHandler<GroupInfoView, StaticTextCloned>,
        IUpdateHandler<GroupInfoView, StaticTextDeleted>,
        IUpdateHandler<GroupInfoView, QuestionnaireItemMoved>,
        IUpdateHandler<GroupInfoView, GroupBecameARoster>,
        IUpdateHandler<GroupInfoView, GroupStoppedBeingARoster>
    {

        public ChaptersInfoViewDenormalizer(IReadSideKeyValueStorage<GroupInfoView> writer)
            : base(writer)
        {
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<NewQuestionnaireCreated> @event)
        {
            return CreateQuestionnaire(@event.EventSourceId);
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<TemplateImported> @event)
        {
            GroupInfoView questionnaire = CreateQuestionnaire(@event.EventSourceId);
            this.BuildQuestionnaireFrom(@event.Payload.Source, questionnaire);

            return questionnaire;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QuestionnaireCloned> @event)
        {
            GroupInfoView questionnaire = CreateQuestionnaire(@event.EventSourceId);
            this.BuildQuestionnaireFrom(@event.Payload.QuestionnaireDocument, questionnaire);

            return questionnaire;
        }

        private void BuildQuestionnaireFrom(QuestionnaireDocument questionnaireDocument, GroupInfoView questionnaire)
        {
            questionnaireDocument.ConnectChildrenWithParent();
            this.AddQuestionnaireItem(currentState: questionnaire, sourceQuestionnaireOrGroup: questionnaireDocument);
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<NewGroupAdded> @event)
        {
            this.AddGroup(questionnaire: state,
                parentGroupId: GetNullAsParentForChapterOrParentGroupIdForGroup(@event.Payload.ParentGroupPublicKey, state.ItemId),
                groupId: @event.Payload.PublicKey.FormatGuid(),
                groupTitle: @event.Payload.GroupText, variableName:@event.Payload.VariableName, 
                groupConditionExpression: @event.Payload.ConditionExpression);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<GroupCloned> @event)
        {
            this.AddGroup(questionnaire: state,
                parentGroupId: GetNullAsParentForChapterOrParentGroupIdForGroup(@event.Payload.ParentGroupPublicKey, state.ItemId),
                groupId: @event.Payload.PublicKey.FormatGuid(),
                groupTitle: @event.Payload.GroupText, variableName: @event.Payload.VariableName, 
                groupConditionExpression: @event.Payload.ConditionExpression, 
                orderIndex: @event.Payload.TargetIndex);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<GroupUpdated> @event)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: state,
                groupId: @event.Payload.GroupPublicKey.FormatGuid());

            groupView.Title = @event.Payload.GroupText;
            groupView.Variable = @event.Payload.VariableName;
            groupView.HasCondition = !string.IsNullOrWhiteSpace(@event.Payload.ConditionExpression);
            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<GroupDeleted> @event)
        { 
            var groupId = @event.Payload.GroupPublicKey.FormatGuid();
            var parentGroupView = this.FindParentOfEntity(questionnaireOrGroup: state,
                entityId: groupId);

            parentGroupView.Items.Remove(parentGroupView.Items.Find(group => group.ItemId == groupId));

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<NewQuestionAdded> @event)
        {
            this.AddQuestion(questionnaire: state,
                groupId: @event.Payload.GroupPublicKey.Value.FormatGuid(),
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: @event.Payload.QuestionType,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
               validationCondions: @event.Payload.ValidationConditions,
                linkedToQuestionId: Monads.Maybe(() => @event.Payload.LinkedToQuestionId.FormatGuid()));

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QuestionCloned> @event)
        {
            this.AddQuestion(questionnaire: state,
                groupId: @event.Payload.GroupPublicKey.Value.FormatGuid(),
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: @event.Payload.QuestionType,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
                validationCondions: @event.Payload.ValidationConditions,
                linkedToQuestionId: Monads.Maybe(() => @event.Payload.LinkedToQuestionId.FormatGuid()),
                orderIndex: @event.Payload.TargetIndex);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<NumericQuestionAdded> @event)
        {
            this.AddQuestion(questionnaire: state,
                groupId: @event.Payload.GroupPublicKey.FormatGuid(),
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: QuestionType.Numeric,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
                validationCondions: @event.Payload.ValidationConditions);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<NumericQuestionCloned> @event)
        {
            this.AddQuestion(questionnaire: state,
                groupId: @event.Payload.GroupPublicKey.FormatGuid(),
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: QuestionType.Numeric,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
                validationCondions: @event.Payload.ValidationConditions,
                orderIndex: @event.Payload.TargetIndex);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<TextListQuestionAdded> @event)
        {
            this.AddQuestion(questionnaire: state,
                groupId: @event.Payload.GroupId.FormatGuid(),
                 questionId: @event.Payload.PublicKey.FormatGuid(),
                 questionTitle: @event.Payload.QuestionText,
                 questionType: QuestionType.TextList,
                 questionVariable: @event.Payload.StataExportCaption,
                 questionConditionExpression: @event.Payload.ConditionExpression,
                 validationCondions: @event.Payload.ValidationConditions); 

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<TextListQuestionCloned> @event)
        {
            this.AddQuestion(questionnaire: state, 
                groupId: @event.Payload.GroupId.FormatGuid(),
                 questionId: @event.Payload.PublicKey.FormatGuid(), 
                 questionTitle: @event.Payload.QuestionText,
                 questionType: QuestionType.TextList, 
                 questionVariable: @event.Payload.StataExportCaption,
                 questionConditionExpression: @event.Payload.ConditionExpression,
                 validationCondions: @event.Payload.ValidationConditions, 
                 orderIndex: @event.Payload.TargetIndex);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QRBarcodeQuestionAdded> @event)
        {
            this.AddQuestion(questionnaire: state, 
                groupId: @event.Payload.ParentGroupId.FormatGuid(),
                 questionId: @event.Payload.QuestionId.FormatGuid(), 
                 questionTitle: @event.Payload.Title,
                 questionType: QuestionType.QRBarcode, 
                 questionVariable: @event.Payload.VariableName,
                 questionConditionExpression: @event.Payload.EnablementCondition,
                 validationCondions: @event.Payload.ValidationConditions);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QRBarcodeQuestionCloned> @event)
        {
            this.AddQuestion(questionnaire: state, groupId: @event.Payload.ParentGroupId.FormatGuid(),
                 questionId: @event.Payload.QuestionId.FormatGuid(), questionTitle: @event.Payload.Title,
                 questionType: QuestionType.QRBarcode, questionVariable: @event.Payload.VariableName,
                 questionConditionExpression: @event.Payload.EnablementCondition,
                validationCondions: @event.Payload.ValidationConditions,
                 orderIndex: @event.Payload.TargetIndex);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QuestionDeleted> @event)
        {
            var questionId = @event.Payload.QuestionId.FormatGuid();
            var parentGroupOfQuestion = this.FindParentOfEntity(state, questionId);

            parentGroupOfQuestion.Items.Remove(
                parentGroupOfQuestion.Items.Find(question => question.ItemId == questionId));

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QuestionChanged> @event)
        {
            this.UpdateQuestion(questionnaire: state,
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: @event.Payload.QuestionType,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
                questionValidationConditions: @event.Payload.ValidationConditions,
                linkedToQuestionId: Monads.Maybe(() => @event.Payload.LinkedToQuestionId.FormatGuid()));

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<NumericQuestionChanged> @event)
        {
            this.UpdateQuestion(questionnaire: state,
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: QuestionType.Numeric,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
                questionValidationConditions: @event.Payload.ValidationConditions,
                linkedToQuestionId: null);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<TextListQuestionChanged> @event)
        {
            this.UpdateQuestion(questionnaire: state,
                questionId: @event.Payload.PublicKey.FormatGuid(),
                questionTitle: @event.Payload.QuestionText,
                questionType: QuestionType.TextList,
                questionVariable: @event.Payload.StataExportCaption,
                questionConditionExpression: @event.Payload.ConditionExpression,
                questionValidationConditions: @event.Payload.ValidationConditions,
                linkedToQuestionId: null);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QRBarcodeQuestionUpdated> @event)
        {
            this.UpdateQuestion(questionnaire: state,
                questionId: @event.Payload.QuestionId.FormatGuid(),
                questionTitle: @event.Payload.Title,
                questionType: QuestionType.QRBarcode,
                questionVariable: @event.Payload.VariableName,
                questionConditionExpression: @event.Payload.EnablementCondition,
                questionValidationConditions: @event.Payload.ValidationConditions,
                linkedToQuestionId: null);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<MultimediaQuestionUpdated> @event)
        {
            this.UpdateQuestion(questionnaire: state,
                questionId: @event.Payload.QuestionId.FormatGuid(),
                questionTitle: @event.Payload.Title,
                questionType: QuestionType.Multimedia,
                questionVariable: @event.Payload.VariableName,
                questionConditionExpression: @event.Payload.EnablementCondition,
                questionValidationConditions: @event.Payload.ValidationConditions,
                linkedToQuestionId: null);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<StaticTextAdded> @event)
        {
            this.AddStaticText(questionnaire: state, parentId: @event.Payload.ParentId.FormatGuid(),
                entityId: @event.Payload.EntityId.FormatGuid(), text: @event.Payload.Text);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<StaticTextCloned> @event)
        {
            this.AddStaticText(questionnaire: state, parentId: @event.Payload.ParentId.FormatGuid(),
                entityId: @event.Payload.EntityId.FormatGuid(), text: @event.Payload.Text,
                orderIndex: @event.Payload.TargetIndex);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<StaticTextUpdated> @event)
        {
            this.UpdateStaticText(questionnaire: state, entityId: @event.Payload.EntityId.FormatGuid(),
                text: @event.Payload.Text);

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<StaticTextDeleted> @event)
        {
            var entityId = @event.Payload.EntityId.FormatGuid();
            var parentGroupOfEntity = this.FindParentOfEntity(state, entityId);

            parentGroupOfEntity.Items.Remove(
                parentGroupOfEntity.Items.Find(entity => entity.ItemId == entityId));

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<QuestionnaireItemMoved> @event)
        {
            var groupOrQuestionKey = @event.Payload.PublicKey.FormatGuid();

            var targetGroupKey = @event.Payload.GroupKey.HasValue
                ? @event.Payload.GroupKey.Value.FormatGuid()
                : state.ItemId;

            var targetGroup = this.FindGroup(state, targetGroupKey);
            var entityView = this.FindEntity<IQuestionnaireItem>(state, groupOrQuestionKey);
            var parentOfEntity = this.FindParentOfEntity(state, groupOrQuestionKey);

            if (targetGroup != null && entityView != null && parentOfEntity != null)
            {
                parentOfEntity.Items.Remove(entityView);

                if (@event.Payload.TargetIndex < 0)
                {
                    targetGroup.Items.Insert(0, entityView);
                }
                else if (@event.Payload.TargetIndex >= targetGroup.Items.Count)
                {
                    targetGroup.Items.Add(entityView);
                }
                else
                {
                    targetGroup.Items.Insert(@event.Payload.TargetIndex, entityView);
                }
            }

            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<GroupBecameARoster> @event)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: state,
               groupId: @event.Payload.GroupId.FormatGuid());

            groupView.IsRoster = true;
            
            return state;
        }

        public GroupInfoView Update(GroupInfoView state, IPublishedEvent<GroupStoppedBeingARoster> @event)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: state,
                groupId: @event.Payload.GroupId.FormatGuid());

            groupView.IsRoster = false;

            return state;
        }

        private StaticTextInfoView FindStaticText(GroupInfoView questionnaireOrGroup, string entityId)
        {
            return this.FindEntity<StaticTextInfoView>(questionnaireOrGroup, entityId);
        }

        private QuestionInfoView FindQuestion(GroupInfoView questionnaireOrGroup, string questionId)
        {
            return this.FindEntity<QuestionInfoView>(questionnaireOrGroup, questionId);
        }

        private GroupInfoView FindGroup(GroupInfoView questionnaireOrGroup, string groupId)
        {
            return this.FindEntity<GroupInfoView>(questionnaireOrGroup, groupId);
        }

        private T FindEntity<T>(IQuestionnaireItem questionnaireOrGroup, string entityId) where T : IQuestionnaireItem
        {
            IQuestionnaireItem retVal = null;

            if (questionnaireOrGroup.ItemId == entityId)
                retVal = questionnaireOrGroup;
            else
            {
                var questionnaireItemAsGroup = questionnaireOrGroup as GroupInfoView;
                if (questionnaireItemAsGroup != null)
                {
                    foreach (var groupInfoView in questionnaireItemAsGroup.Items)
                    {
                        retVal = this.FindEntity<T>(groupInfoView, entityId);
                        if (retVal != null) break;
                    }
                }
            }

            return (T)retVal;
        }

        private GroupInfoView FindParentOfEntity(GroupInfoView questionnaireOrGroup, string entityId)
        {
            GroupInfoView findedGroup = null;

            if (questionnaireOrGroup.Items.Any(group => group.ItemId == entityId))
                findedGroup = questionnaireOrGroup;
            else
            {
                foreach (var groupInfoView in questionnaireOrGroup.Items.OfType<GroupInfoView>())
                {
                    findedGroup = this.FindParentOfEntity(groupInfoView, entityId);
                    if (findedGroup != null) break;
                }
            }

            return findedGroup;
        }

        private void AddQuestion(GroupInfoView questionnaire,
            string groupId,
            string questionId,
            string questionTitle,
            QuestionType questionType,
            string questionVariable,
            string questionConditionExpression,
            List<ValidationCondition> validationCondions,
            string linkedToQuestionId = null,
            int? orderIndex = null)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: questionnaire, groupId: groupId);

            if (groupView == null)
            {
                return;
            }

            var questionInfoView = new QuestionInfoView
            {
                ItemId = questionId,
                Title = questionTitle,
                Type = questionType,
                Variable = questionVariable,
                LinkedToQuestionId = linkedToQuestionId,
                HasCondition = !string.IsNullOrWhiteSpace(questionConditionExpression),
                HasValidation = validationCondions.Count > 0    
            };

            if (orderIndex.HasValue)
            {
                groupView.Items.Insert(orderIndex.Value, questionInfoView);
            }
            else
            {
                groupView.Items.Add(questionInfoView);
            }

        }

        private void UpdateQuestion(GroupInfoView questionnaire,
            string questionId,
            string questionTitle,
            QuestionType questionType,
            string questionVariable,
            string questionConditionExpression,
            List<ValidationCondition> questionValidationConditions,
            string linkedToQuestionId)
        {
            var questionView = this.FindQuestion(questionnaireOrGroup: questionnaire, questionId: questionId);

            if (questionView == null)
                return;

            questionView.Title = questionTitle;
            questionView.Type = questionType;
            questionView.Variable = questionVariable;
            questionView.LinkedToQuestionId = linkedToQuestionId;
            questionView.HasCondition = !string.IsNullOrWhiteSpace(questionConditionExpression);
            questionView.HasValidation = questionValidationConditions.Count > 0;
        }

        private void AddStaticText(GroupInfoView questionnaire, string parentId, string entityId, string text,
            int? orderIndex = null)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: questionnaire, groupId: parentId);
            if (groupView == null)
            {
                return;
            }

            var staticTextInfoView = new StaticTextInfoView()
            {
                ItemId = entityId,
                Text = text
            };

            if (orderIndex.HasValue)
            {
                groupView.Items.Insert(orderIndex.Value, staticTextInfoView);
            }
            else
            {
                groupView.Items.Add(staticTextInfoView);
            }
        }

        private void UpdateStaticText(GroupInfoView questionnaire, string entityId, string text)
        {
            var staticTextInfoView = this.FindStaticText(questionnaireOrGroup: questionnaire, entityId: entityId);

            if (staticTextInfoView == null)
                return;

            staticTextInfoView.Text = text;
        }

        private void AddGroup(GroupInfoView questionnaire, string parentGroupId, string groupId, string groupTitle, string variableName,
            string groupConditionExpression,
            bool isRoster = false, int? orderIndex = null)
        {
            var parentGroup = string.IsNullOrEmpty(parentGroupId)
                ? questionnaire
                : this.FindGroup(questionnaireOrGroup: questionnaire, groupId: parentGroupId);

            if (parentGroup == null)
                return;

            var groupInfoView = new GroupInfoView()
            {
                ItemId = groupId,
                Title = groupTitle,
                Variable = variableName,
                IsRoster = isRoster,
                HasCondition = !string.IsNullOrWhiteSpace(groupConditionExpression),
                Items = new List<IQuestionnaireItem>(),
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            if (orderIndex.HasValue)
            {
                parentGroup.Items.Insert(orderIndex.Value, groupInfoView);
            }
            else
            {
                parentGroup.Items.Add(groupInfoView);
            }
        }

        private void AddQuestionnaireItem(GroupInfoView currentState, IGroup sourceQuestionnaireOrGroup)
        {
            foreach (var child in sourceQuestionnaireOrGroup.Children)
            {
                var questionChild = child as IQuestion;
                if (questionChild != null)
                {
                    this.AddQuestion(questionnaire: currentState,
                                     groupId: questionChild.GetParent().PublicKey.FormatGuid(),
                                     questionId: questionChild.PublicKey.FormatGuid(),
                                     questionTitle: questionChild.QuestionText,
                                     questionType: questionChild.QuestionType,
                                     questionVariable: questionChild.StataExportCaption,
                                     questionConditionExpression: questionChild.ConditionExpression,
                                     validationCondions: questionChild.ValidationConditions,
                                     linkedToQuestionId: Monads.Maybe(() => questionChild.LinkedToQuestionId.FormatGuid()));

                    continue;
                }

                IGroup groupChild = child as IGroup;
                if (groupChild != null)
                {
                    this.AddGroup(questionnaire: currentState,
                                  groupId: groupChild.PublicKey.FormatGuid(),
                                  parentGroupId: groupChild.GetParent().PublicKey.FormatGuid(),
                                  groupTitle: groupChild.Title,
                                  variableName: groupChild.VariableName,
                                  groupConditionExpression: groupChild.ConditionExpression,
                                  isRoster: groupChild.IsRoster);
                    this.AddQuestionnaireItem(currentState: currentState, sourceQuestionnaireOrGroup: groupChild);

                    continue;
                }

                var staticTextChild = child as IStaticText;
                if (staticTextChild != null)
                {
                    this.AddStaticText(questionnaire: currentState,
                        parentId: staticTextChild.GetParent().PublicKey.FormatGuid(),
                        entityId: staticTextChild.PublicKey.FormatGuid(),
                        text: staticTextChild.Text);

                    continue;
                }

                throw new ArgumentException(string.Format("Unknown questionnaire item type in item with id {0}, received type is: {1}", child.PublicKey, child.GetType()));
            }
        }

        private static GroupInfoView CreateQuestionnaire(Guid questionnaireId)
        {
            return new GroupInfoView()
            {
                ItemId = questionnaireId.FormatGuid(),
                Items = new List<IQuestionnaireItem>(),
            };
        }

        private static string GetNullAsParentForChapterOrParentGroupIdForGroup(Guid? sourceParentGroupId, string questionnaireId)
        {
            return !sourceParentGroupId.HasValue || sourceParentGroupId.Value.FormatGuid() == questionnaireId
                ? null
                : sourceParentGroupId.Value.FormatGuid();
        }
    }
}
