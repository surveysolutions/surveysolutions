using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class AnswerToStringService : IAnswerToStringService
    {
        public string AnswerToUIString(BaseQuestionModel question, BaseInterviewAnswer answer)
        {
            if (answer == null || !answer.IsAnswered)
                return string.Empty;

            if (answer is TextAnswer)
                return ((TextAnswer)answer).Answer;

            if (answer is IntegerNumericAnswer)
            {
                var integerNumericAnswer = ((IntegerNumericAnswer)answer);
                return Monads.Maybe(() => integerNumericAnswer.Answer.Value.ToString(CultureInfo.InvariantCulture)) ?? String.Empty;
            }

            if (answer is DateTimeAnswer)
            {
                var dateTimeAnswer = (DateTimeAnswer)answer;
                if (dateTimeAnswer.Answer.HasValue)
                    return dateTimeAnswer.Answer.Value.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
                return string.Empty;
            }

            if (answer is RealNumericAnswer)
            {
                var realNumericAnswer = (RealNumericAnswer)answer;
                return Monads.Maybe(() => realNumericAnswer.Answer.Value.ToString(CultureInfo.InvariantCulture)) ?? String.Empty;
            }

            if (answer is MultiOptionAnswer)
            {
                var multiAnswer = (MultiOptionAnswer)answer;
                List<string> stringAnswers = new List<string>();
                var multiOptionQuestionModel = (MultiOptionQuestionModel) question;
                foreach (var answerValue in multiAnswer.Answers)
                {
                    stringAnswers.Add(multiOptionQuestionModel.Options.Single(x => x.Value == answerValue).Title);
                }
                
                return string.Join(", ", stringAnswers);
            }

            var singleOptionAnswer = answer as SingleOptionAnswer;
            if (singleOptionAnswer != null)
            {
                var singleOptionQuestion = question as SingleOptionQuestionModel;
                if (singleOptionQuestion != null)
                    return singleOptionQuestion.Options.Single(x => x.Value == singleOptionAnswer.Answer).Title;

                var filteredSingleOptionQuestion = question as FilteredSingleOptionQuestionModel;
                if (filteredSingleOptionQuestion != null)
                    return filteredSingleOptionQuestion.Options.Single(x => x.Value == singleOptionAnswer.Answer).Title;

                var cascadingSingleOptionQuestion = question as CascadingSingleOptionQuestionModel;
                if (cascadingSingleOptionQuestion != null)
                    return cascadingSingleOptionQuestion.Options.Single(x => x.Value == singleOptionAnswer.Answer).Title;
            }

            if (answer is TextListAnswer)
            {
                return string.Join(", ", ((TextListAnswer)answer).Answers.Select(x => x.Item2));
            }

            return answer.ToString();
        }
    }
}