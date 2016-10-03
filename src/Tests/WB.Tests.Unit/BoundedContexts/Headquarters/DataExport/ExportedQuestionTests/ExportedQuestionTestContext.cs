using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using QuestionState = WB.Core.SharedKernels.DataCollection.ValueObjects.Interview.QuestionState;


namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    [Subject(typeof(ExportQuestionService))]
    public class ExportedQuestionTestContext
    {
        public static string MissingNumericQuestionValue { get { return ExportFormatSettings.MissingNumericQuestionValue; } }
        public static string MissingStringQuestionValue { get { return ExportFormatSettings.MissingStringQuestionValue; } }
        public static string DisableQuestionValue { get { return ExportFormatSettings.DisableQuestionValue; } }

        private static string[] CreateExportedQuestion(QuestionType questionType, 
            object value,
            string[] columnNames = null,
            QuestionSubtype ? questionSubType = null,
            bool isDisabled = false
            )
        {
            InterviewQuestion interviewQuestion = new InterviewQuestion()
            {
                Answer = value,
                QuestionState = isDisabled ? QuestionState.Valid : QuestionState.Valid | QuestionState.Enabled,
            };
            decimal columnIndex = 0;
            var columnValues = columnNames.Select(v => columnIndex++).ToArray();
            ExportedHeaderItem headerItem = new ExportedHeaderItem()
            {
                QuestionType = questionType,
                QuestionSubType = questionSubType,
                ColumnNames = columnNames,
                ColumnValues = columnValues,
            };
            return new ExportQuestionService().GetExportedQuestion(interviewQuestion, headerItem);
        }

        public static string[] CreateFilledExportedQuestion(QuestionType questionType,
            object value,
            QuestionSubtype? questionSubType = null)
        {
            var columnNames = new []{ "single column" };
            return CreateExportedQuestion(questionType, value, columnNames, questionSubType, false);
        }

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