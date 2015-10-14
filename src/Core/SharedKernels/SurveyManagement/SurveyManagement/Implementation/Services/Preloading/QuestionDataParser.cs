using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class QuestionDataParser : IQuestionDataParser
    {
        public ValueParsingResult TryParse(string answer, string columnName, IQuestion question, out object parsedValue)
        {
            parsedValue =null;

            if (string.IsNullOrEmpty(answer))
                return ValueParsingResult.ValueIsNullOrEmpty;

            if (question == null)
                return ValueParsingResult.QuestionWasNotFound;

            if (question.LinkedToQuestionId.HasValue)
                return ValueParsingResult.UnsupportedLinkedQuestion;

            if (question is IMultimediaQuestion)
                return ValueParsingResult.UnsupportedMultimediaQuestion;

            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    var textQuestion = (TextQuestion) question;
                    parsedValue = answer;
                    if (!string.IsNullOrEmpty(textQuestion.Mask))
                    {
                        var formatter = new MaskedFormatter(textQuestion.Mask);
                        bool maskMatches = formatter.IsTextMaskMatched(answer);
                        if (!maskMatches)
                        {
                            return ValueParsingResult.ParsedValueIsNotAllowed;
                        }
                    }

                    return ValueParsingResult.OK;
                case QuestionType.QRBarcode:
                case QuestionType.TextList:
                    parsedValue = answer;
                    return ValueParsingResult.OK;
                case QuestionType.GpsCoordinates:
                    try
                    {
                        parsedValue = GeoPosition.ParseProperty(answer, GetGpsPropertyFromColumnName(columnName));
                        return ValueParsingResult.OK;
                    }
                    catch (Exception)
                    {
                        return ValueParsingResult.AnswerAsGpsWasNotParsed;
                    }

                case QuestionType.AutoPropagate:
                    int intValue;
                    if (!int.TryParse(answer, out intValue))
                        return ValueParsingResult.AnswerAsIntWasNotParsed;

                    parsedValue = intValue;
                    return ValueParsingResult.OK;

                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;

                    // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                    if (numericQuestion.IsInteger)
                    {
                        int intNumericValue;
                        if (!int.TryParse(answer, out intNumericValue))
                            return ValueParsingResult.AnswerAsIntWasNotParsed;

                        parsedValue = intNumericValue;

                        return ValueParsingResult.OK;
                    }
                    decimal decimalNumericValue;
                    if (!decimal.TryParse(answer, out decimalNumericValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                {
                    parsedValue = decimalNumericValue;

                    return ValueParsingResult.OK;
                }

                case QuestionType.DateTime:
                    DateTime date;
                    if (
                        !DateTime.TryParse(answer, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None,
                            out date))
                        return ValueParsingResult.AnswerAsDateTimeWasNotParsed;
                    parsedValue = date;
                    return ValueParsingResult.OK;

                case QuestionType.SingleOption:
                    var singleOption = question as SingleQuestion;
                    if (singleOption == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;

                    decimal decimalAnswerValue;
                    if (!decimal.TryParse(answer, out decimalAnswerValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    if (!this.GetAnswerOptionsAsValues(question).Contains(decimalAnswerValue))
                        return ValueParsingResult.ParsedValueIsNotAllowed;

                    parsedValue = decimalAnswerValue;
                    return ValueParsingResult.OK;

                case QuestionType.MultyOption:
                    var multyOption = question as MultyOptionsQuestion;
                    if (multyOption == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;

                    decimal answerValue;
                    if (!decimal.TryParse(answer, out answerValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    if (!this.GetAnswerOptionsAsValues(question).Contains(answerValue))
                        return ValueParsingResult.ParsedValueIsNotAllowed;
                    parsedValue = answerValue;
                    return ValueParsingResult.OK;
            }

            return ValueParsingResult.GeneralErrorOccured;                    

        }

        public object BuildAnswerFromStringArray(Tuple<string, string>[] answersWithColumnName, IQuestion question)
        {
            if (question == null || answersWithColumnName == null || !answersWithColumnName.Any())
                return null;

            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    return answersWithColumnName.Select(a => Convert.ToDecimal(a.Item2)).ToArray();
                case QuestionType.TextList:
                    return answersWithColumnName.Select((a, i) => new Tuple<decimal, string>(i + 1, a.Item2)).ToArray();
                case QuestionType.GpsCoordinates:
                    return CreateGeoPositionAnswer(answersWithColumnName, question);
                default:
                    object parsedAnswer;
                    this.TryParse(answersWithColumnName[0].Item2, answersWithColumnName[0].Item1, question, out parsedAnswer);
                    return parsedAnswer;
            }
        }

        private GeoPosition CreateGeoPositionAnswer(Tuple<string, string>[] answersWithColumnName, IQuestion question)
        {
            var result = new GeoPosition();

            foreach (var answerWithColumnName in answersWithColumnName)
            {
                var propertyName = GetGpsPropertyFromColumnName(answerWithColumnName.Item1);
                var typedValue = GeoPosition.ParseProperty(answerWithColumnName.Item2, propertyName);

                switch (propertyName)
                {
                    case "latitude":
                        result.Latitude = (double)typedValue;
                        break;
                    case "longitude":
                        result.Longitude = (double)typedValue;
                        break;
                    case "accuracy":
                        result.Accuracy = (double)typedValue;
                        break;
                    case "altitude":
                        result.Altitude = (double)typedValue;
                        break;
                    case "timestamp":
                        result.Timestamp = (DateTimeOffset)typedValue;
                        break;
                }
            }
            return result;
        }

        private string GetGpsPropertyFromColumnName(string columnName)
        {
            if (!columnName.Contains("_"))
                return columnName;
            return columnName.Substring(columnName.IndexOf("_") + 1).ToLower();
        }

        private decimal[] GetAnswerOptionsAsValues(IQuestion question)
        {
            return
                question.Answers.Select(answer => this.ParseAnswerOptionValueOrThrow(answer.AnswerValue))
                    .Where(o => o.HasValue)
                    .Select(o => o.Value)
                    .ToArray();
        }

        private decimal? ParseAnswerOptionValueOrThrow(string value)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                return null;

            return parsedValue;
        }
    }
}