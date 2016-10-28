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
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading
{
    public class QuestionDataParser : IQuestionDataParser
    {

        private readonly QuestionType[] QuestionTypesCommaFirbidden = new[] { QuestionType.MultyOption, QuestionType.SingleOption, QuestionType.Numeric, QuestionType.GpsCoordinates };
        public static string ColumnDelimiter = "__";

        private AbstractAnswer ParseSingleColumnAnswer(string answer, string columnName, IQuestion question)
        {
            object parsedValue;
            AbstractAnswer parsedSingleColumnAnswer;

            return this.TryParse(answer, columnName, question, out parsedValue, out parsedSingleColumnAnswer)
                == ValueParsingResult.OK
                ? parsedSingleColumnAnswer
                : null;
        }

        public ValueParsingResult TryParse(string answer, string columnName, IQuestion question, out object parsedValue, out AbstractAnswer parsedSingleColumnAnswer)
        {
            answer = answer?
                .Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty);

            if (string.IsNullOrEmpty(answer))
                return ParseFailed(ValueParsingResult.ValueIsNullOrEmpty, out parsedValue, out parsedSingleColumnAnswer);

            if (question == null)
                return ParseFailed(ValueParsingResult.QuestionWasNotFound, out parsedValue, out parsedSingleColumnAnswer);

            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                return ParseFailed(ValueParsingResult.UnsupportedLinkedQuestion, out parsedValue, out parsedSingleColumnAnswer);

            if (question is IMultimediaQuestion)
                return ParseFailed(ValueParsingResult.UnsupportedMultimediaQuestion, out parsedValue, out parsedSingleColumnAnswer);

            if (answer.Contains(',') && this.QuestionTypesCommaFirbidden.Contains(question.QuestionType))
                return ParseFailed(ValueParsingResult.CommaIsUnsupportedInAnswer, out parsedValue, out parsedSingleColumnAnswer);

            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    var textQuestion = (TextQuestion) question;
                    parsedValue = answer;
                    parsedSingleColumnAnswer = new TextAnswer(answer);
                    if (!string.IsNullOrEmpty(textQuestion.Mask))
                    {
                        var formatter = new MaskedFormatter(textQuestion.Mask);
                        bool maskMatches = formatter.IsTextMaskMatched(answer);
                        if (!maskMatches)
                            return ParseFailed(ValueParsingResult.ParsedValueIsNotAllowed, out parsedValue, out parsedSingleColumnAnswer);
                    }
                    return ValueParsingResult.OK;

                case QuestionType.QRBarcode:
                    parsedValue = answer;
                    parsedSingleColumnAnswer = new QRBarcodeAnswer(answer);
                    return ValueParsingResult.OK;

                case QuestionType.TextList:
                    parsedValue = answer;
                    parsedSingleColumnAnswer = null;
                    return ValueParsingResult.OK;

                case QuestionType.GpsCoordinates:
                    try
                    {
                        parsedValue = GeoPosition.ParseProperty(answer, this.ExtractValueFromColumnName(columnName));
                        parsedSingleColumnAnswer = null;
                        return ValueParsingResult.OK;
                    }
                    catch (Exception)
                    {
                        return ParseFailed(ValueParsingResult.AnswerAsGpsWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);
                    }

                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion == null)
                        return ParseFailed(ValueParsingResult.QuestionTypeIsIncorrect, out parsedValue, out parsedSingleColumnAnswer);

                    // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                    if (numericQuestion.IsInteger)
                    {
                        int intNumericValue;
                        if (!int.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out intNumericValue))
                            return ParseFailed(ValueParsingResult.AnswerAsIntWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);

                        parsedValue = intNumericValue;
                        parsedSingleColumnAnswer = new NumericIntegerAnswer(intNumericValue);
                        return ValueParsingResult.OK;
                    }
                    else
                    {
                        decimal decimalNumericValue;
                        if (!decimal.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out decimalNumericValue))
                            return ParseFailed(ValueParsingResult.AnswerAsDecimalWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);

                        parsedValue = decimalNumericValue;
                        parsedSingleColumnAnswer = new NumericRealAnswer((double) decimalNumericValue);
                        return ValueParsingResult.OK;
                    }

                case QuestionType.DateTime:
                    DateTime date;

                    var dateTimeQuestion = question as DateTimeQuestion;
                    if (dateTimeQuestion == null)
                        return ParseFailed(ValueParsingResult.QuestionTypeIsIncorrect, out parsedValue, out parsedSingleColumnAnswer);

                    if (!DateTime.TryParse(answer, out date))
                        return ParseFailed(ValueParsingResult.AnswerAsDateTimeWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);

                    parsedValue = date;
                    parsedSingleColumnAnswer = new DateTimeAnswer(date);
                    return ValueParsingResult.OK;

                case QuestionType.SingleOption:
                    var singleOption = question as SingleQuestion;
                    if (singleOption == null)
                        return ParseFailed(ValueParsingResult.QuestionTypeIsIncorrect, out parsedValue, out parsedSingleColumnAnswer);

                    decimal decimalAnswerValue;
                    if (!decimal.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out decimalAnswerValue))
                        return ParseFailed(ValueParsingResult.AnswerAsDecimalWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);

                    if (!this.GetAnswerOptionsAsValues(question).Contains(decimalAnswerValue))
                        return ParseFailed(ValueParsingResult.ParsedValueIsNotAllowed, out parsedValue, out parsedSingleColumnAnswer);

                    parsedValue = decimalAnswerValue;
                    parsedSingleColumnAnswer = new CategoricalFixedSingleOptionAnswer((int) decimalAnswerValue);
                    return ValueParsingResult.OK;

                case QuestionType.MultyOption:
                    var multyOption = question as MultyOptionsQuestion;
                    if (multyOption == null)
                        return ParseFailed(ValueParsingResult.QuestionTypeIsIncorrect, out parsedValue, out parsedSingleColumnAnswer);

                    string columnEncodedValue = this.ExtractValueFromColumnName(columnName);
                    decimal? columnValue = DecimalToHeaderConverter.ToValue(columnEncodedValue);

                    if (!columnValue.HasValue)
                        return ParseFailed(ValueParsingResult.AnswerAsDecimalWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);

                    if (!this.GetAnswerOptionsAsValues(question).Contains(columnValue.Value))
                        return ParseFailed(ValueParsingResult.ParsedValueIsNotAllowed, out parsedValue, out parsedSingleColumnAnswer);

                    int answerChecked;
                    if (!int.TryParse(answer, out answerChecked))
                        return ParseFailed(ValueParsingResult.AnswerAsIntWasNotParsed, out parsedValue, out parsedSingleColumnAnswer);

                    if (answerChecked > 0)
                    {
                        parsedValue = columnValue.Value;
                    }
                    else
                    {
                        parsedValue = null;
                    }

                    parsedSingleColumnAnswer = null;

                    return ValueParsingResult.OK;
            }

            return ParseFailed(ValueParsingResult.GeneralErrorOccured, out parsedValue, out parsedSingleColumnAnswer);
        }

        private static ValueParsingResult ParseFailed(ValueParsingResult result, out object parsedValue, out AbstractAnswer parsedSingleColumnAnswer)
        {
            parsedValue = null;
            parsedSingleColumnAnswer = null;
            return result;
        }

        public AbstractAnswer BuildAnswerFromStringArray(Tuple<string, string>[] answersWithColumnName, IQuestion question)
        {
            if (question == null || answersWithColumnName == null || !answersWithColumnName.Any())
                return null;

            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    var multioptionQuestion = (IMultyOptionsQuestion) question;
                    if (!multioptionQuestion.YesNoView)
                        return this.ParseMultioptionAnswer(answersWithColumnName);
                    else
                        return this.ParseYesNoAnswer(answersWithColumnName);
                case QuestionType.TextList:
                    var rows = answersWithColumnName.Select((a, i) => new TextListAnswerRow(i + 1, a.Item2)).ToArray();
                    return new TextListAnswer(rows);
                case QuestionType.GpsCoordinates:
                    return this.CreateGeoPositionAnswer(answersWithColumnName, question);
                default:
                    AbstractAnswer parsedAnswer = this.ParseSingleColumnAnswer(answersWithColumnName[0].Item2, answersWithColumnName[0].Item1, question);
                    return parsedAnswer;
            }
        }

        private YesNoAnswer ParseYesNoAnswer(Tuple<string, string>[] answersWithColumnName)
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

            var answeredYesNoOptions = sortedYesOptions.Concat(noOptions).ToArray();

            return new YesNoAnswer(answeredYesNoOptions);
        }

        private CategoricalFixedMultiOptionAnswer ParseMultioptionAnswer(Tuple<string, string>[] answersWithColumnName)
        {
            var result = new List<Tuple<int, int>>();

            foreach (var answerTuple in answersWithColumnName)
            {
                string columnEncodedValue = this.ExtractValueFromColumnName(answerTuple.Item1);
                int? columnValue = ToNullableInt(DecimalToHeaderConverter.ToValue(columnEncodedValue));

                int answerIndex = int.Parse(answerTuple.Item2);

                if (answerIndex > 0)
                {
                    result.Add(Tuple.Create(columnValue.Value, answerIndex));
                }
            }

            return new CategoricalFixedMultiOptionAnswer(result.OrderBy(x => x.Item2).Select(x => x.Item1));
        }

        private GpsAnswer CreateGeoPositionAnswer(Tuple<string, string>[] answersWithColumnName, IQuestion question)
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
            return new GpsAnswer(result);
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

        private IEnumerable<decimal> GetAnswerOptionsAsValues(IQuestion question)
        {
            return question.Answers.Select(answer => answer.GetParsedValue());
        }

        private static int? ToNullableInt(decimal? value) => value.HasValue ? (int)value.Value : null as int?;
    }
}