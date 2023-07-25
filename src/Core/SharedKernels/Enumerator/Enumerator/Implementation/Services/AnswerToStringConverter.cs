using System;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AnswerToStringConverter : IAnswerToStringConverter
    {
        public string GetUiStringAnswerForIdentifyingQuestionOrThrow(object answer, Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.IsIdentifying(questionId)) 
                throw new NotSupportedException("Only identifying question can be converted to string");

            if (answer == null)
                return null;

            var questionType = questionnaire.GetQuestionType(questionId);

            switch (questionType)
            {
                case QuestionType.Text:
                    return (string)answer;
                
                case QuestionType.DateTime:
                    DateTime dateTimeAnswer = answer is string dataTimeAnswerAsString
                        ? DateTime.Parse(dataTimeAnswerAsString)
                        : (DateTime) answer;
                    return FormatDateTimeAnswer(dateTimeAnswer, questionId, questionnaire);

                case QuestionType.MultyOption:
                    if (answer.GetType().IsArray)
                    {
                        answer = (answer as object[])
                            .Select(x => System.Convert.ToDecimal((object) x, CultureInfo.InvariantCulture))
                            .ToArray();
                    }
                    else 
                    {
                        var multiAnswer = ((string) answer).Split(',');
                        answer = multiAnswer
                            .Select(x => System.Convert.ToDecimal(x, CultureInfo.InvariantCulture))
                            .ToArray();
                    }

                    string GetCategoricalOptionText(decimal option) => 
                        questionnaire.GetAnswerOptionTitle(questionId, option, null);
                    return AnswerUtils.AnswerToString(answer, GetCategoricalOptionText);
                
                case QuestionType.SingleOption:
                    var singleAnswer = System.Convert.ToDecimal(answer, CultureInfo.InvariantCulture);
                    return FormatSingleOptionAnswer(singleAnswer, questionId, questionnaire);

                case QuestionType.Numeric:
                    decimal answerTyped = answer is string numericAnswerAsString
                        ? decimal.Parse(numericAnswerAsString, CultureInfo.InvariantCulture)
                        : System.Convert.ToDecimal(answer, CultureInfo.CurrentCulture);
                    return FormatNumericAnswer(answerTyped, questionId, questionnaire);

                case QuestionType.GpsCoordinates:
                    var gps = answer is string gpsAnswerAsString
                        ? GeoPosition.FromString(gpsAnswerAsString)
                        : (GeoPosition) answer;
                    return FormatGpsAnswer(gps.Latitude, gps.Longitude);
            }
        
            throw new InvalidOperationException("Unsupported question type: " + questionType);
        }
        
        public string GetUiStringAnswerForIdentifyingQuestionOrThrow(AbstractAnswer answer, Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.IsIdentifying(questionId)) 
                throw new NotSupportedException("Only identifying question can be converted to string");

            if (answer == null)
                return null;

            switch (answer)
            {
                case TextAnswer textAnswer: 
                    return textAnswer.Value;
                case DateTimeAnswer dateTimeAnswer: 
                    return FormatDateTimeAnswer(dateTimeAnswer.Value, questionId, questionnaire);
                case NumericIntegerAnswer numericIntegerAnswer:
                    return FormatNumericIntegerAnswer(numericIntegerAnswer.Value, questionId, questionnaire);
                case NumericRealAnswer numericRealAnswer: 
                    return FormatNumericRealAnswer(numericRealAnswer.Value, questionId, questionnaire);
                case GpsAnswer gpsAnswer:
                    return FormatGpsAnswer(gpsAnswer.Value.Latitude, gpsAnswer.Value.Longitude);
                case CategoricalFixedSingleOptionAnswer singleOptionAnswer:
                    return FormatSingleOptionAnswer(singleOptionAnswer.SelectedValue, questionId, questionnaire);
            }

            throw new InvalidOperationException("Unsupported answer type: " + answer.GetType());
        }

        
        private string FormatDateTimeAnswer(DateTime dateTime, Guid questionId, IQuestionnaire questionnaire) 
        {
            var isTimestamp = questionnaire.IsTimestampQuestion(questionId);
            return isTimestamp
                ? dateTime.ToString(DateTimeFormat.DateWithTimeFormat)
                : dateTime.ToString(DateTimeFormat.DateFormat);
        }
        private string FormatNumericIntegerAnswer(int answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            return questionnaire.ShouldUseFormatting(questionId)
                ? answer.FormatInt()
                : answer.ToString(CultureInfo.CurrentCulture);    
        }
        private string FormatNumericRealAnswer(double answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            return questionnaire.ShouldUseFormatting(questionId)
                ? answer.FormatDouble()
                : answer.ToString(CultureInfo.CurrentCulture);    
        }
        private string FormatNumericAnswer(decimal answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            return questionnaire.ShouldUseFormatting(questionId)
                ? answer.FormatDecimal()
                : answer.ToString(CultureInfo.CurrentCulture);        
        }
        private string FormatGpsAnswer(double latitude, double longitude)
        {
            return $"{latitude}, {longitude}";
        }
        private string FormatSingleOptionAnswer(decimal answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            string GetCategoricalOptionText(decimal option) => questionnaire.GetAnswerOptionTitle(questionId, option, null);
            return AnswerUtils.AnswerToString(answer, GetCategoricalOptionText);
        }
    }
}
