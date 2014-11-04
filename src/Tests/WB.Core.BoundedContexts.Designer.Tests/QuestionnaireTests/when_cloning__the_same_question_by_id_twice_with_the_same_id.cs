using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning__the_same_question_by_id_twice_with_the_same_id : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            var newQuestionAdded = new NewQuestionAdded
            {
                PublicKey = sourceQuestionId, 
                QuestionText = "text",
                VariableLabel = "varlabel",
                QuestionType = QuestionType.Text,
                Mandatory = true,
                Featured = true,
                QuestionScope = QuestionScope.Interviewer,
                ConditionExpression = "Conditional",
                ValidationExpression = "Validation",
                ValidationMessage = "Val message",
                Instructions = "Intructions",
                LinkedToQuestionId = Guid.NewGuid(),
                IsFilteredCombobox = true,
                CascadeFromQuestionId = Guid.NewGuid(),
                Mask = "(###)-##-##-###"
            };

            questionnaire.Apply(newQuestionAdded);

            eventContext = new EventContext();

            questionnaire.CloneQuestionById(sourceQuestionId, responsibleId, questionId);
        };
        
        Because of = () =>
            exception = Catch.Exception(() => questionnaire.CloneQuestionById(sourceQuestionId, responsibleId, questionId)
        );

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containing_specific_questionId = () =>
            exception.Message.ShouldContain(questionId.ToString());

        It should_throw_exception_with_message_containing__already____exist__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("already", "exist");

        static Questionnaire questionnaire;
        static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid sourceQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static EventContext eventContext;
        private static Exception exception;
    }
}