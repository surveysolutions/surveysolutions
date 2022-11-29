using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class ExportQuestionService : IExportQuestionService
    {
        private static readonly CultureInfo ExportCulture = CultureInfo.InvariantCulture;

        public string[] GetExportedQuestion(InterviewEntity? question, ExportedQuestionHeaderItem header)
        {
            var answers = this.GetAnswers(question, header);

            if (answers.Length != header.ColumnHeaders.Count)
                throw new InvalidOperationException(
                    $"Something wrong with export logic, answer's count is less then required by template. Was '{answers.Length}', expected '{header.ColumnHeaders.Count}'");

            return answers;
        }

        public string[] GetExportedVariable(object? variable, ExportedVariableHeaderItem header, bool isDisabled)
        {
            if (isDisabled)
                return header.ColumnHeaders.Select(c => ExportFormatSettings.DisableValue).ToArray();

            switch (header.VariableType)
            {
                case VariableType.String:
                    return new string[] { (string?)variable ?? ExportFormatSettings.MissingStringQuestionValue };
                case VariableType.LongInteger:
                    return new string[] { ((long?)variable)?.ToString(CultureInfo.InvariantCulture) ?? ExportFormatSettings.MissingNumericQuestionValue };
                case VariableType.Boolean:
                    return new string[] { (bool?)variable == true ? "1" : (bool?)variable == false ? "0" : ExportFormatSettings.MissingNumericQuestionValue };
                case VariableType.Double:
                {
                    if(variable == null)
                        return new string[]{ ExportFormatSettings.MissingNumericQuestionValue};
                    var val = Convert.ToDouble(variable);
                    if(double.IsNaN(val)|| double.IsInfinity(val))
                        return new string[] { ExportFormatSettings.MissingNumericQuestionValue };

                    return new string[] {val.ToString(CultureInfo.InvariantCulture)};
                };
                case VariableType.DateTime:
                    return new string[] { ((DateTime?)variable)?.ToString(ExportFormatSettings.ExportDateTimeFormat) ?? ExportFormatSettings.MissingStringQuestionValue };

                default:
                    throw new ArgumentException("Unknown variable type");
            }
        }

        private string[] GetAnswers(InterviewEntity? question, ExportedQuestionHeaderItem header)
        {
            if (question == null)
                return BuildMissingValueAnswer(header);

            if (!question.IsEnabled)
                return header.ColumnHeaders.Select(c => ExportFormatSettings.DisableValue).ToArray();

            if (question.AsObject() == null)
                return BuildMissingValueAnswer(header);

            var gpsQuestion = question.AsGps;
            if (gpsQuestion != null)
            {
                return new[]
                {
                    gpsQuestion.Latitude.ToString(ExportCulture),
                    gpsQuestion.Longitude.ToString(ExportCulture),
                    gpsQuestion.Accuracy?.ToString(ExportCulture) ?? string.Empty,
                    gpsQuestion.Altitude?.ToString(ExportCulture) ?? string.Empty,
                    gpsQuestion.Timestamp.DateTime.ToString(ExportFormatSettings.ExportDateTimeFormat, ExportCulture)
                };
            }

            var areaQuestion = question.AsArea;
            if (areaQuestion != null)
            {
                return new[]
                {
                    this.ConvertAnswerToStringValue(question.AsObject(), header),
                    areaQuestion.AreaSize?.ToString(ExportCulture) ?? string.Empty,
                    areaQuestion.Length?.ToString(ExportCulture) ?? string.Empty,
                    areaQuestion.NumberOfPoints?.ToString(ExportCulture) ?? string.Empty,
                    areaQuestion.RequestedAccuracy?.ToString(ExportCulture) ?? string.Empty,
                    areaQuestion.RequestedFrequency?.ToString(ExportCulture) ?? string.Empty,
                };
            }
            
            switch (header.QuestionType)
            {
                case QuestionType.DateTime:
                case QuestionType.Multimedia:
                case QuestionType.Numeric:
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                case QuestionType.Audio:
                    return new string[] { this.ConvertAnswerToStringValue(question.AsObject(), header) };

                case QuestionType.MultyOption:
                {
                    switch (header.QuestionSubType)
                    {
                        case null:
                            return GetCategoricalMultiAnswers(question.AsIntArray, header, header.ColumnHeaders.Count).ToArray();
                        case QuestionSubtype.MultyOption_Combobox:
                            return GetMultiLinkedToListAnswers(question.AsIntArray, header, header.ColumnHeaders.Count).ToArray();
                        case QuestionSubtype.MultiOptionOrdered:
                            return GetCategoricalMultiOrderedAnswers(question.AsIntArray, header, header.ColumnHeaders.Count).ToArray();
                        case QuestionSubtype.MultiOptionYesNo:
                            return GetYesNoAnswers(question.AsYesNo, header, header.ColumnHeaders.Count, false).ToArray();
                        case QuestionSubtype.MultiOptionYesNoOrdered:
                            return GetYesNoAnswers(question.AsYesNo, header, header.ColumnHeaders.Count, true).ToArray();
                        case QuestionSubtype.MultiOptionLinkedFirstLevel:
                        case QuestionSubtype.MultiOptionLinkedNestedLevel:
                        {
                            if (question.AsIntMatrix != null)
                                return GetMultiLinkedToRosterAnswers(question.AsIntMatrix, header, header.ColumnHeaders.Count).ToArray();
                            if (question.AsIntArray != null)
                                return GetMultiLinkedToListAnswers(question.AsIntArray, header, header.ColumnHeaders.Count).ToArray();
                        }
                            break;
                    }
                    return BuildAnswerListForQuestionByHeader(question.AsObject(), header);
                }
                case QuestionType.SingleOption:
                {
                    return (header.QuestionSubType == QuestionSubtype.SingleOptionLinkedFirstLevel 
                            || header.QuestionSubType == QuestionSubtype.SingleOptionLinkedNestedLevel) 
                           && question.AsIntArray != null
                        ? GetSingleLinkedToRosterAnswer(question.AsIntArray, header).ToArray()
                        : new[] {this.ConvertAnswerToStringValue(question.AsInt, header)};
                }
                case QuestionType.TextList:
                    return this.BuildAnswerListForQuestionByHeader(question.AsObject(), header);
                default:
                    return Array.Empty<string>();
            }
        }

        private static string[] BuildMissingValueAnswer(ExportedQuestionHeaderItem header)
        {
            return header.ColumnHeaders.Select(headerColumn =>
            {
                switch (headerColumn.ExportType)
                {
                    case ExportValueType.NumericInt:
                    case ExportValueType.Numeric:
                        return ExportFormatSettings.MissingNumericQuestionValue;
                    case ExportValueType.Boolean:
                    case ExportValueType.Date:
                    case ExportValueType.String:
                    case ExportValueType.DateTime:
                        return ExportFormatSettings.MissingStringQuestionValue;
                    case ExportValueType.Unknown:
                    default:
                        throw new Exception("Unknown ExportValueType: " + headerColumn.ExportType);
                }
            }).ToArray();
        }

        private IEnumerable<object>? TryCastToEnumerable(object? value)
        {
            switch (value)
            {
                case IEnumerable<decimal> arrayOfDecimal:
                    return arrayOfDecimal.Select(d => (object)d);
                case IEnumerable<int> arrayOfInteger:
                    return arrayOfInteger.Select(i => (object)i);
                case InterviewTextListAnswer[] interviewTextListAnswer:
                    return interviewTextListAnswer.Select(a => a.Answer).ToArray();
                default:
                {
                    var listOfAnswers = value as IEnumerable<object>;
                    return listOfAnswers;
                }
            }
        }

        private string ConvertAnswerToStringValue(object? answer, ExportedQuestionHeaderItem header)
        {
            if (answer == null)
                return ExportFormatSettings.MissingStringQuestionValue;

            var arrayOfObject = this.TryCastToEnumerable(answer)?.ToArray();

            if (arrayOfObject != null && arrayOfObject.Any())
            {
                if (header.LengthOfRosterVectorWhichNeedToBeExported.HasValue)
                {
                    var shrinkedArrayOfAnswers = arrayOfObject.Skip(arrayOfObject.Count() - header.LengthOfRosterVectorWhichNeedToBeExported.Value).ToArray();

                    if (shrinkedArrayOfAnswers.Length == 1)
                    {
                        var arrayOfAnswers = this.TryCastToEnumerable(shrinkedArrayOfAnswers[0])?.ToArray();

                        return arrayOfAnswers != null
                            ? string.Join(ExportFormatSettings.DefaultDelimiter, arrayOfAnswers.Select(x => this.ConvertAnswerToString(x, header.QuestionType, header.QuestionSubType)).ToArray())
                            : this.ConvertAnswerToString(shrinkedArrayOfAnswers[0], header.QuestionType, header.QuestionSubType);
                    }

                    return string.Join(ExportFormatSettings.DefaultDelimiter, shrinkedArrayOfAnswers.Select(x => this.ConvertAnswerToString(x, header.QuestionType, header.QuestionSubType)).ToArray());
                }
                return string.Join(ExportFormatSettings.DefaultDelimiter, arrayOfObject.Select(x => this.ConvertAnswerToString(x, header.QuestionType, header.QuestionSubType)).ToArray());
            }

            return this.ConvertAnswerToString(answer, header.QuestionType, header.QuestionSubType);
        }

        private string[] BuildAnswerListForQuestionByHeader(object? answer, ExportedQuestionHeaderItem header)
        {
            if (header.ColumnHeaders.Count == 1)
                return new string[] { this.ConvertAnswerToStringValue(answer, header) };

            var result = new string[header.ColumnHeaders.Count];

            var answersAsEnumerable = this.TryCastToEnumerable(answer);
            var answers = answersAsEnumerable?.ToArray() ?? new object?[] { answer };

            if (header.QuestionType != QuestionType.MultyOption)
            {
                this.PutAnswersAsStringValuesIntoResultArray(answers, header, result);
            }


            return result;
        }

        private IEnumerable<string> GetSingleLinkedToRosterAnswer(int[]? answer, ExportedQuestionHeaderItem header)
        {
            if (answer == null) yield return ExportFormatSettings.MissingNumericQuestionValue;

            yield return ConvertAnswerToStringValue(answer, header);
        }

        private IEnumerable<string> GetMultiLinkedToListAnswers(int[]? answers, ExportedQuestionHeaderItem header, int expectedColumnCount)
        {
            if (answers != null)
            {
                foreach (var answer in answers)
                    yield return this.ConvertAnswerToStringValue(answer, header);
            }

            for (int i = 0; i < expectedColumnCount - (answers?.Length ?? 0); i++)
                yield return ExportFormatSettings.MissingNumericQuestionValue;
        }

        private IEnumerable<string> GetMultiLinkedToRosterAnswers(int[][]? answers, ExportedQuestionHeaderItem header, int expectedColumnCount)
        {
            if (answers != null)
            {
                foreach (var answer in answers)
                    yield return this.ConvertAnswerToStringValue(answer, header);
            }

            for (int i = 0; i < expectedColumnCount - (answers?.Length ?? 0); i++)
                yield return ExportFormatSettings.MissingNumericQuestionValue;
        }

        private static IEnumerable<string> GetCategoricalMultiAnswers(int[]? answers, ExportedQuestionHeaderItem header, int expectedColumnCount)
        {
            bool isMissingQuestion = answers == null || answers.Length == 0;

            for (int i = 0; i < expectedColumnCount; i++)
            {
                if (isMissingQuestion)
                {
                    yield return ExportFormatSettings.MissingNumericQuestionValue;
                }
                else
                {
                    int checkedOptionIndex = Array.IndexOf(answers!, header.ColumnValues[i]);
                    yield return checkedOptionIndex > -1 ? "1" : "0";
                }
            }
        }

        private static IEnumerable<string> GetCategoricalMultiOrderedAnswers(int[]? answers, ExportedQuestionHeaderItem header, int expectedColumnCount)
        {
            bool isMissingQuestion = answers == null || answers.Length == 0;

            for (int i = 0; i < expectedColumnCount; i++)
            {
                if (isMissingQuestion)
                {
                    yield return ExportFormatSettings.MissingNumericQuestionValue;
                }
                else
                {
                    int checkedOptionIndex = Array.IndexOf(answers!, header.ColumnValues[i]);
                    yield return checkedOptionIndex > -1 ? (checkedOptionIndex + 1).ToString(ExportCulture) : "0";
                }
            }
        }

        private void PutAnswersAsStringValuesIntoResultArray(object?[] answers, ExportedQuestionHeaderItem header, string[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = answers.Length > i ? this.ConvertAnswerToStringValue(answers[i], header) : ExportFormatSettings.MissingStringQuestionValue;
            }
        }

        private static IEnumerable<string> GetYesNoAnswers(AnsweredYesNoOption[]? answers, ExportedQuestionHeaderItem header, int expectedColumnCount, bool ordered)
        {
            for (int i = 0; i < expectedColumnCount; i++)
            {
                if(answers == null)
                    yield return ExportFormatSettings.MissingNumericQuestionValue;

                decimal columnValue = header.ColumnValues[i];
                var selectedOption = answers!.FirstOrDefault(x => x.OptionValue == columnValue);

                if (selectedOption != null)
                {
                    if (selectedOption.Yes)
                    {
                        if (ordered)
                        {
                            var selectedItemIndex = answers!
                                .Where(x => x.Yes)
                                .Select((item, index) => new { item, index })
                                .FirstOrDefault(x => x.item.OptionValue == columnValue);
                            
                            if(selectedItemIndex == null)
                                yield return ExportFormatSettings.MissingNumericQuestionValue;
                            
                            yield return (selectedItemIndex!.index + 1).ToString(ExportCulture);
                        }
                        else
                        {
                            yield return "1";
                        }
                    }
                    else
                    {
                        yield return "0";
                    }
                }
                else
                {
                    yield return ExportFormatSettings.MissingNumericQuestionValue;
                }
            }
        }

        private string ConvertAnswerToString(object obj, QuestionType questionType, QuestionSubtype? questionSubType)
        {
            if (obj is IFormattable formattable)
            {
                var isDateTimeQuestion = questionType == QuestionType.DateTime;
                var isTimestampQuestion = isDateTimeQuestion && questionSubType.HasValue && questionSubType == QuestionSubtype.DateTimeTimestamp;

                return formattable.ToString(isTimestampQuestion ? ExportFormatSettings.ExportDateTimeFormat : isDateTimeQuestion ? ExportFormatSettings.ExportDateFormat : null, ExportCulture);
            }

            if (questionType == QuestionType.Audio)
                return (obj as AudioAnswer)?.FileName ?? string.Empty;

            var answersSeparator = ExportFileSettings.NotReadableAnswersSeparator.ToString();

            if (questionType == QuestionType.TextList)
            {
                if (obj is String ans) return ans;
                
                return (obj as InterviewTextListAnswer)?.Answer.Replace(answersSeparator, String.Empty) ?? String.Empty;
            }

            var asString = obj.ToString();
            return asString == null ? string.Empty : asString.Replace(answersSeparator, string.Empty);
        }
    }
}
