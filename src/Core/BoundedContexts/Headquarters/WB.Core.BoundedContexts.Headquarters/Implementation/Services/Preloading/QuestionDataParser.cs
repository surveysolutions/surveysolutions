using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading
{
    public class QuestionDataParser : IQuestionDataParser
    {

        private readonly QuestionType[] QuestionTypesCommaFirbidden = new[] { QuestionType.MultyOption, QuestionType.SingleOption, QuestionType.Numeric, QuestionType.GpsCoordinates };
        public static string ColumnDelimiter = "__";

        public ValueParsingResult TryParse(string answer, string columnName, IQuestion question, out object parsedValue)
        {
            parsedValue =null;
            answer = answer?
                .Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty);

            if (string.IsNullOrEmpty(answer))
                return ValueParsingResult.ValueIsNullOrEmpty;

            if (question == null)
                return ValueParsingResult.QuestionWasNotFound;

            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                return ValueParsingResult.UnsupportedLinkedQuestion;

            if (question is IMultimediaQuestion)
                return ValueParsingResult.UnsupportedMultimediaQuestion;

            if (answer.Contains(',') && this.QuestionTypesCommaFirbidden.Contains(question.QuestionType))
                return ValueParsingResult.CommaIsUnsupportedInAnswer;

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
                        parsedValue = GeoPosition.ParseProperty(answer, this.ExtractValueFromColumnName(columnName));
                        return ValueParsingResult.OK;
                    }
                    catch (Exception)
                    {
                        return ValueParsingResult.AnswerAsGpsWasNotParsed;
                    }

                case QuestionType.AutoPropagate:
                    int intValue;
                    if (!int.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out intValue))
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
                        if (!int.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out intNumericValue))
                            return ValueParsingResult.AnswerAsIntWasNotParsed;

                        parsedValue = intNumericValue;

                        return ValueParsingResult.OK;
                    }
                    decimal decimalNumericValue;
                    if (!decimal.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out decimalNumericValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                {
                    parsedValue = decimalNumericValue;

                    return ValueParsingResult.OK;
                }

                case QuestionType.DateTime:
                    DateTime date;

                    var dateTimeQuestion = question as DateTimeQuestion;
                    if (dateTimeQuestion == null) return ValueParsingResult.AnswerAsDateTimeWasNotParsed;

                    if (!DateTime.TryParse(answer, out date))
                    {
                        return ValueParsingResult.AnswerAsDateTimeWasNotParsed;
                    }

                    parsedValue = date;
                    return ValueParsingResult.OK;

                case QuestionType.SingleOption:
                    var singleOption = question as SingleQuestion;
                    if (singleOption == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;

                    decimal decimalAnswerValue;
                    if (!decimal.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out decimalAnswerValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    if (!this.GetAnswerOptionsAsValues(question).Contains(decimalAnswerValue))
                        return ValueParsingResult.ParsedValueIsNotAllowed;

                    parsedValue = decimalAnswerValue;
                    return ValueParsingResult.OK;

                case QuestionType.MultyOption:
                    var multyOption = question as MultyOptionsQuestion;
                    if (multyOption == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;

                    string columnEncodedValue = this.ExtractValueFromColumnName(columnName);
                    decimal? columnValue = DecimalToHeaderConverter.ToValue(columnEncodedValue);

                    if (!columnValue.HasValue)
                    {
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    }

                    if (!this.GetAnswerOptionsAsValues(question).Contains(columnValue.Value))
                    {
                        return ValueParsingResult.ParsedValueIsNotAllowed;
                    }

                    int answerChecked;
                    if (!int.TryParse(answer, out answerChecked))
                    {
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    }
                    if (answerChecked > 0)
                    {
                        parsedValue = columnValue.Value;
                    }
                    
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
                    var multioptionQuestion = question as IMultyOptionsQuestion;

                    if (!multioptionQuestion.YesNoView)
                    {
                        return this.ParseMultioptionAnswer(answersWithColumnName);
                    }
                    else
                    {
                        return this.ParseYesNoAnswer(answersWithColumnName);
                    }
                case QuestionType.TextList:
                    return answersWithColumnName.Select((a, i) => new Tuple<decimal, string>(i + 1, a.Item2)).ToArray();
                case QuestionType.GpsCoordinates:
                    return this.CreateGeoPositionAnswer(answersWithColumnName, question);
                default:
                    object parsedAnswer;
                    this.TryParse(answersWithColumnName[0].Item2, answersWithColumnName[0].Item1, question, out parsedAnswer);
                    return parsedAnswer;
            }
        }

        private AnsweredYesNoOption[] ParseYesNoAnswer(Tuple<string, string>[] answersWithColumnName)
        {
            List<Tuple<AnsweredYesNoOption, int>> result = new List<Tuple<AnsweredYesNoOption, int>>();

            foreach (var answerTuple in answersWithColumnName)
            {
                if (!string.IsNullOrEmpty(answerTuple.Item2))
                {
                    string columnEncodedValue = this.ExtractValueFromColumnName(answerTuple.Item1);
                    decimal? columnValue = DecimalToHeaderConverter.ToValue(columnEncodedValue);

                    int answerIndex = int.Parse(answerTuple.Item2);

                    if (answerIndex == 0)
                    {
                        result.Add(Tuple.Create(new AnsweredYesNoOption(columnValue.Value, false), answerIndex));
                    }
                    if (answerIndex > 0)
                    {
                        result.Add(Tuple.Create(new AnsweredYesNoOption(columnValue.Value, true), answerIndex));
                    }
                }
            }

            var sortedYesOptions = result.Where(x => x.Item2 != 0)
                                         .OrderBy(x => x.Item2)
                                         .Select(x => x.Item1);
            var noOptions = result.Where(x => x.Item2 == 0)
                                  .Select(x => x.Item1);

            return sortedYesOptions.Concat(noOptions).ToArray();
        }

        private decimal[] ParseMultioptionAnswer(Tuple<string, string>[] answersWithColumnName)
        {
            List<Tuple<decimal, int>> result = new List<Tuple<decimal, int>>();

            foreach (var answerTuple in answersWithColumnName)
            {
                string columnEncodedValue = this.ExtractValueFromColumnName(answerTuple.Item1);
                decimal? columnValue = DecimalToHeaderConverter.ToValue(columnEncodedValue);

                int answerIndex = int.Parse(answerTuple.Item2);

                if (answerIndex > 0)
                {
                    result.Add(Tuple.Create(columnValue.Value, answerIndex));
                }
            }

            return result.OrderBy(x => x.Item2).Select(x => x.Item1).ToArray();
        }

        private GeoPosition CreateGeoPositionAnswer(Tuple<string, string>[] answersWithColumnName, IQuestion question)
        {
            var result = new GeoPosition();

            foreach (var answerWithColumnName in answersWithColumnName)
            {
                var propertyName = this.ExtractValueFromColumnName(answerWithColumnName.Item1);
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

        private string ExtractValueFromColumnName(string columnName)
        {
            if(columnName.Contains(ColumnDelimiter))
                return columnName.Substring(columnName.LastIndexOf(ColumnDelimiter) + 2).ToLower();
            
            //support of old format is disabled
            /*else if(columnName.Contains("_"))
                return columnName.Substring(columnName.LastIndexOf("_") + 1).ToLower();*/

            return columnName;
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