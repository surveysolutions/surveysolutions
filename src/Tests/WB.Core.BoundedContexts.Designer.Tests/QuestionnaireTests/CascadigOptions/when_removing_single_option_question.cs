using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests.CascadigOptions
{
    public class when_removing_single_option_question_used_as_cascading_parent : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rootGroupId = Guid.NewGuid();
            responsibleId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: rootGroupId);

            parentQuestionId = Guid.NewGuid();
            updatedQuestionId = Guid.NewGuid();

            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = parentQuestionId,
                QuestionType = QuestionType.SingleOption,
                Answers = new[]
                {
                    new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                }
            });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = updatedQuestionId,
                QuestionType = QuestionType.SingleOption,
                CascadeFromQuestionId = parentQuestionId,
                Answers = new[]
                {
                    new Answer { AnswerText = "one one", AnswerValue = "1.1", ParentValue = "1", PublicKey = Guid.NewGuid() },
                }
            });
        };

        private Because of = () => exception = Catch.Exception(() => questionnaire.NewDeleteQuestion(parentQuestionId, responsibleId));

        private It should_not_allow_removal = () =>
        {
            var ex = exception as QuestionnaireException;
            ex.ShouldNotBeNull();

            new [] { "remove", "cascading", "parent" }.ShouldEachConformTo(keyword => ex.Message.ToLower().Contains(keyword));
        };
        private static Guid rootGroupId;
        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Guid updatedQuestionId;
        private static Guid responsibleId;
        private static Exception exception;
    }
}

