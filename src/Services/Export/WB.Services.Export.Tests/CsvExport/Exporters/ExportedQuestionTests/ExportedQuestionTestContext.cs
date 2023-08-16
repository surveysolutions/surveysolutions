using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Interview;
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
            int columnIndex = 0;
            var columnValues = columnNames.Select(v => columnIndex++).ToArray();
            ExportedQuestionHeaderItem headerItem = new ExportedQuestionHeaderItem
            {
                QuestionType = questionType,
                QuestionSubType = questionSubType,
                ColumnHeaders = Create.ColumnHeaders(columnNames),
                ColumnValues = columnValues,
            };

            InterviewEntity interviewQuestion = new InterviewEntity
            {
                IsEnabled = !isDisabled
            };

            switch (questionType)
            {
                case QuestionType.MultyOption:
                    if (questionSubType == QuestionSubtype.MultiOptionYesNo || questionSubType == QuestionSubtype.MultiOptionYesNoOrdered)
                    {
                        interviewQuestion.AsYesNo = (AnsweredYesNoOption[]) value;
                    }
                    else
                    {
                        interviewQuestion.AsIntArray = (int[]) value;
                    }
                    headerItem.ColumnHeaders = Create.ColumnHeaders(columnNames, ExportValueType.NumericInt);
                    break;
                case QuestionType.Numeric:
                    interviewQuestion.AsInt = (int?) value;
                    headerItem.ColumnHeaders = Create.ColumnHeaders(columnNames, ExportValueType.NumericInt);
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
                    interviewQuestion.AsInt = (int?) value;
                    headerItem.ColumnHeaders = Create.ColumnHeaders(columnNames, ExportValueType.NumericInt);
                    break;
                case QuestionType.GpsCoordinates:
                    interviewQuestion.AsGps = (GeoPosition)value;
                    headerItem.ColumnHeaders = new List<HeaderColumn>
                    {
                        Create.ColumnHeader(columnNames[0], ExportValueType.NumericInt),
                        Create.ColumnHeader(columnNames[1], ExportValueType.NumericInt),
                        Create.ColumnHeader(columnNames[2], ExportValueType.NumericInt),
                        Create.ColumnHeader(columnNames[3], ExportValueType.NumericInt),
                        Create.ColumnHeader(columnNames[4], ExportValueType.String),
                    };
                    break;
                case QuestionType.Area:
                    interviewQuestion.AsArea = (Area)value;
                    headerItem.ColumnHeaders = new List<HeaderColumn>
                    {
                        Create.ColumnHeader(columnNames[0], ExportValueType.String),
                        Create.ColumnHeader(columnNames[1], ExportValueType.Numeric),
                        Create.ColumnHeader(columnNames[2], ExportValueType.Numeric),
                        Create.ColumnHeader(columnNames[3], ExportValueType.NumericInt),
                        Create.ColumnHeader(columnNames[4], ExportValueType.Numeric),
                        Create.ColumnHeader(columnNames[5], ExportValueType.Numeric),
                    };
                    break;
                case QuestionType.Audio:
                    interviewQuestion.AsAudio = (AudioAnswer) value;
                    break;
                case QuestionType.TextList:
                    interviewQuestion.AsList = (InterviewTextListAnswer[]) value;
                    break;
            }

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
