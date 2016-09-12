using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class ExportedQuestion
    {
        private static CultureInfo exportCulture = CultureInfo.InvariantCulture;
        public const string ExportDateTimeFormat = "yyyy-MM-ddTHH:mm:ssK";
        public const string ExportDateFormat = "yyyy-MM-dd";
        private const string DefaultDelimiter = "|";
        internal const string DisableQuestionValue = "";
        internal const string MissingNumericQuestionValue = "-999999999";
        internal const string MissingStringQuestionValue = "##N/A##";

        public ExportedQuestion()
        {
        }

        public ExportedQuestion(InterviewQuestion question, ExportedHeaderItem header)
        {
            this.QuestionType = header.QuestionType;
            this.Answers = this.GetAnswers(question, header);

            if (this.Answers.Length != header.ColumnNames.Length)
                throw new InvalidOperationException(
                    string.Format("something wrong with export logic, answer's count is less then required by template. Was '{0}', expected '{1}'",
                                  this.Answers.Length, 
                                  header.ColumnNames.Length));
        }

        private QuestionType QuestionType { get; }

        public virtual int Id { get; protected set; }
        public virtual string[] Answers { get; set; }

        private string[] GetAnswers(InterviewQuestion question, ExportedHeaderItem header)
        {
            if (question == null)
                return BuildMissingValueAnswer(header);

            if (question.IsDisabled())
                return header.ColumnNames.Select(c => DisableQuestionValue).ToArray();

            if (question.Answer == null)
                return BuildMissingValueAnswer(header);

            var gpsQuestion = question.Answer as GeoPosition;
            if (gpsQuestion != null)
            {
                return new[]
                {
                    gpsQuestion.Latitude.ToString(exportCulture),
                    gpsQuestion.Longitude.ToString(exportCulture),
                    gpsQuestion.Accuracy.ToString(exportCulture),
                    gpsQuestion.Altitude.ToString(exportCulture),
                    gpsQuestion.Timestamp.DateTime.ToString(ExportDateTimeFormat, exportCulture)
                };
            }

            switch (header.QuestionType)
            {
                case QuestionType.DateTime:
                case QuestionType.Multimedia:
                case QuestionType.Numeric:
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                    return new string[] { this.ConvertAnswerToStringValue(question.Answer, header) };

                case QuestionType.MultyOption:
                case QuestionType.SingleOption:
                case QuestionType.TextList:
                    return this.BuildAnswerListForQuestionByHeader(question.Answer, header);
                default:
                    return new string[0];
            }
            
        }

        private static string[] BuildMissingValueAnswer(ExportedHeaderItem header)
        {
            if (header.QuestionType == QuestionType.GpsCoordinates)
                return new[] { MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingStringQuestionValue };

            string missingValue = (header.QuestionType == QuestionType.Numeric
                                  || header.QuestionType == QuestionType.SingleOption
                                  || header.QuestionType == QuestionType.MultyOption
                                  || header.QuestionType == QuestionType.YesNo)
                    ? MissingNumericQuestionValue
                    : MissingStringQuestionValue;
            return header.ColumnNames.Select(c => missingValue).ToArray();
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

            var interviewTextListAnswer = value as InterviewTextListAnswers;
            if (interviewTextListAnswer != null)
            {
                return interviewTextListAnswer.Answers.Select(a => a.Answer).ToArray();
            }

            var listOfAnswers = value as IEnumerable<object>;
            return listOfAnswers;
        }

        private string ConvertAnswerToStringValue(object answer, ExportedHeaderItem header)
        {
            if (answer == null)
                return MissingStringQuestionValue;

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
                            ? string.Join(DefaultDelimiter, arrayOfAnswers.Select(x => this.ConvertAnswerToString(x, header.QuestionSubType)).ToArray())
                            : this.ConvertAnswerToString(shrinkedArrayOfAnswers[0], header.QuestionSubType);
                    }

                    return string.Format("[{0}]", string.Join(DefaultDelimiter, shrinkedArrayOfAnswers.Select(x => this.ConvertAnswerToString(x, header.QuestionSubType)).ToArray()));
                }
                return string.Join(DefaultDelimiter, arrayOfObject.Select(x => this.ConvertAnswerToString(x, header.QuestionSubType)).ToArray());
            }
            
            return this.ConvertAnswerToString(answer, header.QuestionSubType);
        }


        private string[] BuildAnswerListForQuestionByHeader(object answer, ExportedHeaderItem header)
        {
            if (header.ColumnNames.Length == 1)
                return new string[] { ConvertAnswerToStringValue(answer, header)};

            var result = new string[header.ColumnNames.Length];

            var ansresAsEnumerable = this.TryCastToEnumerable(answer);
            var answers = ansresAsEnumerable?.ToArray() ?? new object[] { answer };

            if (this.QuestionType == QuestionType.MultyOption)
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

        private static void FillMultioptionAnswers(object[] answers, ExportedHeaderItem header, string[] result)
        {
            bool isMissingQuestion = answers.Length == 0;

            for (int i = 0; i < result.Length; i++)
            {
                if (isMissingQuestion)
                {
                    result[i] = MissingNumericQuestionValue;
                }
                else
                {
                    int checkedOptionIndex = Array.IndexOf(answers, header.ColumnValues[i]);
                    result[i] = checkedOptionIndex > -1 ? "1" : "0";
                }
            }
        }

        private static void FillMultioptionOrderedAnswers(object[] answers, ExportedHeaderItem header, string[] result)
        {
            bool isMissingQuestion = answers.Length == 0;

            for (int i = 0; i < result.Length; i++)
            {
                if (isMissingQuestion)
                {
                    result[i] = MissingNumericQuestionValue;
                }
                else
                {
                    int checkedOptionIndex = Array.IndexOf(answers, header.ColumnValues[i]);
                    result[i] = checkedOptionIndex > -1 ? (checkedOptionIndex + 1).ToString(exportCulture) : "0";
                }
            }
        }

        private void PutAnswersAsStringValuesIntoResultArray(object[] answers, ExportedHeaderItem header, string[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = answers.Length > i ? this.ConvertAnswerToStringValue(answers[i], header) : MissingStringQuestionValue;
            }
        }

        private static void FillYesNoAnswers(object[] answers, ExportedHeaderItem header, string[] result, bool ordered)
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
                                .Select((item, index) => new {item, index})
                                .FirstOrDefault(x => x.item.OptionValue == columnValue);
                            result[i] = (selectedItemIndex.index + 1).ToString(exportCulture);
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
                    result[i] = MissingNumericQuestionValue;
                }
            }
        }

        private string ConvertAnswerToString(object obj, QuestionSubtype? questionSubType)
        {
            var formattable = obj as IFormattable;
            if (formattable != null)
            {
                var isDateTimeQuestion = this.QuestionType == QuestionType.DateTime;
                var isTimestampQuestion = isDateTimeQuestion && questionSubType.HasValue && questionSubType == QuestionSubtype.DateTime_Timestamp;

                return formattable.ToString(isTimestampQuestion ? ExportDateTimeFormat : isDateTimeQuestion ? ExportDateFormat : null, exportCulture);
            }
            return  obj.ToString();
        }
    }
}
    