using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateNumericQuestionHandlerTests
{
    internal class when_updating_numeric_question_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NumericQuestionAdded()
            {
                GroupPublicKey = chapterId,
                PublicKey = questionId,
                StataExportCaption = "var",
                QuestionText = "title"
            });
            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.UpdateNumericQuestion(questionId, questionTitle, "var",null, false, false, QuestionScope.Interviewer, null, null, null, null,
                responsibleId: responsibleId, isInteger: false, countOfDecimalPlaces: null));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__unknown__and__substitution__ = () =>
            new[] { "unknown", "substitution" }.ShouldEachConformTo(
           keyword => exception.Message.ToLower().Contains(keyword));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
        private static string questionTitle = "title %rostertitle%";
        private static Exception exception;
    }
}
