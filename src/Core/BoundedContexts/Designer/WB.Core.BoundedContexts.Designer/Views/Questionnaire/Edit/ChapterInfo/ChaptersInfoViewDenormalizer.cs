using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    internal class ChaptersInfoViewDenormalizer :
        AbstractFunctionalEventHandler<GroupInfoView>,
        ICreateHandler<GroupInfoView, NewQuestionnaireCreated>,
        ICreateHandler<GroupInfoView, TemplateImported>,
        ICreateHandler<GroupInfoView, QuestionnaireCloned>,
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
        IUpdateHandler<GroupInfoView, QuestionnaireItemMoved>

    {
        private readonly IExpressionProcessor expressionProcessor;

        public ChaptersInfoViewDenormalizer(IReadSideRepositoryWriter<GroupInfoView> writer, IExpressionProcessor expressionProcessor)
            : base(writer)
        {
            this.expressionProcessor = expressionProcessor;
        }

        public override Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public override Type[] BuildsViews
        {
            get { return base.BuildsViews.Union(new[] {typeof (GroupInfoView)}).ToArray(); }
        }

        public GroupInfoView Create(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            return CreateQuestionnaire(evnt.EventSourceId);
        }

        public GroupInfoView Create(IPublishedEvent<TemplateImported> evnt)
        {
            var questionnaire = CreateQuestionnaire(evnt.EventSourceId);

            evnt.Payload.Source.ConnectChildrenWithParent();
            this.AddQuestionnaireItem(currentState: questionnaire, sourceQuestionnaireOrGroup: evnt.Payload.Source);

            return questionnaire;
        }

        public GroupInfoView Create(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var questionnaire = CreateQuestionnaire(evnt.EventSourceId);

            evnt.Payload.QuestionnaireDocument.ConnectChildrenWithParent();
            this.AddQuestionnaireItem(currentState: questionnaire, sourceQuestionnaireOrGroup: evnt.Payload.QuestionnaireDocument);

            return questionnaire;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            this.AddGroup(questionnaire: currentState,
                parentGroupId: GetNullAsParentForChapterOrParentGroupIdForGroup(evnt.Payload.ParentGroupPublicKey, currentState.ItemId),
                groupId: evnt.Payload.PublicKey.FormatGuid(), groupTitle: evnt.Payload.GroupText);


            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            this.AddGroup(questionnaire: currentState,
                parentGroupId: GetNullAsParentForChapterOrParentGroupIdForGroup(evnt.Payload.ParentGroupPublicKey, currentState.ItemId),
                groupId: evnt.Payload.PublicKey.FormatGuid(), groupTitle: evnt.Payload.GroupText, orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<GroupUpdated> evnt)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: currentState,
                groupId: evnt.Payload.GroupPublicKey.FormatGuid());

            groupView.Title = evnt.Payload.GroupText;

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<GroupDeleted> evnt)
        {
            var groupId = evnt.Payload.GroupPublicKey.FormatGuid();
            var parentGroupView = this.FindParentOfGroupOrQuestion(questionnaireOrGroup: currentState,
                groupId: groupId);

            parentGroupView.Items.Remove(parentGroupView.Items.Find(group => group.ItemId == groupId));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<NewQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupPublicKey.Value.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                questionType: evnt.Payload.QuestionType, questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<QuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupPublicKey.Value.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                questionType: evnt.Payload.QuestionType, questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression, orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<NumericQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupPublicKey.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                questionType: QuestionType.Numeric, questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<NumericQuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupPublicKey.FormatGuid(),
                questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                questionType: QuestionType.Numeric, questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression, orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<TextListQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupId.FormatGuid(),
                 questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                 questionType: QuestionType.TextList, questionVariable: evnt.Payload.StataExportCaption,
                 questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<TextListQuestionCloned> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.GroupId.FormatGuid(),
                 questionId: evnt.Payload.PublicKey.FormatGuid(), questionTitle: evnt.Payload.QuestionText,
                 questionType: QuestionType.TextList, questionVariable: evnt.Payload.StataExportCaption,
                 questionConditionExpression: evnt.Payload.ConditionExpression, orderIndex: evnt.Payload.TargetIndex);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            this.AddQuestion(questionnaire: currentState, groupId: evnt.Payload.ParentGroupId.FormatGuid(),
                 questionId: evnt.Payload.QuestionId.FormatGuid(), questionTitle: evnt.Payload.Title,
                 questionType: QuestionType.QRBarcode, questionVariable: evnt.Payload.VariableName,
                 questionConditionExpression: evnt.Payload.EnablementCondition);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState,IPublishedEvent<QRBarcodeQuestionCloned> evnt)
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
            var parentGroupOfQuestion = this.FindParentOfGroupOrQuestion(currentState, questionId);

            parentGroupOfQuestion.Items.Remove(
                parentGroupOfQuestion.Items.Find(question => question.ItemId == questionId));

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionChanged> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState, questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText, questionType: evnt.Payload.QuestionType,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NumericQuestionChanged> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState, questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText, questionType: QuestionType.Numeric,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<TextListQuestionChanged> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState, questionId: evnt.Payload.PublicKey.FormatGuid(),
                questionTitle: evnt.Payload.QuestionText, questionType: QuestionType.TextList,
                questionVariable: evnt.Payload.StataExportCaption,
                questionConditionExpression: evnt.Payload.ConditionExpression);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            this.UpdateQuestion(questionnaire: currentState, questionId: evnt.Payload.QuestionId.FormatGuid(),
                questionTitle: evnt.Payload.Title, questionType: QuestionType.QRBarcode,
                questionVariable: evnt.Payload.VariableName,
                questionConditionExpression: evnt.Payload.EnablementCondition);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            var groupOrQuestionKey = evnt.Payload.PublicKey.FormatGuid();

            var targetGroupKey = evnt.Payload.GroupKey.HasValue
                ? evnt.Payload.GroupKey.Value.FormatGuid()
                : currentState.ItemId;

            var targetGroup = this.FindGroup(currentState, targetGroupKey);


            var groupOrQuestionView = this.FindGroupOrQuestion<IQuestionnaireItem>(currentState, groupOrQuestionKey);
            if (groupOrQuestionView != null)
            {
                var parentOfGroup = this.FindParentOfGroupOrQuestion(currentState, groupOrQuestionView.ItemId);

                parentOfGroup.Items.Remove(groupOrQuestionView);
                targetGroup.Items.Insert(Math.Min(evnt.Payload.TargetIndex, targetGroup.Items.Count), groupOrQuestionView);
            }

            return currentState;
        }

        private QuestionInfoView FindQuestion(GroupInfoView questionnaireOrGroup, string questionId)
        {
            return this.FindGroupOrQuestion<QuestionInfoView>(questionnaireOrGroup, questionId);
        }

        private GroupInfoView FindGroup(GroupInfoView questionnaireOrGroup, string groupId)
        {
            return this.FindGroupOrQuestion<GroupInfoView>(questionnaireOrGroup, groupId);
        }

        private T FindGroupOrQuestion<T>(IQuestionnaireItem questionnaireOrGroup, string groupOrQuestionId) where T : IQuestionnaireItem
        {
            IQuestionnaireItem retVal = null;

            if (questionnaireOrGroup.ItemId == groupOrQuestionId)
                retVal = questionnaireOrGroup;
            else
            {
                var questionnaireItemAsGroup = questionnaireOrGroup as GroupInfoView;
                if (questionnaireItemAsGroup != null)
                {
                    foreach (var groupInfoView in questionnaireItemAsGroup.Items)
                    {
                        retVal = this.FindGroupOrQuestion<T>(groupInfoView, groupOrQuestionId);
                        if (retVal != null) break;
                    }    
                }
                
            }

            return (T)retVal;
        }

        private GroupInfoView FindParentOfGroupOrQuestion(GroupInfoView questionnaireOrGroup, string groupId)
        {
            GroupInfoView findedGroup = null;

            if (questionnaireOrGroup.Items.Any(group => group.ItemId == groupId))
                findedGroup = questionnaireOrGroup;
            else
            {
                foreach (var groupInfoView in questionnaireOrGroup.Items.OfType<GroupInfoView>())
                {
                    findedGroup = this.FindParentOfGroupOrQuestion(groupInfoView, groupId);
                    if (findedGroup != null) break;
                }
            }

            return findedGroup;
        }
        
        private void AddQuestion(GroupInfoView questionnaire, string groupId, string questionId, string questionTitle,
            QuestionType questionType, string questionVariable, string questionConditionExpression, int? orderIndex = null)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: questionnaire, groupId: groupId);

            if (groupView == null)
            {
                return;
            }

            var questionsUsedInConditionExpression = string.IsNullOrEmpty(questionConditionExpression)
                        ? new string[0]
                        : expressionProcessor.GetIdentifiersUsedInExpression(questionConditionExpression);

            var questionInfoView = new QuestionInfoView()
            {
                ItemId = questionId,
                Title = questionTitle,
                Type = questionType,
                Variable = questionVariable,
                LinkedVariables = questionsUsedInConditionExpression
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

        private void UpdateQuestion(GroupInfoView questionnaire, string questionId, string questionTitle,
            QuestionType questionType, string questionVariable, string questionConditionExpression)
        {
            var questionView = this.FindQuestion(questionnaireOrGroup: questionnaire, questionId: questionId);

            if (questionView == null)
                return;

            var questionsUsedInConditionExpression = string.IsNullOrEmpty(questionConditionExpression)
                        ? new string[0]
                        : expressionProcessor.GetIdentifiersUsedInExpression(questionConditionExpression);

            questionView.Title = questionTitle;
            questionView.Type = questionType;
            questionView.Variable = questionVariable;
            questionView.LinkedVariables = questionsUsedInConditionExpression;
        }

        private void AddGroup(GroupInfoView questionnaire, string parentGroupId, string groupId, string groupTitle, int? orderIndex = null)
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
            foreach (var group in sourceQuestionnaireOrGroup.Children.OfType<IGroup>())
            {
                this.AddGroup(questionnaire: currentState, groupId: group.PublicKey.FormatGuid(),
                    parentGroupId: group.GetParent().PublicKey.FormatGuid(), groupTitle: group.Title);
                this.AddQuestionnaireItem(currentState: currentState, sourceQuestionnaireOrGroup: @group);
            }

            foreach (var question in sourceQuestionnaireOrGroup.Children.OfType<IQuestion>())
            {
                this.AddQuestion(questionnaire: currentState, groupId: question.GetParent().PublicKey.FormatGuid(),
                    questionId: question.PublicKey.FormatGuid(), questionTitle: question.QuestionText,
                    questionType: question.QuestionType, questionVariable: question.StataExportCaption,
                    questionConditionExpression: question.ConditionExpression);
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
