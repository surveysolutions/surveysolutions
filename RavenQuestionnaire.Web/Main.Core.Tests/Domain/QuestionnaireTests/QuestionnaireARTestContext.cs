using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using Main.Core.Domain;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    public class QuestionnaireARTestContext {

        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        public static QuestionnaireAR CreateQuestionnaireAR()
        {
            return new QuestionnaireAR();
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneQuestion(Guid questionId)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionId);
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneQuestionnInTypeAndOptions(Guid questionId, QuestionType questionType, Option[] options)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionId, questionType: questionType, options: options);
        }

        private static QuestionnaireAR CreateQuestionnaireAR(Guid? questionnaireId = null, string text = "text of questionnaire")
        {
            return new QuestionnaireAR(questionnaireId ?? Guid.NewGuid(), text);
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneQuestionAndOneImage(Guid questionKey, Guid imageKey)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroupAndQuestionInIt(questionKey);

            questionnaire.UploadImage(questionKey, "image title", "image description", imageKey);

            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneGroup(Guid? questionnaireId = null, Guid? groupId = null, Propagate propagationKind = Propagate.None)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireAR(questionnaireId ?? Guid.NewGuid(), "Title");

            questionnaire.NewAddGroup(groupId ?? Guid.NewGuid(), null, "New group", propagationKind, null, null);

            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneAutoPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireARWithOneGroup(groupId: groupId, propagationKind: Propagate.AutoPropagated);
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneNonPropagatedGroup(Guid groupId)
        {
            return CreateQuestionnaireARWithOneGroup(groupId: groupId, propagationKind: Propagate.None);
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneAutoGroupAndQuestionInIt(Guid questionId)
        {
            return CreateQuestionnaireARWithOneGroupAndQuestionInIt(
                questionId: questionId, groupPropagationKind: Propagate.AutoPropagated);
        }

        public static QuestionnaireAR CreateQuestionnaireARWithOneGroupAndQuestionInIt(Guid questionId, Guid? groupId = null,
                                                                                       Propagate groupPropagationKind = Propagate.None, QuestionType questionType = QuestionType.Text, Option[] options = null)
        {
            groupId = groupId ?? Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId.Value, groupPropagationKind);

            questionnaire.NewAddQuestion(questionId,
                                         groupId.Value, "Title", questionType, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", options ?? new Option[] { }, Order.AsIs, null,
                                         new Guid[] { });

            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithTwoQuestions(Guid secondQuestionId)
        {
            var groupId = Guid.NewGuid();

            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneGroup(Guid.NewGuid(), groupId);

            questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Title", QuestionType.Text, "text", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            questionnaire.NewAddQuestion(secondQuestionId, groupId, "Title", QuestionType.Text, "name", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);

            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithTwoGroups(Guid firstGroup, Guid secondGroup)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneNonPropagatedGroup(firstGroup);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithAutoGroupAndRegularGroup(Guid autoGroupPublicKey, Guid secondGroup)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneAutoPropagatedGroup(autoGroupPublicKey);

            questionnaire.NewAddGroup(secondGroup, null, "Second group", Propagate.None, null, null);

            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithAutoGroupAndRegularGroupAndQuestionInIt(Guid autoGroupPublicKey, Guid secondGroup, Guid autoQuestoinId)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithAutoGroupAndRegularGroup(autoGroupPublicKey, secondGroup);

            questionnaire.NewAddQuestion(autoQuestoinId, secondGroup, "Title", QuestionType.AutoPropagate, "auto", false, false,
                                         false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0,
                                         new Guid[0]);
            return questionnaire;
        }

        public static QuestionnaireAR CreateQuestionnaireARWithTwoRegularGroupsAndQuestionInLast(Guid firstGroup, Guid autoQuestoinId)
        {
            var secondGroup = Guid.NewGuid();
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithTwoGroups(firstGroup, secondGroup);
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

        public static QuestionnaireAR CreateQuestionnaireARWithRegularGroupAndRegularGroupInIt(Guid groupId)
        {
            QuestionnaireAR questionnaire = CreateQuestionnaireARWithOneNonPropagatedGroup(groupId);
            
            questionnaire.NewAddGroup(Guid.NewGuid(), groupId, "New group", Propagate.None, null, null);

            return questionnaire;
        }
    }
}