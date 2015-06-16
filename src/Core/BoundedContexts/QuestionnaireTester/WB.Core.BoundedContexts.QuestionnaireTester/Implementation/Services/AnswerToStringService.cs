using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class AnswerToStringService : IAnswerToStringService
    {
        public string AnswerToString(BaseQuestionModel question, BaseInterviewAnswer answer)
        {
            if (answer == null)
                return string.Empty;

            if (answer is TextAnswer)
                return ((TextAnswer)answer).Answer;

            if (answer is IntegerNumericAnswer)
            {
                var integerNumericAnswer = ((IntegerNumericAnswer)answer);
                return Monads.Maybe(() => integerNumericAnswer.Answer.Value.ToString(CultureInfo.InvariantCulture)) ?? String.Empty;
            }

            if (answer is DateTimeAnswer)
                return ((DateTimeAnswer)answer).Answer.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);

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

            if (answer is SingleOptionAnswer)
            {
                var singleOptionAnswer = (SingleOptionAnswer) answer;
                var singleOptionQuestion = (SingleOptionQuestionModel) question;

                return singleOptionQuestion.Options.Single(x => x.Value == singleOptionAnswer.Answer).Title;
            }

            if (answer is TextListAnswer)
            {
                return string.Join(", ", ((TextListAnswer)answer).Answers.Select(x => x.Item2));
            }

            return answer.ToString();
        }
    }
}