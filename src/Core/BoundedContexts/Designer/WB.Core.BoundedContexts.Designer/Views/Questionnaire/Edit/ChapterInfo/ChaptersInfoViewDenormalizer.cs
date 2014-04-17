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
    internal class GroupInfoViewDenormalizer :
        AbstractFunctionalEventHandler<GroupInfoView>,
        ICreateHandler<GroupInfoView, NewQuestionnaireCreated>,
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

        public GroupInfoViewDenormalizer(IReadSideRepositoryWriter<GroupInfoView> writer, IExpressionProcessor expressionProcessor)
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
            var questionnaire = new GroupInfoView()
            {
                GroupId = evnt.EventSourceId.FormatGuid(),
                Groups = new List<GroupInfoView>()
            };

            return questionnaire;
        }

        public GroupInfoView Create(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var questionnaire = new GroupInfoView()
            {
                GroupId = evnt.EventSourceId.FormatGuid(),
                Groups = new List<GroupInfoView>()
            };

            return questionnaire;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<NewGroupAdded> evnt)
        {
            var groupInfoView = new GroupInfoView()
            {
                GroupId = evnt.Payload.PublicKey.FormatGuid(),
                Title = evnt.Payload.GroupText,
                Groups = new List<GroupInfoView>(),
                Questions = new List<QuestionInfoView>(),
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.GroupId)
            {
                currentState.Groups.Add(groupInfoView);
            }
            else
            {
                var existsGroup = this.FindGroup(questionnaireOrGroup: currentState,
                    groupId: evnt.Payload.ParentGroupPublicKey.Value.FormatGuid());

                existsGroup.Groups.Add(groupInfoView);
            }
            
            return currentState;
        }

        public GroupInfoView Update(GroupInfoView currentState, IPublishedEvent<GroupCloned> evnt)
        {
            var groupInfoView = new GroupInfoView()
            {
                GroupId = evnt.Payload.PublicKey.FormatGuid(),
                Title = evnt.Payload.GroupText,
                GroupsCount = 0,
                RostersCount = 0,
                QuestionsCount = 0
            };

            if (!evnt.Payload.ParentGroupPublicKey.HasValue ||
                evnt.Payload.ParentGroupPublicKey.Value.FormatGuid() == currentState.GroupId)
            {
                currentState.Groups.Add(groupInfoView);
            }
            else
            {
                var existsGroup = this.FindGroup(questionnaireOrGroup: currentState,
                    groupId: evnt.Payload.ParentGroupPublicKey.Value.FormatGuid());

                existsGroup.Groups.Add(groupInfoView);
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

            GroupInfoView groupInfoView = this.FindGroup(currentState, groupOrQuestionKey);
            if (groupInfoView != null)
            {
                
            }

            QuestionInfoView questionInfoView = this.FindQuestion(currentState, groupOrQuestionKey);
            if (questionInfoView != null)
            {

            }

            return currentState;
        }

        private GroupInfoView FindGroup(GroupInfoView questionnaireOrGroup, string groupId)
        {
            GroupInfoView findedGroup = null;

            foreach (var groupInfoView in questionnaireOrGroup.Groups)
            {
                findedGroup = questionnaireOrGroup.GroupId == groupId ? groupInfoView : this.FindGroup(groupInfoView, groupId);
            }

            return findedGroup;
        }

        private GroupInfoView FindParentOfGroup(GroupInfoView questionnaireOrGroup, string groupId)
        {
            GroupInfoView findedGroup = null;

            foreach (var groupInfoView in questionnaireOrGroup.Groups)
            {
                findedGroup = questionnaireOrGroup.Groups.Any(group => group.GroupId == groupId)
                    ? groupInfoView
                    : this.FindParentOfGroup(groupInfoView, groupId);
            }

            return findedGroup;
        }

        private QuestionInfoView FindQuestion(GroupInfoView questionnaireOrGroup, string questionId)
        {
            QuestionInfoView findedQuestion = null;

            foreach (var groupInfoView in questionnaireOrGroup.Groups)
            {
                findedQuestion = questionnaireOrGroup.Questions.Any(question => question.QuestionId == questionId)
                    ? questionnaireOrGroup.Questions.Find(question => question.QuestionId == questionId)
                    : this.FindQuestion(groupInfoView, questionId);
            }

            return findedQuestion;
        }

        private GroupInfoView FindParentOfQuestion(GroupInfoView questionnaireOrGroup, string questionId)
        {
            GroupInfoView findedGroup = null;

            foreach (var groupInfoView in questionnaireOrGroup.Groups)
            {
                findedGroup = questionnaireOrGroup.Questions.Any(question => question.QuestionId == questionId)
                    ? groupInfoView
                    : this.FindParentOfQuestion(groupInfoView, questionId);
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

            questionView.QuestionId = questionId;
            questionView.Title = questionTitle;
            questionView.Type = questionType;
            questionView.Variable = questionVariable;
            questionView.LinkedVariables = questionsUsedInConditionExpression;
        }

        
    }
}
