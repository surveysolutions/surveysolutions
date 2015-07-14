using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.AnswerToStringServiceTests
{
    public class AnswerToStringServiceTestsContext
    {
        public static IAnswerToStringService CreateAnswerToStringService()
        {
            return new AnswerToStringService();
        }

        public static SingleOptionAnswer CreateSingleOptionAnswer(decimal answer)
        {
            var singleOptionAnswer = new SingleOptionAnswer();
            singleOptionAnswer.SetAnswer(answer);
            return singleOptionAnswer;
        }

        public static FilteredSingleOptionQuestionModel CreateFilteredSingleOptionQuestionModel(IEnumerable<OptionModel> options )
        {
            var model = new FilteredSingleOptionQuestionModel();
            model.Options = options.ToList();
            return model;
        }

        public static SingleOptionQuestionModel CreateSingleOptionQuestionModel(IEnumerable<OptionModel> options)
        {
            var model = new SingleOptionQuestionModel();
            model.Options = options.ToList();
            return model;
        }
    }
}