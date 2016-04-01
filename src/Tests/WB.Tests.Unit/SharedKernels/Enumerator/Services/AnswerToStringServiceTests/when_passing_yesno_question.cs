using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.AnswerToStringServiceTests
{
    internal class when_passing_yesno_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            answerToStringService = CreateAnswerToStringService();
            yesNoAnswer = CreateYesNoAnswer(new []
            {
                new AnsweredYesNoOption(5, true), 
                new AnsweredYesNoOption(3, false), 
                new AnsweredYesNoOption(2, true), 
            });
           
            questionnaireMock = Mock.Of<IQuestionnaire>(_ 
                => _.GetAnswerOptionTitle(questionId, 5) == "5"
                && _.GetAnswerOptionTitle(questionId, 2) == "2");
        };

        Because of = () =>
            result = answerToStringService.AnswerToUIString(questionId, yesNoAnswer, null, questionnaireMock);

        It should_return_3 = () =>
            result.ShouldEqual("5, 2");


        static string result;
        static Guid questionId = Id.g1;
        static IQuestionnaire questionnaireMock;
        static YesNoAnswer yesNoAnswer;
        static IAnswerToStringService answerToStringService;
    }
}