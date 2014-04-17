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
            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.GroupId)
            {
                this.AddGroup(questionnaire: currentState, parentGroupId: null,
                    groupId: evnt.Payload.PublicKey.FormatGuid(), groupTitle: evnt.Payload.GroupText);
            }
            else
            {
                this.AddGroup(questionnaire: currentState,
                    parentGroupId: evnt.Payload.ParentGroupPublicKey.Value.FormatGuid(),
                    groupId: evnt.Payload.PublicKey.FormatGuid(), groupTitle: evnt.Payload.GroupText);
            }

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.GroupId)
            {
                this.AddGroup(questionnaire: currentState, parentGroupId: null,
                    groupId: evnt.Payload.PublicKey.FormatGuid(), groupTitle: evnt.Payload.GroupText);
            }
            else
            {
                this.AddGroup(questionnaire: currentState,
                    parentGroupId: evnt.Payload.ParentGroupPublicKey.Value.FormatGuid(),
                    groupId: evnt.Payload.PublicKey.FormatGuid(), groupTitle: evnt.Payload.GroupText);
            }

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
            var parentGroupView = this.FindParentOfGroup(questionnaireOrGroup: currentState,
                groupId: groupId);

            parentGroupView.Groups.Remove(parentGroupView.Groups.Find(group => group.GroupId == groupId));

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
                questionConditionExpression: evnt.Payload.ConditionExpression);

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
                questionConditionExpression: evnt.Payload.ConditionExpression);

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
                 questionConditionExpression: evnt.Payload.ConditionExpression);

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
                 questionConditionExpression: evnt.Payload.EnablementCondition);

            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<QuestionDeleted> evnt)
        {
            var questionId = evnt.Payload.QuestionId.FormatGuid();
            var parentGroupOfQuestion = this.FindParentOfQuestion(currentState, questionId);

            parentGroupOfQuestion.Questions.Remove(
                parentGroupOfQuestion.Questions.Find(question => question.QuestionId == questionId));

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
                : currentState.GroupId;

            var targetGroup = this.FindGroup(currentState, targetGroupKey);


            var groupInfoView = this.FindGroup(currentState, groupOrQuestionKey);
            if (groupInfoView != null)
            {
                var parentOfGroup = this.FindParentOfGroup(currentState, groupInfoView.GroupId);

                if (targetGroup.GroupId != parentOfGroup.GroupId)
                {
                    targetGroup.Groups.Add(groupInfoView);
                    parentOfGroup.Groups.Remove(groupInfoView);    
                }
                
            }

            var questionInfoView = this.FindQuestion(currentState, groupOrQuestionKey);
            if (questionInfoView != null)
            {
                var parentOfQuestion = this.FindParentOfQuestion(currentState, questionInfoView.QuestionId);

                if (targetGroup.GroupId != parentOfQuestion.GroupId)
                {
                    targetGroup.Questions.Add(questionInfoView);
                    parentOfQuestion.Questions.Remove(questionInfoView);
                }
            }

            return currentState;
        }

        private GroupInfoView FindGroup(GroupInfoView questionnaireOrGroup, string groupId)
        {
            GroupInfoView findedGroup = null;

            if (questionnaireOrGroup.GroupId == groupId)
                findedGroup = questionnaireOrGroup;
            else
            {
                foreach (var groupInfoView in questionnaireOrGroup.Groups)
                {
                    findedGroup = this.FindGroup(groupInfoView, groupId);
                    if (findedGroup != null) break;
                }
            }

            return findedGroup;
        }

        private GroupInfoView FindParentOfGroup(GroupInfoView questionnaireOrGroup, string groupId)
        {
            GroupInfoView findedGroup = null;

            if (questionnaireOrGroup.Groups.Any(group => group.GroupId == groupId))
                findedGroup = questionnaireOrGroup;
            else
            {
                foreach (var groupInfoView in questionnaireOrGroup.Groups)
                {
                    findedGroup = this.FindParentOfGroup(groupInfoView, groupId);
                    if (findedGroup != null) break;
                }
            }

            return findedGroup;
        }

        private QuestionInfoView FindQuestion(GroupInfoView questionnaireOrGroup, string questionId)
        {
            QuestionInfoView findedQuestion = null;

            if (questionnaireOrGroup.Questions.Any(question => question.QuestionId == questionId))
                findedQuestion = questionnaireOrGroup.Questions.Find(question => question.QuestionId == questionId);
            else
            {
                foreach (var groupInfoView in questionnaireOrGroup.Groups)
                {
                    findedQuestion = this.FindQuestion(groupInfoView, questionId);
                    if (findedQuestion != null) break;
                }
            }

            return findedQuestion;
        }

        private GroupInfoView FindParentOfQuestion(GroupInfoView questionnaireOrGroup, string questionId)
        {
            GroupInfoView findedGroup = null;

            if (questionnaireOrGroup.Questions.Any(question => question.QuestionId == questionId))
                findedGroup = questionnaireOrGroup;
            else
            {
                foreach (var groupInfoView in questionnaireOrGroup.Groups)
                {
                    findedGroup = this.FindParentOfQuestion(groupInfoView, questionId);
                    if (findedGroup != null) break;
                }
            }

            return findedGroup;
        }

        private void AddQuestion(GroupInfoView questionnaire, string groupId, string questionId, string questionTitle,
            QuestionType questionType, string questionVariable, string questionConditionExpression)
        {
            var groupView = this.FindGroup(questionnaireOrGroup: questionnaire, groupId: groupId);

            var questionsUsedInConditionExpression = string.IsNullOrEmpty(questionConditionExpression)
                        ? new string[0]
                        : expressionProcessor.GetIdentifiersUsedInExpression(questionConditionExpression);

            groupView.Questions.Add(new QuestionInfoView()
            {
                QuestionId = questionId,
                Title = questionTitle,
                Type = questionType,
                Variable = questionVariable,
                LinkedVariables = questionsUsedInConditionExpression
            });
        }

        private void UpdateQuestion(GroupInfoView questionnaire, string questionId, string questionTitle,
            QuestionType questionType, string questionVariable, string questionConditionExpression)
        {
            var questionView = this.FindQuestion(questionnaireOrGroup: questionnaire, questionId: questionId);

            var questionsUsedInConditionExpression = string.IsNullOrEmpty(questionConditionExpression)
                        ? new string[0]
                        : expressionProcessor.GetIdentifiersUsedInExpression(questionConditionExpression);

            questionView.Title = questionTitle;
            questionView.Type = questionType;
            questionView.Variable = questionVariable;
            questionView.LinkedVariables = questionsUsedInConditionExpression;
        }

        private void AddGroup(GroupInfoView questionnaire, string parentGroupId, string groupId, string groupTitle)
        {
            var parentGroup = string.IsNullOrEmpty(parentGroupId)
                ? questionnaire
                : this.FindGroup(questionnaireOrGroup: questionnaire, groupId: parentGroupId);

            var groupInfoView = new GroupInfoView()
            {
                GroupId = groupId,
                Title = groupTitle,
                Groups = new List<GroupInfoView>(),
                Questions = new List<QuestionInfoView>(),
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            parentGroup.Groups.Add(groupInfoView);
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
                GroupId = questionnaireId.FormatGuid(),
                Groups = new List<GroupInfoView>(),
                Questions = new List<QuestionInfoView>()
            };
        }
    }
}
