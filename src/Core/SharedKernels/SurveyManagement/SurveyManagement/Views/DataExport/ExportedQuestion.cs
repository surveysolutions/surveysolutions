using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NHibernate.Util;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class ExportedQuestion
    {
        private static CultureInfo exportCulture = CultureInfo.InvariantCulture;
        private static string exportDatetimeFormat = "o";

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
            if (question == null || question.Answer == null || question.IsDisabled())
                return header.ColumnNames.Select(c => string.Empty).ToArray();

            var gpsQuestion = question.Answer as GeoPosition;
            if (gpsQuestion != null)
            {
                return new[]
                {
                    gpsQuestion.Latitude.ToString(exportCulture),
                    gpsQuestion.Longitude.ToString(exportCulture),
                    gpsQuestion.Accuracy.ToString(exportCulture),
                    gpsQuestion.Altitude.ToString(exportCulture),
                    gpsQuestion.Timestamp.DateTime.ToString(exportDatetimeFormat, exportCulture)
                };
            }

            if (header.ColumnNames.Length == 1)
                return new string[] { this.AnswerToStringValue(question.Answer, header) };

            var listOfAnswers = this.TryCastToEnumerable(question.Answer);
            if (listOfAnswers != null)
                return this.BuildAnswerListForQuestionByHeader(listOfAnswers.ToArray(), header);

            return new string[0];
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

        private string AnswerToStringValue(object answer, ExportedHeaderItem header)
        {
            const string DefaultDelimiter = "|";
            if (answer == null)
                return string.Empty;

            var arrayOfObject = TryCastToEnumerable(answer);

            if (arrayOfObject != null && arrayOfObject.Any())
            {
                if (header.LengthOfRosterVectorWhichNeedToBeExported.HasValue)
                {
                    var shrinkedArrayOfAnswers = arrayOfObject.Skip(arrayOfObject.Count() - header.LengthOfRosterVectorWhichNeedToBeExported.Value).ToArray();

                    if (shrinkedArrayOfAnswers.Length == 1)
                    {
                        return ConvertToString(shrinkedArrayOfAnswers[0]);
                    }

                    return string.Format("[{0}]", string.Join(DefaultDelimiter, shrinkedArrayOfAnswers));
                }
                return string.Join(DefaultDelimiter, arrayOfObject);
            }
            
            return ConvertToString(answer);
        }


        private string[] BuildAnswerListForQuestionByHeader(object[] answers, ExportedHeaderItem header)
        {
            var result = new string[header.ColumnNames.Length];

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
                        FillYesNoAnswers(answers, header, result);
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
            for (int i = 0; i < result.Length; i++)
            {
                int checkedOptionIndex = Array.IndexOf(answers, header.ColumnValues[i]);
                result[i] = checkedOptionIndex > -1 ? (checkedOptionIndex + 1).ToString(exportCulture) : "0";
            }
        }

        private void PutAnswersAsStringValuesIntoResultArray(object[] answers, ExportedHeaderItem header, string[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = answers.Length > i ? this.AnswerToStringValue(answers[i], header) : string.Empty;
            }
        }

        private static void FillYesNoAnswers(object[] answers, ExportedHeaderItem header, string[] result)
        {
            AnsweredYesNoOption[] typedAnswers = answers.Cast<AnsweredYesNoOption>().ToArray();
            int filledYesAnswersCount = 0;
            for (int i = 0; i < result.Length; i++)
            {
                decimal columnValue = header.ColumnValues[i];

                var selectedOption = typedAnswers.FirstOrDefault(x => x.OptionValue == columnValue);

                if (selectedOption != null)
                {
                    if (selectedOption.Yes)
                    {
                        var selectedItemIndex = typedAnswers
                                .Where(x => x.Yes)
                                .Select((item, index) => new {item, index})
                                .FirstOrDefault(x => x.item.OptionValue == columnValue);

                        result[i] = (selectedItemIndex.index + 1).ToString(exportCulture);
                        filledYesAnswersCount++;
                    }
                    else
                    {
                        result[i] = "0";
                    }
                }
                else
                {
                    result[i] = "";
                }
            }
        }

        private string ConvertToString(object obj)
        {
            var formattable = obj as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(this.QuestionType == QuestionType.DateTime? exportDatetimeFormat : null, exportCulture);
            }
            return  obj.ToString();
        }
    }
}
    