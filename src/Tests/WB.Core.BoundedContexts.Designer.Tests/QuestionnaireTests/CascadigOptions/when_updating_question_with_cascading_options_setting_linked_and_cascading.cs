using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_with_cascading_options_setting_linked_and_cascading : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rootGroupId = Guid.NewGuid();
            actorId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.NewGuid();
            updatedQuestionId = Guid.NewGuid();

            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = parentQuestionId,
                QuestionType = QuestionType.SingleOption,
                Answers = new Answer[] { new Answer
                {
                    AnswerText = "one", 
                    AnswerValue = "1", 
                    PublicKey = Guid.NewGuid()
                } }
            });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = updatedQuestionId,
                QuestionType = QuestionType.SingleOption,
            });
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.UpdateSingleOptionQuestion(
            updatedQuestionId,
            "title",
            "var",
            null,
            false,
            false,
            QuestionScope.Interviewer,
            null,
            null,
            null,
            null,
            actorId,
            new Option[]{}, 
            parentQuestionId,
            false,
            cascadeFromQuestionId: parentQuestionId
            ));

        It should_not_allow_to_set_both_linked_and_cascading_qestion_at_the_same_time = () =>
        {
            var ex = exception as QuestionnaireException;
            ex.ShouldNotBeNull();

            new []{"cascading", "linked", "same", "time"}.ShouldEachConformTo(keyword => ex.Message.ToLower().Contains(keyword));
        };


        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Exception exception;
        private static Guid rootGroupId;
        private static Guid updatedQuestionId;
        private static Guid actorId;
    }
}

