using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.AnswerToStringServiceTests
{
    internal class AnswerToStringServiceTestsContext
    {
        public static IAnswerToStringService CreateAnswerToStringService()
        {
            return new AnswerToStringService();
        }

        public static YesNoAnswer CreateYesNoAnswer(AnsweredYesNoOption[] answer)
        {
            var yesNoAnswer = new YesNoAnswer();
            yesNoAnswer.SetAnswers(answer);
            return yesNoAnswer;
        }

        public static YesNoQuestionModel CreateYesNoQuestionModel(IEnumerable<OptionModel> options)
        {
            var model = new YesNoQuestionModel();
            model.Options = options.ToList();
            return model;
        }

        public static SingleOptionAnswer CreateSingleOptionAnswer(Guid id, decimal answer)
        {
            var singleOptionAnswer = new SingleOptionAnswer(id, Create.RosterVector());
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

        public static LinkedSingleOptionQuestionModel CreateLinkedSingleOptionQuestionModel(Guid sourceOfLinkId)
        {
            var model = new LinkedSingleOptionQuestionModel();
            model.LinkedToQuestionId = sourceOfLinkId;
            return model;
        }

        public static LinkedSingleOptionAnswer CreateLinkedSingleOptionAnswer(decimal[] answer, Guid? id = null)
        {
            var model= new LinkedSingleOptionAnswer(id ?? Guid.NewGuid(),new decimal[0]);
            model.SetAnswer(answer);
            return model;
        }
    }
}