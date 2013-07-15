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

        public static Questionnaire CreateQuestionnaire()
        {
            return new Questionnaire();
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestion(Guid questionId)
        {
            return CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId);
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestionnInTypeAndOptions(Guid questionId, QuestionType questionType, Option[] options)
        {
            return CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId, questionType: questionType, options: options);
        }

        private static Questionnaire CreateQuestionnaire(Guid? questionnaireId = null, string text = "text of questionnaire")
        {
            return new Questionnaire(questionnaireId ?? Guid.NewGuid(), text);
        }

        public static Questionnaire CreateQuestionnaireWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(questionKey);

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneGroup(Guid? questionnaireId = null, Guid? groupId = null, Propagate propagationKind = Propagate.None)
        {
            Questionnaire questionnaire = CreateQuestionnaire(questionnaireId ?? Guid.NewGuid(), "Title");

            questionnaire.NewAddGroup(groupId ?? Guid.NewGuid(), null, "New group", propagationKind, null, null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithOneAutoPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireWithOneGroup(groupId: groupId, propagationKind: Propagate.AutoPropagated);
        }

        public static Questionnaire CreateQuestionnaireWithOneNonPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireWithOneGroup(groupId: groupId, propagationKind: Propagate.None);
        }

        public static Questionnaire CreateQuestionnaireWithOneAutoGroupAndQuestionInIt(Guid questionId)
        {
            return CreateQuestionnaireWithOneGroupAndQuestionInIt(
                questionId: questionId, groupPropagationKind: Propagate.AutoPropagated);
        }

        public static Questionnaire CreateQuestionnaireWithOneGroupAndQuestionInIt(Guid questionId, Guid? groupId = null,
                                                                                       Propagate groupPropagationKind = Propagate.None, QuestionType questionType = QuestionType.Text, Option[] options = null)
        {
            groupId = groupId ?? Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(Guid.NewGuid(), groupId.Value, groupPropagationKind);

            questionnaire.NewAddQuestion(questionId,
                                         groupId.Value, "Title", questionType, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", options ?? new Option[] { }, Order.AsIs, null,
                                         new Guid[] { });

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoQuestions(Guid secondQuestionId)
        {
            var groupId = Guid.NewGuid();

            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(Guid.NewGuid(), groupId);

            questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            questionnaire.NewAddQuestion(secondQuestionId, groupId, "Title", QuestionType.Text, "name", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoGroups(Guid firstGroup, Guid secondGroup)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(firstGroup);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoGroupAndRegularGroup(Guid autoGroupPublicKey, Guid secondGroup)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(autoGroupPublicKey);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionInIt(Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestoinId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithAutoGroupAndRegularGroup(autoGroupPublicKey, secondGroup);

            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);
            return questionnaire;
        }

        public static Questionnaire CreateQuestionnaireWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId)
        {
            var secondGroup = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(firstGroup, secondGroup);
            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);
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

        public static Questionnaire CreateQuestionnaireWithRegularGroupAndRegularGroupInIt(Guid groupId)
        {
            Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(groupId);
            
            questionnaire.NewAddGroup(Guid.NewGuid(), groupId, "New group", Propagate.None, null, null);

            return questionnaire;
        }
    }
}