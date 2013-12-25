using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_integer_question_with_14400_and_max_value_is_3600 : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.HasQuestion(questionId) == true
                && _.GetQuestionType(questionId) == QuestionType.Numeric
                && _.IsQuestionInteger(questionId) == true
                && _.GetMaxValueForNumericQuestion(questionId) == 3600);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                interview.AnswerNumericIntegerQuestion(userId, questionId, new decimal[] { }, DateTime.Now, 14400));

        It should_throw_InterviewException = () =>
            exception.ShouldBeOfType<InterviewException>();

        It should_throw_exception_with_message_containting__greater__ = () =>
            exception.Message.ToLower().ShouldContain("greater");

        It should_throw_exception_with_message_containting__max_value__ = () =>
            exception.Message.ToLower().ShouldContain("max value");

        It should_throw_exception_with_message_containting__3600__ = () =>
            exception.Message.ToLower().ShouldContain("3600");

        private static Exception exception;
        private static Guid questionId;
        private static Interview interview;
        private static Guid userId;
    }
}