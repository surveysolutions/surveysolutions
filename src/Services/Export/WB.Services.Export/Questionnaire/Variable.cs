﻿using System;
using System.Collections.Generic;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;

namespace WB.Services.Export.Questionnaire
{
    public class Variable : IQuestionnaireEntity
    {
        public Variable()
        {
            this.Children = new List<IQuestionnaireEntity>();
        }

        public Guid PublicKey { get; set;  }

        public IEnumerable<IQuestionnaireEntity> Children { get; set;  }

        public IQuestionnaireEntity GetParent()
        {
            return parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.parent = parent;
        }

        private IQuestionnaireEntity parent;

        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Expression { get; set; }

        public bool DoNotExport { get; set; }

        private string columnName;
        public string ColumnName
        {
            get
            {
                if (columnName != null) return columnName;
                columnName = PostgresSystemColumns.Escape(this.Name?.ToLower());
                if (string.IsNullOrEmpty(columnName))
                    throw new ArgumentException($"Column name cant be empty. Entity: {PublicKey}");
                return columnName;
            }
        }
    }

    public enum VariableType
    {
        LongInteger = 1,
        Double = 2,
        Boolean = 3,
        DateTime = 4,
        String = 5
    }
}
