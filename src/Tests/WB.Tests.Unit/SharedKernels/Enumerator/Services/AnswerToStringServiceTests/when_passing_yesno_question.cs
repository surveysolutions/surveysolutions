using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;

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
            yesNoQuestionModel = CreateYesNoQuestionModel(
                new[]
                {
                    new OptionModel() { Title = "1", Value = 1 },
                    new OptionModel() { Title = "2", Value = 2 },
                    new OptionModel() { Title = "3", Value = 3 },
                    new OptionModel() { Title = "4", Value = 4 },
                    new OptionModel() { Title = "5", Value = 5 },
                });
        };

        Because of = () =>
            result = answerToStringService.AnswerToUIString(yesNoQuestionModel, yesNoAnswer, null, null);

        It should_return_3 = () =>
            result.ShouldEqual("5, 2");


        static string result;
        static YesNoAnswer yesNoAnswer;
        static YesNoQuestionModel yesNoQuestionModel;
        static IAnswerToStringService answerToStringService;
    }
}