using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateNumericQuestionHandlerTests
{
    internal class when_cloning_numeric_question_with_variable_name_roster_title : QuestionnaireTestsContext
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
                StataExportCaption = "var"
            });
            eventContext = new EventContext();
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        questionnaire.CloneNumericQuestion(Guid.NewGuid(), chapterId, "title", rosterTitle,null, false, false,
                            QuestionScope.Interviewer, null, null, null, null,
                            responsibleId: responsibleId, isInteger: false, countOfDecimalPlaces: null,
                            maxValue: null, sourceQuestionId: questionId, targetIndex: 1));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__variable____rosterTitle__and__keyword__ = () =>
       new[] { "variable", rosterTitle, "keyword" }.ShouldEachConformTo(
           keyword => exception.Message.ToLower().Contains(keyword));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
        private static Exception exception;
        private const string rosterTitle = "rostertitle";
    }
}
