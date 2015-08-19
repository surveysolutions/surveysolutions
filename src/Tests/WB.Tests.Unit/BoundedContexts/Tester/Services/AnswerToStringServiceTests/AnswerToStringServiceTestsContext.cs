using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.AnswerToStringServiceTests
{
    internal class AnswerToStringServiceTestsContext
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