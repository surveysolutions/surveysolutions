using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            return CreateQuestionnaire(evnt.EventSourceId);
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<TemplateImported> evnt)
        {
            GroupInfoView questionnaire = CreateQuestionnaire(evnt.EventSourceId);
            this.BuildQuestionnaireFrom(evnt.Payload.Source, questionnaire);

            return questionnaire;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionnaireCloned> evnt)
        {
            GroupInfoView questionnaire = CreateQuestionnaire(evnt.EventSourceId);
            this.BuildQuestionnaireFrom(evnt.Payload.QuestionnaireDocument, questionnaire);

            return questionnaire;
        }

        private void BuildQuestionnaireFrom(QuestionnaireDocument questionnaireDocument, GroupInfoView questionnaire)
        {
            questionnaireDocument.ConnectChildrenWithParent();
            this.AddQuestionnaireItem(currentState: questionnaire, sourceQuestionnaireOrGroup: questionnaireDocument);
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            this.AddGroup(questionnaire: currentState,
                parentGroupId: GetNullAsParentForChapterOrParentGroupIdForGroup(evnt.Payload.ParentGroupPublicKey, currentState.ItemId),
                groupId: evnt.Payload.PublicKey.FormatGuid(),
                groupTitle: evnt.Payload.GroupText, variableName:evnt.Payload.VariableName);


            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            this.AddGroup(questionnaire: currentState,
                parentGroupId: GetNullAsParentForChapterOrParentGroupIdForGroup(evnt.Payload.ParentGroupPublicKey, currentState.ItemId),
                groupId: evnt.Payload.PublicKey.FormatGuid(),
                groupTitle: evnt.Payload.GroupText, variableName: evnt.Payload.VariableName, 
                orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupUpdated> evnt)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: currentState,
                groupId: evnt.Payload.GroupPublicKey.FormatGuid());

            groupView.Title = evnt.Payload.GroupText;
            groupView.Variable = evnt.Payload.VariableName;
            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupDeleted> evnt)
        { 
            var groupId = evnt.Payload.GroupPublicKey.FormatGuid();
            var parentGroupView = this.FindParentOfEntity(questionnaireOrGroup: currentState,
                entityId: groupId);

            parentGroupView.Items.Remove(parentGroupView.Items.Find(group => group.ItemId == groupId));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NewQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState,
                groupId: evnt.Payload.GroupPublicKey.Value.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: evnt.Payload.QuestionType,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression,
                linkedToQuestionId: Monads.Maybe(() => evnt.Payload.LinkedToQuestionId.FormatGuid()));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState,
                groupId: evnt.Payload.GroupPublicKey.Value.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: evnt.Payload.QuestionType,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression,
                linkedToQuestionId: Monads.Maybe(() => evnt.Payload.LinkedToQuestionId.FormatGuid()),
                orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NumericQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState,
                groupId: evnt.Payload.GroupPublicKey.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: QuestionType.Numeric,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NumericQuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState,
                groupId: evnt.Payload.GroupPublicKey.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: QuestionType.Numeric,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression,
                orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<TextListQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState,
                groupId: evnt.Payload.GroupId.FormatGuid(),
                 questionId: evnt.Payload.PublicKey.FormatGuid(),
                 questionTitle: evnt.Payload.QuestionText,
                 questionType: QuestionType.TextList,
                 questionVariable: evnt.Payload.StataExportCaption,
                 questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<TextListQuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupId.FormatGuid(),
                 questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                 questionType: QuestionType.TextList, questionVariable: evnt.Payload.StataExportCaption,
                 questionConditionExpression: evnt.Payload.ConditionExpression, orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.ParentGroupId.FormatGuid(),
                 questionId: evnt.Payload.QuestionId.FormatGuid(), questionTitle: evnt.Payload.Title,
                 questionType: QuestionType.QRBarcode, questionVariable: evnt.Payload.VariableName,
                 questionConditionExpression: evnt.Payload.EnablementCondition);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.ParentGroupId.FormatGuid(),
                 questionId: evnt.Payload.QuestionId.FormatGuid(), questionTitle: evnt.Payload.Title,
                 questionType: QuestionType.QRBarcode, questionVariable: evnt.Payload.VariableName,
                 questionConditionExpression: evnt.Payload.EnablementCondition, orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionDeleted> evnt)
        {
            var questionId = evnt.Payload.QuestionId.FormatGuid();
            var parentGroupOfQuestion = this.FindParentOfEntity(currentState, questionId);

            parentGroupOfQuestion.Items.Remove(
                parentGroupOfQuestion.Items.Find(question => question.ItemId == questionId));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionChanged> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState,
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: evnt.Payload.QuestionType,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression,
                linkedToQuestionId: Monads.Maybe(() => evnt.Payload.LinkedToQuestionId.FormatGuid()));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NumericQuestionChanged> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState,
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: QuestionType.Numeric,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression,
                linkedToQuestionId: null);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<TextListQuestionChanged> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState,
                questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText,
                questionType: QuestionType.TextList,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression,
                linkedToQuestionId: null);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState,
                questionId: evnt.Payload.QuestionId.FormatGuid(),
                questionTitle: evnt.Payload.Title,
                questionType: QuestionType.QRBarcode,
                questionVariable: evnt.Payload.VariableName,
                questionConditionExpression: evnt.Payload.EnablementCondition,
                linkedToQuestionId: null);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState,
                questionId: evnt.Payload.QuestionId.FormatGuid(),
                questionTitle: evnt.Payload.Title,
                questionType: QuestionType.Multimedia,
                questionVariable: evnt.Payload.VariableName,
                questionConditionExpression: evnt.Payload.EnablementCondition,
                linkedToQuestionId: null);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<StaticTextAdded> evnt)
        {
            this.AddStaticText(questionnaire: currentState, parentId: evnt.Payload.ParentId.FormatGuid(),
                entityId: evnt.Payload.EntityId.FormatGuid(), text: evnt.Payload.Text);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<StaticTextCloned> evnt)
        {
            this.AddStaticText(questionnaire: currentState, parentId: evnt.Payload.ParentId.FormatGuid(),
                entityId: evnt.Payload.EntityId.FormatGuid(), text: evnt.Payload.Text,
                orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<StaticTextUpdated> evnt)
        {
            this.UpdateStaticText(questionnaire: currentState, entityId: evnt.Payload.EntityId.FormatGuid(),
                text: evnt.Payload.Text);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<StaticTextDeleted> evnt)
        {
            var entityId = evnt.Payload.EntityId.FormatGuid();
            var parentGroupOfEntity = this.FindParentOfEntity(currentState, entityId);

            parentGroupOfEntity.Items.Remove(
                parentGroupOfEntity.Items.Find(entity => entity.ItemId == entityId));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            var groupOrQuestionKey = evnt.Payload.PublicKey.FormatGuid();

            var targetGroupKey = evnt.Payload.GroupKey.HasValue
                ? evnt.Payload.GroupKey.Value.FormatGuid()
                : currentState.ItemId;

            var targetGroup = this.FindGroup(currentState, targetGroupKey);
            var entityView = this.FindEntity<IQuestionnaireItem>(currentState, groupOrQuestionKey);
            var parentOfEntity = this.FindParentOfEntity(currentState, groupOrQuestionKey);

            if (targetGroup != null && entityView != null && parentOfEntity != null)
            {
                parentOfEntity.Items.Remove(entityView);

                if (evnt.Payload.TargetIndex < 0)
                {
                    targetGroup.Items.Insert(0, entityView);
                }
                else if (evnt.Payload.TargetIndex >= targetGroup.Items.Count)
                {
                    targetGroup.Items.Add(entityView);
                }
                else
                {
                    targetGroup.Items.Insert(evnt.Payload.TargetIndex, entityView);
                }
            }

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupBecameARoster> evnt)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: currentState,
               groupId: evnt.Payload.GroupId.FormatGuid());

            groupView.IsRoster = true;
            
            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupStoppedBeingARoster> evnt)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: currentState,
                groupId: evnt.Payload.GroupId.FormatGuid());

            groupView.IsRoster = false;

            return currentState;
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
            string linkedToQuestionId = null,
            int? orderIndex = null)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: questionnaire, groupId: groupId);

            if (groupView == null)
            {
                return;
            }

            var questionInfoView = new QuestionInfoView()
            {
                ItemId = questionId,
                Title = questionTitle,
                Type = questionType,
                Variable = questionVariable,
                LinkedToQuestionId = linkedToQuestionId
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
            string linkedToQuestionId)
        {
            var questionView = this.FindQuestion(questionnaireOrGroup: questionnaire, questionId: questionId);

            if (questionView == null)
                return;

            questionView.Title = questionTitle;
            questionView.Type = questionType;
            questionView.Variable = questionVariable;
            questionView.LinkedToQuestionId = linkedToQuestionId;
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

        private void AddGroup(GroupInfoView questionnaire, string parentGroupId, string groupId, string groupTitle,string variableName, bool isRoster = false, int? orderIndex = null)
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
                if (child is IGroup)
                {
                    IGroup group = child as IGroup;
                    this.AddGroup(questionnaire: currentState,
                                  groupId: group.PublicKey.FormatGuid(),
                                  parentGroupId: group.GetParent().PublicKey.FormatGuid(),
                                  groupTitle: group.Title,
                    variableName:group.VariableName,
                                  isRoster: group.IsRoster);
                    this.AddQuestionnaireItem(currentState: currentState, sourceQuestionnaireOrGroup: @group);
                }
                else if (child is IQuestion)
                {
                    var question = child as IQuestion;
                    this.AddQuestion(questionnaire: currentState,
                                     groupId: question.GetParent().PublicKey.FormatGuid(),
                                     questionId: question.PublicKey.FormatGuid(),
                                     questionTitle: question.QuestionText,
                                     questionType: question.QuestionType,
                                     questionVariable: question.StataExportCaption,
                                     questionConditionExpression: question.ConditionExpression,
                                     linkedToQuestionId: Monads.Maybe(() => question.LinkedToQuestionId.FormatGuid()));
                }
                else if (child is IStaticText)
                {
                    var staticText = child as IStaticText;
                    this.AddStaticText(questionnaire: currentState,
                        parentId: staticText.GetParent().PublicKey.FormatGuid(),
                        entityId: staticText.PublicKey.FormatGuid(),
                        text: staticText.Text);
                }
                else
                {
                    throw new ArgumentException(string.Format("Unknown qustionnaire item type in item with id {0}, received type is: {1}", child.PublicKey, child.GetType()));
                }
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
