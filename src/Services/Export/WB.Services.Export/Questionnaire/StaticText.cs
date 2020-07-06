using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class StaticText : IValidatableQuestionnaireEntity
    {
        private IQuestionnaireEntity? parent;

        public StaticText()
        {
            Children = new List<IQuestionnaireEntity>();
            ValidationConditions = new List<ValidationCondition>();
        }

        public Guid PublicKey { get; set; }

        public IEnumerable<IQuestionnaireEntity> Children { get; }

        public IQuestionnaireEntity? GetParent()
        {
            return parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.parent = parent;
        }

        public IList<ValidationCondition> ValidationConditions { get; set; }

        private string? columnName;
        public string ColumnName
        {
            get
            {
                if (columnName != null) return columnName;
                columnName = this.PublicKey.ToString().ToLower();
                if (string.IsNullOrEmpty(columnName))
                    throw new ArgumentException($"Column name cant be empty. Entity: {PublicKey}");
                return columnName;
            }
        }
    }
}
