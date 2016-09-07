using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CascadigOptions
{
    internal class when_removing_single_option_question_used_as_cascading_parent : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rootGroupId = Guid.NewGuid();
            responsibleId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: rootGroupId);

            parentQuestionId = Guid.NewGuid();
            updatedQuestionId = Guid.NewGuid();

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : parentQuestionId,
                groupPublicKey: rootGroupId,
                questionType : QuestionType.SingleOption,
                answers : new[]
                {
                    new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                }
            ));
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
                (
                publicKey : updatedQuestionId,
                groupPublicKey: rootGroupId,
                questionType : QuestionType.SingleOption,
                cascadeFromQuestionId : parentQuestionId,
                answers : new[]
                {
                    new Answer { AnswerText = "one one", AnswerValue = "1.1", ParentValue = "1", PublicKey = Guid.NewGuid() },
                }
            ));
        };

        private Because of = () => exception = Catch.Exception(() => questionnaire.DeleteQuestion(parentQuestionId, responsibleId));

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

