using System;
using System.Linq.Expressions;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Services
{
    [Serializable]
    public class ExpressionLocation
    {
        public Guid Id { set; get; }

        public ExpressionLocationItemType ItemType { set; get; }

        public ExpressionLocationType ExpressionType { set; get; }

        public ExpressionLocation()
        {
        }

        public static ExpressionLocation LookupTables()
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.LookupTable,
                ExpressionType = ExpressionLocationType.General,
                Id = Guid.Empty
            };
        }

        public static ExpressionLocation Questionnaire(Guid questionnaireId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Questionnaire,
                ExpressionType = ExpressionLocationType.General,
                Id = questionnaireId
            };
        }

        public static ExpressionLocation GroupCondition(Guid groupId)
        {
            return new  ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Group,
                ExpressionType = ExpressionLocationType.Condition,
                Id = groupId
            };
        }

        public static ExpressionLocation QuestionValidation(Guid questionId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Question,
                ExpressionType = ExpressionLocationType.Validation,
                Id = questionId
            };
        }

        public static ExpressionLocation QuestionCondition(Guid questionId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Question,
                ExpressionType = ExpressionLocationType.Condition,
                Id = questionId
            };
        }

        public static ExpressionLocation RosterCondition(Guid rosterId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Roster,
                ExpressionType = ExpressionLocationType.Condition,
                Id = rosterId
            };
        }

        public ExpressionLocation(ExpressionLocationItemType itemType = ExpressionLocationItemType.Question, ExpressionLocationType expressionType = ExpressionLocationType.General, Guid? id = null)
        {
            this.ItemType = itemType;
            this.ExpressionType = expressionType;
            this.Id = id ?? Guid.Empty;
        }

        public ExpressionLocation(string stringValue)
        {
            string[] expressionLocation = stringValue.Split(':');
            if (expressionLocation.Length != 3)
                throw new ArgumentException("stringValue");

            ItemType =
                (ExpressionLocationItemType)
                    Enum.Parse(typeof (ExpressionLocationItemType), expressionLocation[0], true);
            ExpressionType =
                (ExpressionLocationType) Enum.Parse(typeof (ExpressionLocationType), expressionLocation[1], true);
            Id = Guid.Parse(expressionLocation[2]);
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}:{2}", ItemType, ExpressionType, Id);
        }

        public string Key => this.ToString();
    }
}
