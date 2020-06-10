using System;
using System.Collections.Generic;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;

namespace WB.Services.Export.Questionnaire
{
    public abstract class Question : IValidatableQuestionnaireEntity
    {
        private IQuestionnaireEntity? parent;

        protected Question()
        {
            this.ValidationConditions = new List<ValidationCondition>();
        }

        public QuestionType QuestionType { get; set; }

        public bool Featured { get; set;  }

        public string VariableName { get; set;  } = String.Empty;

        public string VariableLabel { get; set;  } = String.Empty; 

        public string QuestionText { get; set; } = String.Empty;

        public List<Answer> Answers { get; set; } = new List<Answer>();

        public Guid? LinkedToQuestionId { get; set; }

        public Guid? LinkedToRosterId { get; set; }

        public Guid PublicKey { get; set; }

        public IEnumerable<IQuestionnaireEntity> Children { get; } = new List<IQuestionnaireEntity>();

        public IQuestionnaireEntity? GetParent()
        {
            return parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.parent = parent;
        }

        public bool IsQuestionLinked()
        {
            return LinkedToQuestionId != null || LinkedToRosterId != null;
        }

        public IList<ValidationCondition> ValidationConditions { get; set; }

        public string Instructions { get; set; } = String.Empty;

        private string? columnName;
        public string ColumnName
        {
            get
            {
                if (columnName != null) return columnName;
                columnName = PostgresSystemColumns.Escape(this.VariableName?.ToLower() ?? "");
                if (string.IsNullOrEmpty(columnName))
                    throw new ArgumentException($"Column name cant be empty. Entity: {PublicKey}");
                return columnName;
            }
        }
    }
}
