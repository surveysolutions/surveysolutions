using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    public static class QuestionnaireARUtils {

        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireAR()
        {
            return new Core.Domain.QuestionnaireAR();
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneQuestion(Guid questionId)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionId);
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(Guid questionId, QuestionType questionType, Option[] options)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionId, questionType: questionType, options: options);
        }

        private static Core.Domain.QuestionnaireAR CreateQuestionnaireAR(Guid? questionnaireId = null, string text = "text of questionnaire")
        {
            return new Core.Domain.QuestionnaireAR(questionnaireId ?? Guid.NewGuid(), text);
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey)
        {
            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionKey);

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneGroup(Guid? questionnaireId = null, Guid? groupId = null, Propagate propagationKind = Propagate.None)
        {
            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireAR(questionnaireId ?? Guid.NewGuid(), "Title");

            questionnaire.NewAddGroup(groupId ?? Guid.NewGuid(), null, "New group", propagationKind, null, null);

            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneAutoPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireARWithOneGroup(groupId: groupId, propagationKind: Propagate.AutoPropagated);
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneNonPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireARWithOneGroup(groupId: groupId, propagationKind: Propagate.None);
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(Guid questionId)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(
                questionId: questionId, groupPropagationKind: Propagate.AutoPropagated);
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid questionId, Guid? groupId = null,
                                                                                       Propagate groupPropagationKind = Propagate.None, QuestionType questionType = QuestionType.Text, Option[] options = null)
        {
            groupId = groupId ?? Guid.NewGuid();

            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId.Value, groupPropagationKind);

            questionnaire.NewAddQuestion(questionId,
                                         groupId.Value, "Title", questionType, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", options ?? new Option[] { }, Order.AsIs, null,
                                         new Guid[] { });

            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithTwoQuestions(Guid secondQuestionId)
        {
            var groupId = Guid.NewGuid();

            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId);

            questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            questionnaire.NewAddQuestion(secondQuestionId, groupId, "Title", QuestionType.Text, "name", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithTwoGroups(Guid firstGroup, Guid secondGroup)
        {
            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneNonPropagatedGroup(firstGroup);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithAutoGroupAndRegularGroup(Guid autoGroupPublicKey, Guid secondGroup)
        {
            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupPublicKey);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithAutoGroupAndRegularGroupAndQuestionInIt(Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestoinId)
        {
            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithAutoGroupAndRegularGroup(autoGroupPublicKey, secondGroup);

            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);
            return questionnaire;
        }

        public static Core.Domain.QuestionnaireAR CreateQuestionnaireARWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId)
        {
            var secondGroup = Guid.NewGuid();
            Core.Domain.QuestionnaireAR questionnaire = CreateQuestionnaireARWithTwoGroups(firstGroup, secondGroup);
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
    }
}