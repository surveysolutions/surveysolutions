using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
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

        public static Questionnaire CreateQuestionnaireWithOneQuestionnInTypeAndOptions(Guid questionId, QuestionType questionType, Option[] options, Guid responsibleId, Guid? groupId = null)
        {
            groupId = groupId ?? Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: Guid.NewGuid(),
                groupId: groupId.Value, groupPropagationKind: Propagate.None, responsibleId: responsibleId);

            questionnaire.NewAddQuestion(questionId,
                                         groupId.Value, "Title", questionType, "text1", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "",
                                         options, Order.AsIs, null,
                                         new Guid[] { }, responsibleId);

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
                                                                                       Propagate groupPropagationKind = Propagate.None, QuestionType questionType = QuestionType.Text)
        {
            groupId = groupId ?? Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, Guid.NewGuid(), groupId.Value,
                groupPropagationKind);

            questionnaire.NewAddQuestion(questionId,
                                         groupId.Value, "Title", questionType, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", 
                                         new Option[0], Order.AsIs, null,
                                         new Guid[] { }, responsibleId: responsibleId);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoQuestions(Guid secondQuestionId, Guid responsibleId)
        {
            var groupId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupId, responsibleId: responsibleId);

            questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0], responsibleId: responsibleId);

            questionnaire.NewAddQuestion(secondQuestionId, groupId, "Title", QuestionType.Text, "name", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0], responsibleId:responsibleId);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoGroups(Guid firstGroup, Guid secondGroup, Guid responsibleId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(groupId: firstGroup,
                responsibleId: responsibleId);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null,
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

            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0], responsibleId);
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId, Guid responsibleId)
        {
            var secondGroup = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(firstGroup: firstGroup,
                secondGroup: secondGroup, responsibleId: responsibleId);
            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false,
                false,
                false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                new Guid[0], responsibleId: responsibleId);
            return questionnaire;
        }

        public static bool AreOptionsRequiredByQuestionType(QuestionType type)
        {
            return type == QuestionType.MultyOption || type == QuestionType.SingleOption;
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