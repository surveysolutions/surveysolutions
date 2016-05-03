﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class AnswerToStringService : IAnswerToStringService
    {
        public string AnswerToUIString(Guid questionId, BaseInterviewAnswer answer, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            if (answer == null || !answer.IsAnswered)
                return string.Empty;

            if (answer is TextAnswer)
                return ((TextAnswer)answer).Answer;

            if (answer is IntegerNumericAnswer)
            {
                var integerNumericAnswer = (IntegerNumericAnswer)answer;
                var answerValue = (decimal?)integerNumericAnswer.Answer;
                return answerValue.FormatDecimal();
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
                return realNumericAnswer.Answer.FormatDecimal();
            }

            if (answer is MultiOptionAnswer)
            {
                var multiAnswer = (MultiOptionAnswer)answer;
                List<string> stringAnswers = new List<string>();
                foreach (var answerValue in multiAnswer.Answers)
                {
                    stringAnswers.Add(questionnaire.GetAnswerOptionTitle(questionId, answerValue));
                }
                
                return string.Join(", ", stringAnswers);
            }

            if (answer is YesNoAnswer)
            {
                var yesNoAnswer = (YesNoAnswer)answer;
                List<string> stringAnswers = new List<string>();
                var yesAnswers = yesNoAnswer.Answers.Where(s => s.Yes);
                foreach (var answerValue in yesAnswers)
                {
                    stringAnswers.Add(questionnaire.GetAnswerOptionTitle(questionId, answerValue.OptionValue));
                }

                return string.Join(", ", stringAnswers);
            }

            var singleOptionAnswer = answer as SingleOptionAnswer;
            if (singleOptionAnswer != null)
            {
                if (singleOptionAnswer.Answer.HasValue)
                {
                    return questionnaire.GetAnswerOptionTitle(questionId, singleOptionAnswer.Answer.Value);
                }
                return string.Empty;
            }

            if (answer is TextListAnswer)
            {
                return string.Join(", ", ((TextListAnswer)answer).Answers.Select(x => x.Item2));
            }

            var singleOptionLinkedAnswer = answer as LinkedSingleOptionAnswer;
            if (singleOptionLinkedAnswer != null)
            {
                if (questionnaire.IsQuestionLinked(questionId))
                {
                    var referencedQuestionIdentity = new Identity(singleOptionLinkedAnswer.Id, singleOptionLinkedAnswer.RosterVector);

                    var questionIdReferencedByLinkedQuestion = questionnaire.GetQuestionReferencedByLinkedQuestion(singleOptionLinkedAnswer.Id);
                    var referencedAnswer = interview.FindAnswersOfReferencedQuestionForLinkedQuestion(questionIdReferencedByLinkedQuestion, referencedQuestionIdentity)
                            .FirstOrDefault(a => a.RosterVector.SequenceEqual(singleOptionLinkedAnswer.Answer));

                    if (referencedAnswer != null)
                    {
                        return this.AnswerToUIString(questionIdReferencedByLinkedQuestion, referencedAnswer, interview, questionnaire);
                    }
                }
            }
        
            return answer.ToString();
        }
    }
}