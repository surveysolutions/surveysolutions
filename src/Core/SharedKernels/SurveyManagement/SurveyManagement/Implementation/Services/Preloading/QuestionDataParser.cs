using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class QuestionDataParser : IQuestionDataParser
    {
        public KeyValuePair<Guid, object>? Parse(string answer, string variableName, Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            if (string.IsNullOrEmpty(answer))
                return null;
            var question = getQuestionByStataCaption(variableName);
            if (question == null)
                return null;
            if (question.LinkedToQuestionId.HasValue)
                return null;

            switch (question.QuestionType)
            {
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                    return new KeyValuePair<Guid, object>(question.PublicKey, answer);
                case QuestionType.GpsCoordinates:
                    var parsedAnswer = GeoPosition.Parse(answer);
                    if(parsedAnswer==null)
                        break;
                    return new KeyValuePair<Guid, object>(question.PublicKey, parsedAnswer);
                case QuestionType.AutoPropagate:
                    int intValue;
                    if (int.TryParse(answer, out intValue))
                        return new KeyValuePair<Guid, object>(question.PublicKey, intValue);
                    break;

                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion == null)
                        break;
                    // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                    if (numericQuestion.IsInteger)
                    {
                        int intNumericValue;
                        if (int.TryParse(answer, out intNumericValue))
                            return new KeyValuePair<Guid, object>(question.PublicKey, intNumericValue);
                    }
                    else
                    {
                        decimal decimalNumericValue;
                        if (decimal.TryParse(answer, out decimalNumericValue))
                            return new KeyValuePair<Guid, object>(question.PublicKey, decimalNumericValue);
                    }
                    break;

                case QuestionType.DateTime:
                    DateTime date;
                    if (!DateTime.TryParse(answer, out date))
                        break;
                    return new KeyValuePair<Guid, object>(question.PublicKey, date);

                case QuestionType.SingleOption:
                    var singleOption = question as SingleQuestion;
                    if (singleOption != null)
                    {
                        decimal answerValue;
                        if (!decimal.TryParse(answer, out answerValue))
                            break;
                        if (!getAnswerOptionsAsValues(question.PublicKey).Contains(answerValue))
                            break;
                        return new KeyValuePair<Guid, object>(question.PublicKey, answerValue);
                    }
                    break;

                    /* case QuestionType.TextList:
                    case QuestionType.MultyOption:
                    */
            }

            return null;
        }
    }
}