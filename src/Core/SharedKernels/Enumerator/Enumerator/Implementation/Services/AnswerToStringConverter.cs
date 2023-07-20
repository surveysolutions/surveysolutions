using System;
using System.Globalization;
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
        public string Convert(object answer, Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.IsPrefilled(questionId)) 
                throw new NotSupportedException("Only identifying question can be converted to string");

            var questionType = questionnaire.GetQuestionType(questionId);

            switch (questionType)
            {
                case QuestionType.Text:
                    return (string)answer;
                
                case QuestionType.DateTime:
                    DateTime dateTimeAnswer = (DateTime) answer;
                    return DateTimeQuestion(dateTimeAnswer, questionId, questionnaire);

                case QuestionType.SingleOption:
                    var singleAnswer = System.Convert.ToDecimal(answer, CultureInfo.InvariantCulture);
                    return SingleOptionQuestion(singleAnswer, questionId, questionnaire);

                case QuestionType.Numeric:
                    decimal answerTyped = System.Convert.ToDecimal(answer, CultureInfo.CurrentCulture);
                    return NumericQuestion(answerTyped, questionId, questionnaire);

                case QuestionType.GpsCoordinates:
                    var gps = (GeoPosition) answer;
                    return GpsQuestion(gps.Latitude, gps.Longitude);
            }
        
            throw new Exception("Unsupported question type: " + questionType);
        }
        
        public string Convert(AbstractAnswer answer, Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.IsPrefilled(questionId)) 
                throw new NotSupportedException("Only identifying question can be converted to string");

            if (answer == null)
                return null;

            switch (answer)
            {
                case TextAnswer textAnswer: 
                    return textAnswer.Value;
                case DateTimeAnswer dateTimeAnswer: 
                    return DateTimeQuestion(dateTimeAnswer.Value, questionId, questionnaire);
                case NumericIntegerAnswer numericIntegerAnswer:
                    return NumericIntegerQuestion(numericIntegerAnswer.Value, questionId, questionnaire);
                case NumericRealAnswer numericRealAnswer: 
                    return NumericRealQuestion(numericRealAnswer.Value, questionId, questionnaire);
                case GpsAnswer gpsAnswer:
                    return GpsQuestion(gpsAnswer.Value.Latitude, gpsAnswer.Value.Longitude);
                case CategoricalFixedSingleOptionAnswer singleOptionAnswer:
                    return SingleOptionQuestion(singleOptionAnswer.SelectedValue, questionId, questionnaire);
            }

            throw new Exception("Unsupported answer type: " + answer.GetType());
        }

        
        private string DateTimeQuestion(DateTime dateTime, Guid questionId, IQuestionnaire questionnaire) 
        {
            var isTimestamp = questionnaire.IsTimestampQuestion(questionId);
            return isTimestamp
                ? dateTime.ToString(DateTimeFormat.DateWithTimeFormat)
                : dateTime.ToString(DateTimeFormat.DateFormat);
        }
        private string NumericIntegerQuestion(int answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            return questionnaire.ShouldUseFormatting(questionId)
                ? answer.FormatInt()
                : answer.ToString(CultureInfo.CurrentCulture);    
        }
        private string NumericRealQuestion(double answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            return questionnaire.ShouldUseFormatting(questionId)
                ? answer.FormatDouble()
                : answer.ToString(CultureInfo.CurrentCulture);    
        }
        private string NumericQuestion(decimal answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            return questionnaire.ShouldUseFormatting(questionId)
                ? answer.FormatDecimal()
                : answer.ToString(CultureInfo.CurrentCulture);        
        }
        private string GpsQuestion(double latitude, double longitude)
        {
            return $"{latitude}, {longitude}";
        }
        private string SingleOptionQuestion(decimal answer, Guid questionId, IQuestionnaire questionnaire) 
        {
            string GetCategoricalOptionText(decimal option) => questionnaire.GetAnswerOptionTitle(questionId, option, null);
            return AnswerUtils.AnswerToString(answer, GetCategoricalOptionText);
        }
    }
}
