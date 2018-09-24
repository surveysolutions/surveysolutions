using System;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    [NUnit.Framework.TestOf(typeof(ExportQuestionService))]
    public class ExportedQuestionTestContext
    {
        public static string MissingNumericQuestionValue => ExportFormatSettings.MissingNumericQuestionValue;
        public static string MissingStringQuestionValue => ExportFormatSettings.MissingStringQuestionValue;
        public static string DisableValue => ExportFormatSettings.DisableValue;

        private static string[] CreateExportedQuestion(QuestionType questionType, 
            object value,
            string[] columnNames = null,
            QuestionSubtype ? questionSubType = null,
            bool isDisabled = false
            )
        {
            InterviewEntity interviewQuestion = new InterviewEntity
            {
                IsEnabled = !isDisabled
            };

            switch (questionType)
            {
                case QuestionType.MultyOption:
                    if (questionSubType == QuestionSubtype.MultyOption_YesNo || questionSubType == QuestionSubtype.MultyOption_YesNoOrdered)
                    {
                        interviewQuestion.AsYesNo = (AnsweredYesNoOption[]) value;
                    }
                    else
                    {
                        interviewQuestion.AsIntArray = (int[]) value;
                    }
                    break;
                case QuestionType.Numeric:
                    interviewQuestion.AsInt = (int?) value;
                    break;
                case QuestionType.DateTime:
                    interviewQuestion.AsDateTime = (DateTime?) value;
                    break;
                case QuestionType.Multimedia:
                    interviewQuestion.AsString = (string) value;
                    break;
                case QuestionType.QRBarcode:
                    interviewQuestion.AsString = (string) value;
                    break;
                case QuestionType.Text:
                    interviewQuestion.AsString = (string) value;
                    break;
                case QuestionType.SingleOption:
                    interviewQuestion.AsString = (string) value;
                    break;
                case QuestionType.GpsCoordinates:
                    interviewQuestion.AsGps = (GeoPosition) value;
                    break;
            }

            int columnIndex = 0;
            var columnValues = columnNames.Select(v => columnIndex++).ToArray();
            ExportedQuestionHeaderItem headerItem = new ExportedQuestionHeaderItem
            {
                QuestionType = questionType,
                QuestionSubType = questionSubType,
                ColumnHeaders = Create.ColumnHeaders(columnNames),
                ColumnValues = columnValues,
            };
            return new ExportQuestionService().GetExportedQuestion(interviewQuestion, headerItem);
        }

        public static string[] CreateFilledExportedQuestion(QuestionType questionType,
            object value,
            QuestionSubtype? questionSubType = null)
            => CreateExportedQuestion(questionType, value, new[] {"single column"}, questionSubType, false);

        public static string[] CreateFilledExportedQuestion(QuestionType questionType,
            int columnsCount,
            object value,
            QuestionSubtype? questionSubType = null)
        {
            var columnNames = Enumerable.Repeat("column", columnsCount).ToArray();
            return CreateExportedQuestion(questionType, value, columnNames, questionSubType, false);
        }

        public static string[] CreateDisabledExportedQuestion(QuestionType questionType, 
            QuestionSubtype? questionSubType = null,
            int columnsCount = 1)
        {
            var columnNames = Enumerable.Repeat("column", columnsCount).ToArray();
            return CreateExportedQuestion(questionType, null, columnNames, questionSubType, true);
        }

        public static string[] CreateMissingValueExportedQuestion(QuestionType questionType,
            QuestionSubtype? questionSubType = null,
            int columnsCount = 1)
        {
            var columnNames = Enumerable.Repeat("column", columnsCount).ToArray();
            return CreateExportedQuestion(questionType, null, columnNames, questionSubType);
        }
    }
}
