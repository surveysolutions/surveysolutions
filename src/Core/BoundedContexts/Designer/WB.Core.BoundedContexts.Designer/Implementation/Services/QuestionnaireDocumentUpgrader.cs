using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireDocumentUpgrader : IQuestionnaireDocumentUpgrader
    {
        private readonly IQuestionFactory _questionFactory;

        public QuestionnaireDocumentUpgrader(IQuestionFactory questionFactory)
        {
            _questionFactory = questionFactory;
        }

        public QuestionnaireDocument TranslatePropagatePropertiesToRosterProperties(QuestionnaireDocument originalDocument)
        {
            var document = originalDocument.Clone();

            var questions = document.Find<AbstractQuestion>(q => q.QuestionType == QuestionType.AutoPropagate).ToList();
            foreach (var question in questions)
            {
                var triggers = GetTriggersFromQuestion(question);
                int? maxValue = GetMaxValueFromQuestion(question);

                IQuestion newNumericQuestion = CreateNumericQuestion(question, maxValue);

                document.ReplaceQuestionWithNew(question, newNumericQuestion);
                document.ConnectChildrenWithParent();
                foreach (var groupId in triggers)
                {
                    FindGroupAndUpdateItToRoster(document, groupId, question.PublicKey);
                }
            }

            MarkAllNonReferencedAutoPropagatedGroupsAsNotPropagated(document);

            FindHeadQuestionsAndUpdateThemToRosterTitles(document);

            return document;
        }

        public QuestionnaireDocument CleanExpressionCaches(QuestionnaireDocument originalDocument)
        {
            var document = originalDocument.Clone();

            var allQuestions = document.Find<IQuestion>(_ => true).ToList();

            foreach (var question in allQuestions)
            {
                question.ConditionalDependentQuestions = null;
                question.ConditionalDependentGroups = null;
                question.QuestionsWhichCustomValidationDependsOnQuestion = null;
                question.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion = null;
                question.QuestionIdsInvolvedInCustomValidationOfQuestion = null;
                question.QuestionsInvolvedInCustomEnablementConditionOfQuestion = null;
                question.QuestionsInvolvedInCustomValidationOfQuestion = null;
            }

            return document;
        }

        private static void MarkAllNonReferencedAutoPropagatedGroupsAsNotPropagated(QuestionnaireDocument document)
        {
            var autoGroups = document.Find<Group>(q => q.Propagated != Propagate.None).ToList();
            foreach (var autoGroup in autoGroups)
            {
                document.UpdateGroup(autoGroup.PublicKey, g => { g.Propagated = Propagate.None; });
            }
        }

        private static void FindGroupAndUpdateItToRoster(QuestionnaireDocument document, Guid groupId, Guid rosterSizeQuestionId)
        {
            var @group = document.Find<Group>(groupId);
            if (@group != null)
            {
                document.UpdateGroup(
                    @group.PublicKey,
                    @group.Title,
                    @group.Description,
                    conditionExpression: @group.ConditionExpression);

                document.UpdateGroup(@group.PublicKey, g =>
                {
                    g.IsRoster = true;
                    g.RosterSizeQuestionId = rosterSizeQuestionId;
                    g.Propagated = Propagate.None;
                });
            }
        }

        private IQuestion CreateNumericQuestion(AbstractQuestion question, int? maxValue)
        {
            return this._questionFactory.CreateQuestion(new QuestionData(
                    question.PublicKey,
                    QuestionType.Numeric,
                    question.QuestionScope,
                    question.QuestionText,
                    question.StataExportCaption,
                    question.VariableLabel,
                    question.ConditionExpression,
                    question.ValidationExpression,
                    question.ValidationMessage,
                    question.AnswerOrder,
                    question.Featured,
                    question.Mandatory,
                    question.Capital,
                    question.Instructions,
                    triggers: new List<Guid>(),
                    maxValue: maxValue,
                    answers: null,
                    linkedToQuestionId: null,
                    isInteger: true,
                    countOfDecimalPlaces: null,
                    areAnswersOrdered: false,
                    maxAllowedAnswers: null,
                    maxAnswerCount: null));
        }

        private int? GetMaxValueFromQuestion(AbstractQuestion question)
        {
            var propagateQuestion = question as AutoPropagateQuestion;
            if (propagateQuestion != null)
            {
                return propagateQuestion.MaxValue;
            }
            var numeric = question as NumericQuestion;
            return numeric != null ? numeric.MaxValue : null;
        }

        private static IEnumerable<Guid> GetTriggersFromQuestion(AbstractQuestion question)
        {
            var propagateQuestion = question as AutoPropagateQuestion;
            return propagateQuestion != null
                ? propagateQuestion.Triggers.ToArray()
                : new Guid[0];
        }

        private static void FindHeadQuestionsAndUpdateThemToRosterTitles(QuestionnaireDocument document)
        {
            //assuming all groups were updated to roster
            var headQuestions = document.Find<AbstractQuestion>(q => q.Capital).ToList();

            if (!headQuestions.Any()) 
                return;

            document.ConnectChildrenWithParent();
            foreach (var headQuestion in headQuestions)
            {
                var parentGroup = headQuestion.GetParent() as Group;
                if (parentGroup != null && parentGroup.IsRoster)
                {
                    parentGroup.RosterTitleQuestionId = headQuestion.PublicKey;

                    if (parentGroup.RosterSizeQuestionId != null)
                    {
                        var rosterGroups =
                            document.Find<Group>(q => q.RosterSizeQuestionId == parentGroup.RosterSizeQuestionId).ToList();
                        foreach (var @group in rosterGroups)
                        {
                            document.UpdateGroup(@group.PublicKey, g =>
                            {
                                g.RosterTitleQuestionId = headQuestion.PublicKey;
                            });

                        }
                    }
                }
                headQuestion.Capital = false;
            }
        }
    }
}
