using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Services.MaskText;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.AnswerToStringServiceTests
{
    public class when_passing_single_option_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            answerToStringService = CreateAnswerToStringService();
            singleOptionAnswer = CreateSingleOptionAnswer(3);
            singleOptionQuestionModel = CreateSingleOptionQuestionModel(
                new[]
                {
                    new OptionModel() { Title = "1", Value = 1 },
                    new OptionModel() { Title = "2", Value = 2 },
                    new OptionModel() { Title = "3", Value = 3 },
                    new OptionModel() { Title = "4", Value = 4 },
                });
        };

        Because of = () =>
            result = answerToStringService.AnswerToUIString(singleOptionQuestionModel, singleOptionAnswer);

        It should_return_3 = () =>
            result.ShouldEqual("3");


        static string result;
        static SingleOptionAnswer singleOptionAnswer;
        static SingleOptionQuestionModel singleOptionQuestionModel;
        static IAnswerToStringService answerToStringService;
    }
}