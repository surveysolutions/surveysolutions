using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class QuestionnaireLevelDatabaseTable
    {
        private const int ColumnsLimit = 1400;
        private const int DataMaximumSizeLimit = 7800; //https://www.postgresql.org/docs/current/limits.html

        public Guid Id { get; set; }
        public bool IsRoster { get; set; }
        public string TableName { get; set; } = String.Empty;
        public string EnablementTableName { get; set; } = String.Empty;
        public string ValidityTableName { get; set; } = String.Empty;
        public List<IQuestionnaireEntity> DataColumns { get; set; } = new List<IQuestionnaireEntity>();
        public List<IQuestionnaireEntity> EnablementColumns { get; set; } = new List<IQuestionnaireEntity>();
        public List<IQuestionnaireEntity> ValidityColumns { get; set; } = new List<IQuestionnaireEntity>();
        public List<IQuestionnaireEntity> Entities { get; set; } = new List<IQuestionnaireEntity>();

        private int rowDataSize = 0;
        
        public bool CanAddChildren(Group group)
        {
            var overDataColumnsCount = ColumnsLimit - DataColumns.Count;
            var overEnablementColumnsCount = ColumnsLimit - EnablementColumns.Count;
            var overValidityColumnsCount = ColumnsLimit - ValidityColumns.Count;
            var columnsDataSize = DataMaximumSizeLimit - rowDataSize;
            
            foreach (var entity in group.Children)
            {
                switch (entity)
                {
                    case Question question:
                        overDataColumnsCount--;
                        overEnablementColumnsCount--;
                        overValidityColumnsCount--;
                        columnsDataSize -= GetDataRowSize(question);
                        break;
                    case Variable variable:
                        overDataColumnsCount--;
                        overEnablementColumnsCount--;
                        columnsDataSize -= GetDataRowSize(variable);
                        break;
                    case StaticText staticText:
                        overEnablementColumnsCount--;
                        overValidityColumnsCount--;
                        break;
                    case Group { IsRoster: false }:
                        overEnablementColumnsCount--;
                        break;
                }

                if (overDataColumnsCount <= 0
                    || overEnablementColumnsCount <= 0
                    || overValidityColumnsCount <= 0
                    || columnsDataSize <= 0)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private static int GetDataRowSize(Question question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text: /* text */ return 24;
                case QuestionType.Numeric: return ((NumericQuestion)question).IsInteger ? /* int */ 4 : /* double */ 8;
                case QuestionType.TextList: /* json */ return 24;
                case QuestionType.MultyOption: return question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue
                    ? /* jsonb */ 24
                    : ((MultyOptionsQuestion)question).YesNoView ? /* jsonb */ 24 : /* int array */ 24;
                case QuestionType.SingleOption: return question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue
                    ? /* int array */ 24
                    : /* int */ 4;
                case QuestionType.Area: /* jsonb */ return 24; 
                case QuestionType.Audio: /* jsonb */ return 24; 
                case QuestionType.DateTime: /* timestamp */ return 8; 
                case QuestionType.GpsCoordinates: /* jsonb */ return 24; 
                case QuestionType.Multimedia: /* text */ return 24; 
                case QuestionType.QRBarcode: /* text */ return 24; 
                default:
                    throw new Exception("Unknown question type: " + question.QuestionType);
            }
        }

        private static int GetDataRowSize(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Boolean: /* bool */ return 2;
                case VariableType.Double: /* double */ return 8;
                case VariableType.String: /* text */ return 24;
                case VariableType.DateTime: /* timestamp */ return 8;
                case VariableType.LongInteger: /* int64 */ return 8;
                default:
                    throw new Exception("Unknown variable type: " + variable.Type);
            }
        }

        public void AddChildren(Group group)
        {
            if (group.Children.Count() > ColumnsLimit)
                throw new ArgumentException($"Group {group.PublicKey} ({group.Children.Count()} children) fail to insert in table with {Entities.Count} entities. Max allowed {ColumnsLimit}");

            Entities.Add(group);

            if (!(group is QuestionnaireDocument))
            {
                EnablementColumns.Add(group);
            }


            foreach (var child in group.Children)
            {
                if (!child.IsExportable) continue;

                switch (child)
                {
                    case Question question:
                        DataColumns.Add(question);
                        EnablementColumns.Add(question);
                        ValidityColumns.Add(question);
                        Entities.Add(child);
                        rowDataSize += GetDataRowSize(question);
                        break;
                    case Variable variable:
                        DataColumns.Add(variable);
                        EnablementColumns.Add(variable);
                        Entities.Add(child);
                        rowDataSize += GetDataRowSize(variable);
                        break;
                    case StaticText staticText:
                        EnablementColumns.Add(staticText);
                        ValidityColumns.Add(staticText);
                        Entities.Add(child);
                        break;
                }
            }
        }
    }
}
