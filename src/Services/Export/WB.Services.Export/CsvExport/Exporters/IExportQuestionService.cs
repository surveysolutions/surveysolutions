using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IExportQuestionService
    {
        string[] GetExportedQuestion(InterviewEntity question, ExportedQuestionHeaderItem header);
        string[] GetExportedVariable(object variable, ExportedVariableHeaderItem header, bool isDisabled);
    }

    public class ExportQuestionService : IExportQuestionService
    {
        private static readonly CultureInfo ExportCulture = CultureInfo.InvariantCulture;

        public string[] GetExportedQuestion(InterviewEntity question, ExportedQuestionHeaderItem header)
        {
            var answers = this.GetAnswers(question, header);

            if (answers.Length != header.ColumnHeaders.Count)
                throw new InvalidOperationException(
                    $"Something wrong with export logic, answer's count is less then required by template. Was '{answers.Length}', expected '{header.ColumnHeaders.Count}'");

            return answers;
        }

        public string[] GetExportedVariable(object variable, ExportedVariableHeaderItem header, bool isDisabled)
        {
            if (isDisabled)
                return header.ColumnHeaders.Select(c => ExportFormatSettings.DisableValue).ToArray();

            switch (header.VariableType)
            {
                case VariableType.String:
                    return new string[] { (string)variable ?? ExportFormatSettings.MissingStringQuestionValue };
                case VariableType.LongInteger:
                    return new string[] { ((long?)variable)?.ToString(CultureInfo.InvariantCulture) ?? ExportFormatSettings.MissingNumericQuestionValue };
                case VariableType.Boolean:
                    return new string[] { (bool?)variable == true ? "1" : (bool?)variable == false ? "0" : ExportFormatSettings.MissingNumericQuestionValue };
                case VariableType.Double:
                    return new string[] { variable == null ? ExportFormatSettings.MissingNumericQuestionValue : Convert.ToDouble(variable).ToString(CultureInfo.InvariantCulture) };
                case VariableType.DateTime:
                    return new string[] { ((DateTime?)variable)?.ToString(ExportFormatSettings.ExportDateTimeFormat) ?? ExportFormatSettings.MissingStringQuestionValue };

                default:
                    throw new ArgumentException("Unknown variable type");
            }
        }

        private string[] GetAnswers(InterviewEntity question, ExportedQuestionHeaderItem header)
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
                    gpsQuestion.Accuracy.ToString(ExportCulture),
                    gpsQuestion.Altitude.ToString(ExportCulture),
                    gpsQuestion.Timestamp.DateTime.ToString(ExportFormatSettings.ExportDateTimeFormat, ExportCulture)
                };
            }

            switch (header.QuestionType)
            {
                case QuestionType.DateTime:
                case QuestionType.Multimedia:
                case QuestionType.Numeric:
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                case QuestionType.Area:
                case QuestionType.Audio:
                    return new string[] { this.ConvertAnswerToStringValue(question.AsObject(), header) };

                case QuestionType.MultyOption:
                case QuestionType.SingleOption:
                case QuestionType.TextList:
                    return this.BuildAnswerListForQuestionByHeader(question.AsObject(), header);
                default:
                    return Array.Empty<string>();
            }
        }

        private static string[] BuildMissingValueAnswer(ExportedQuestionHeaderItem header)
        {
            if (header.QuestionType == QuestionType.GpsCoordinates)
                return new[] { ExportFormatSettings.MissingNumericQuestionValue, ExportFormatSettings.MissingNumericQuestionValue, ExportFormatSettings.MissingNumericQuestionValue, ExportFormatSettings.MissingNumericQuestionValue, ExportFormatSettings.MissingStringQuestionValue };

            string missingValue = header.QuestionType == QuestionType.Numeric
                                  || header.QuestionType == QuestionType.SingleOption
                                  || header.QuestionType == QuestionType.MultyOption
                    ? ExportFormatSettings.MissingNumericQuestionValue
                    : ExportFormatSettings.MissingStringQuestionValue;
            return header.ColumnHeaders.Select(c => missingValue).ToArray();
        }

        private IEnumerable<object> TryCastToEnumerable(object value)
        {
            var arrayOfDecimal = value as IEnumerable<decimal>;
            if (arrayOfDecimal != null)
            {
                return arrayOfDecimal.Select(d => (object)d);
            }

            var arrayOfInteger = value as IEnumerable<int>;
            if (arrayOfInteger != null)
            {
                return arrayOfInteger.Select(i => (object)i);
            }

            if (value is InterviewTextListAnswer[] interviewTextListAnswer)
            {
                return interviewTextListAnswer.Select(a => a.Answer).ToArray();
            }

            var listOfAnswers = value as IEnumerable<object>;
            return listOfAnswers;
        }

        private string ConvertAnswerToStringValue(object answer, ExportedQuestionHeaderItem header)
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

                    return string.Format("[{0}]", string.Join(ExportFormatSettings.DefaultDelimiter, shrinkedArrayOfAnswers.Select(x => this.ConvertAnswerToString(x, header.QuestionType, header.QuestionSubType)).ToArray()));
                }
                return string.Join(ExportFormatSettings.DefaultDelimiter, arrayOfObject.Select(x => this.ConvertAnswerToString(x, header.QuestionType, header.QuestionSubType)).ToArray());
            }

            return this.ConvertAnswerToString(answer, header.QuestionType, header.QuestionSubType);
        }

        private string[] BuildAnswerListForQuestionByHeader(object answer, ExportedQuestionHeaderItem header)
        {
            if (header.ColumnHeaders.Count == 1)
                return new string[] { this.ConvertAnswerToStringValue(answer, header) };

            var result = new string[header.ColumnHeaders.Count];

            var answersAsEnumerable = this.TryCastToEnumerable(answer);
            var answers = answersAsEnumerable?.ToArray() ?? new object[] { answer };

            if (header.QuestionType == QuestionType.MultyOption)
            {
                if (!header.QuestionSubType.HasValue)
                {
                    FillMultioptionAnswers(answers, header, result);
                }
                else
                {
                    if (header.QuestionSubType.Value == QuestionSubtype.MultyOption_YesNo)
                    {
                        FillYesNoAnswers(answers, header, result, false);
                    }
                    else if (header.QuestionSubType.Value == QuestionSubtype.MultyOption_YesNoOrdered)
                    {
                        FillYesNoAnswers(answers, header, result, true);
                    }
                    else if (header.QuestionSubType.Value == QuestionSubtype.MultyOption_Ordered)
                    {
                        FillMultioptionOrderedAnswers(answers, header, result);
                    }
                    else
                    {
                        this.PutAnswersAsStringValuesIntoResultArray(answers, header, result);
                    }
                }
            }
            else
            {
                this.PutAnswersAsStringValuesIntoResultArray(answers, header, result);
            }


            return result;
        }

        private static void FillMultioptionAnswers(object[] answers, ExportedQuestionHeaderItem header, string[] result)
        {
            bool isMissingQuestion = answers.Length == 0;

            for (int i = 0; i < result.Length; i++)
            {
                if (isMissingQuestion)
                {
                    result[i] = ExportFormatSettings.MissingNumericQuestionValue;
                }
                else
                {
                    int checkedOptionIndex = Array.IndexOf(answers, header.ColumnValues[i]);
                    result[i] = checkedOptionIndex > -1 ? "1" : "0";
                }
            }
        }

        private static void FillMultioptionOrderedAnswers(object[] answers, ExportedQuestionHeaderItem header, string[] result)
        {
            bool isMissingQuestion = answers.Length == 0;

            for (int i = 0; i < result.Length; i++)
            {
                if (isMissingQuestion)
                {
                    result[i] = ExportFormatSettings.MissingNumericQuestionValue;
                }
                else
                {
                    int checkedOptionIndex = Array.IndexOf(answers, header.ColumnValues[i]);
                    result[i] = checkedOptionIndex > -1 ? (checkedOptionIndex + 1).ToString(ExportCulture) : "0";
                }
            }
        }

        private void PutAnswersAsStringValuesIntoResultArray(object[] answers, ExportedQuestionHeaderItem header, string[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = answers.Length > i ? this.ConvertAnswerToStringValue(answers[i], header) : ExportFormatSettings.MissingStringQuestionValue;
            }
        }

        private static void FillYesNoAnswers(object[] answers, ExportedQuestionHeaderItem header, string[] result, bool ordered)
        {
            AnsweredYesNoOption[] typedAnswers = answers.Cast<AnsweredYesNoOption>().ToArray();
            for (int i = 0; i < result.Length; i++)
            {
                decimal columnValue = header.ColumnValues[i];

                var selectedOption = typedAnswers.FirstOrDefault(x => x.OptionValue == columnValue);

                if (selectedOption != null)
                {
                    if (selectedOption.Yes)
                    {
                        if (ordered)
                        {
                            var selectedItemIndex = typedAnswers
                                .Where(x => x.Yes)
                                .Select((item, index) => new { item, index })
                                .FirstOrDefault(x => x.item.OptionValue == columnValue);
                            result[i] = (selectedItemIndex.index + 1).ToString(ExportCulture);
                        }
                        else
                        {
                            result[i] = "1";
                        }
                    }
                    else
                    {
                        result[i] = "0";
                    }
                }
                else
                {
                    result[i] = ExportFormatSettings.MissingNumericQuestionValue;
                }
            }
        }

        private string ConvertAnswerToString(object obj, QuestionType questionType, QuestionSubtype? questionSubType)
        {
            if (obj is IFormattable formattable)
            {
                var isDateTimeQuestion = questionType == QuestionType.DateTime;
                var isTimestampQuestion = isDateTimeQuestion && questionSubType.HasValue && questionSubType == QuestionSubtype.DateTime_Timestamp;

                return formattable.ToString(isTimestampQuestion ? ExportFormatSettings.ExportDateTimeFormat : isDateTimeQuestion ? ExportFormatSettings.ExportDateFormat : null, ExportCulture);
            }

            if (questionType == QuestionType.Audio)
                return ((AudioAnswer)obj)?.FileName;

            var answersSeparator = ExportFileSettings.NotReadableAnswersSeparator.ToString();

            if (questionType == QuestionType.TextList)
            {
                if (obj is String ans) return ans;
                
                return ((InterviewTextListAnswer)obj)?.Answer.Replace(answersSeparator, String.Empty);
            }

            return obj.ToString().Replace(answersSeparator, string.Empty);
        }
    }

}
