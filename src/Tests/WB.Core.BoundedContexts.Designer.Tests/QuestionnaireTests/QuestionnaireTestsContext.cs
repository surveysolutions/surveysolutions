using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    public class QuestionnaireTestsContext
    {
        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        public static T GetLastEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Last(e => e.Payload is T).Payload;
        }

        public static Questionnaire CreateQuestionnaire(Guid responsibleId)
        {
            return new Questionnaire(publicKey: Guid.NewGuid(), title: "title", createdBy: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestion(Guid questionId, Guid responsibleId)
        {
            return CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: questionId, responsibleId: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestionInTypeAndOptions(Guid questionId, QuestionType questionType, Option[] options, Guid responsibleId, Guid? groupId = null)
        {
            groupId = groupId ?? Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: Guid.NewGuid(),
                groupId: groupId.Value, groupPropagationKind: Propagate.None, responsibleId: responsibleId);

            AddQuestion(questionnaire, questionId, groupId.Value, responsibleId, questionType, "text1", options);

            return questionnaire;
            
        }

        private static Questionnaire CreateQuestionnaire(Guid responsibleId, Guid? questionnaireId = null, string text = "text of questionnaire")
        {
            return new Questionnaire(publicKey: questionnaireId ?? Guid.NewGuid(), title: text, createdBy: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: questionKey,
                responsibleId: responsibleId);

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneGroup(Guid responsibleId, Guid? questionnaireId = null, Guid? groupId = null, Propagate propagationKind = Propagate.None)
        {
            Questionnaire questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId ?? Guid.NewGuid(), text: "Title", responsibleId: responsibleId);

            questionnaire.NewAddGroup(groupId ?? Guid.NewGuid(), null, "New group", propagationKind, null, null,
                responsibleId: responsibleId);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneAutoPropagatedGroup(Guid groupId, Guid responsibleId)
        {
            return CreateQuestionnaireWithOneGroup(groupId: groupId, propagationKind: Propagate.AutoPropagated,
                responsibleId: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneNonPropagatedGroup(Guid groupId, Guid responsibleId)
        {
            return CreateQuestionnaireWithOneGroup(groupId: groupId, propagationKind: Propagate.None,
                responsibleId: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(Guid questionId, Guid responsibleId)
        {
            return CreateQuestionnaireWithOneGroupAndQuestionInIt(
                questionId: questionId, groupPropagationKind: Propagate.AutoPropagated, responsibleId: responsibleId);
        }

        public static Questionnaire CreateQuestionnaireWithOneGroupAndQuestionInIt(Guid questionId, Guid responsibleId, Guid? groupId = null, 
                                                                                   Propagate groupPropagationKind = Propagate.None, QuestionType questionType = QuestionType.Text, 
                                                                                   bool? isInteger = null, string alias = "text")
        {
            groupId = groupId ?? Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, Guid.NewGuid(), groupId.Value,
                groupPropagationKind);
            
            AddQuestion(questionnaire, questionId, groupId.Value, responsibleId, questionType, alias,
                                               (questionType == QuestionType.MultyOption || questionType == QuestionType.SingleOption)?
                                                  new Option[2]{new Option(Guid.NewGuid(),"1", "1"),new Option(Guid.NewGuid(), "2", "2") }:
                                                  new Option[0]);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoQuestions(Guid secondQuestionId, Guid responsibleId)
        {
            var groupId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupId, responsibleId: responsibleId);

            questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, responsibleId: responsibleId, linkedToQuestionId: null);

            questionnaire.NewAddQuestion(secondQuestionId, groupId, "Title", QuestionType.Text, "name", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, responsibleId: responsibleId, linkedToQuestionId: null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoGroups(Guid firstGroup, Guid secondGroup, Guid responsibleId, Propagate propagationKind = Propagate.None)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: firstGroup,
                responsibleId: responsibleId, propagationKind: propagationKind);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", propagationKind, null, null,
                responsibleId: responsibleId);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoGroupAndRegularGroup(Guid autoGroupPublicKey, Guid secondGroup, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: autoGroupPublicKey,
                responsibleId: responsibleId);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null, responsibleId);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionInIt(Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestoinId, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithAutoGroupAndRegularGroup(autoGroupPublicKey, secondGroup, responsibleId);
            questionnaire.AddNumericQuestion(autoQuestoinId, secondGroup, "Title", true, "auto", false, false,
                false, QuestionScope.Interviewer, "", "", "", "", null, new Guid[] { autoGroupPublicKey }, responsibleId, true, null);
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
            Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestionId, Guid questionId, Guid responsibleId,
            QuestionType questionType, QuestionType autoQuestionType = QuestionType.Text)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithAutoGroupAndRegularGroup(autoGroupPublicKey,
                                                                                          secondGroup, responsibleId);

            questionnaire.OnNewQuestionAdded(new NewQuestionAdded()
                {
                    PublicKey = autoQuestionId,
                    GroupPublicKey = autoGroupPublicKey,
                    QuestionText = "Title",
                    QuestionType = autoQuestionType,
                    StataExportCaption = "auto",
                    Mandatory = false,
                    Featured = false,
                    Capital = false,
                    QuestionScope = QuestionScope.Interviewer,
                    ConditionExpression = string.Empty,
                    ValidationExpression = string.Empty,
                    ValidationMessage = string.Empty,
                    Instructions = string.Empty,
                    Answers = null,
                    AnswerOrder = Order.AsIs,
                    MaxValue = 0,
                    Triggers = new List<Guid>(),
                    ResponsibleId = responsibleId,
                    LinkedToQuestionId = null
                });
            AddQuestion(questionnaire, questionId, secondGroup, responsibleId, questionType, "manual",
                new[] { new Option(Guid.NewGuid(), "1", "title") });
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoAndRegularGroupsAnd1QuestionInAutoGroupAnd2QuestionsInRegular(
            Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestionId, Guid questionId, Guid responsibleId,
            QuestionType questionType, Guid questionThatLinkedButNotFromPropagateGroup, QuestionType autoQuestionType = QuestionType.Text)
        {
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupPublicKey,
                    secondGroup: secondGroup, autoQuestionId: autoQuestionId, questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType, autoQuestionType: autoQuestionType);

            questionnaire.NewAddQuestion(questionThatLinkedButNotFromPropagateGroup, secondGroup, "Title",
                                         autoQuestionType, "manual2", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", null, Order.AsIs, responsibleId, null);

            return questionnaire;
        }


        public static Questionnaire CreateQuestionnaireWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId, Guid responsibleId)
        {
            var secondGroup = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(firstGroup: firstGroup,
                secondGroup: secondGroup, responsibleId: responsibleId);
            AddQuestion(questionnaire, autoQuestoinId, secondGroup, responsibleId, QuestionType.AutoPropagate,"auto");
            return questionnaire;
        }

        public static void AddQuestion(Questionnaire questionnaire, Guid questionId, Guid groupId, Guid responsible,
            QuestionType questionType, string alias, Option[] options = null)
        {
            if (IsNumericQuestion(questionType))
            {
                questionnaire.AddNumericQuestion(questionId, groupId, "Title", questionType == QuestionType.AutoPropagate, alias, false,
                    false,
                    false, QuestionScope.Interviewer, "", "", "", "", null, new Guid[0], responsible, true, null);
                return;
            }
            questionnaire.NewAddQuestion(questionId, groupId, "Title", questionType, alias, false,
                false,
                false, QuestionScope.Interviewer, "", "", "", "", AreOptionsRequiredByQuestionType(questionType) ? options : null,
                Order.AsIs, responsible, null);
        }


        public static bool AreOptionsRequiredByQuestionType(QuestionType type)
        {
            return type == QuestionType.MultyOption || type == QuestionType.SingleOption;
        }

        public static bool IsNumericQuestion(QuestionType type)
        {
            return type == QuestionType.Numeric || type == QuestionType.AutoPropagate;
        }

        public static Option[] CreateTwoOptions()
        {
            return new[]
                {
                    new Option(Guid.Parse("00000000-1111-0000-1111-000000000000"), "-1", "No"),
                    new Option(Guid.Parse("00000000-2222-0000-2222-000000000000"), "42", "Yes"),
                };
        }

        public static Questionnaire CreateQuestionnaireWithRegularGroupAndRegularGroupInIt(Guid groupId, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(groupId: groupId,
                responsibleId: responsibleId);

            questionnaire.NewAddGroup(Guid.NewGuid(), groupId, "New group", Propagate.None, null, null,
                responsibleId: responsibleId);

            return questionnaire;
        }
    }
}