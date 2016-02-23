using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.AnswerToStringServiceTests
{
    internal class when_passing_filtered_single_option_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            answerToStringService = CreateAnswerToStringService();
            singleOptionAnswer = CreateSingleOptionAnswer(questionId, 3);
            questionnaireMock = Mock.Of<IQuestionnaire>(_  => _.GetAnswerOptionTitle(questionId, 3) == "3");
        };

        Because of = () =>
            result = answerToStringService.AnswerToUIString(questionId, singleOptionAnswer,null, questionnaireMock);

        It should_return_3 = () =>
            result.ShouldEqual("3");

        static string result;
        static Guid questionId = Id.g1;
        static IQuestionnaire questionnaireMock;
        static SingleOptionAnswer singleOptionAnswer;
        static IAnswerToStringService answerToStringService;
    }
}