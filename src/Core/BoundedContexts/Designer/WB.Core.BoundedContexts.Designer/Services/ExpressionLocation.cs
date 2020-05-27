using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    [Serializable]
    public class ExpressionLocation
    {
        public Guid Id { set; get; }

        public ExpressionLocationItemType ItemType { set; get; }

        public ExpressionLocationType ExpressionType { set; get; }

        public int? ExpressionPosition { set; get; }

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

        public static ExpressionLocation GroupCondition(Guid groupId) => new ExpressionLocation
        {
            ItemType = ExpressionLocationItemType.Group,
            ExpressionType = ExpressionLocationType.Condition,
            Id = groupId
        };

        public static ExpressionLocation StaticTextCondition(Guid staticTextId) => new ExpressionLocation
        {
            ItemType = ExpressionLocationItemType.StaticText,
            ExpressionType = ExpressionLocationType.Condition,
            Id = staticTextId
        };

        public static ExpressionLocation LinkedQuestionFilter(Guid questionId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Question,
                ExpressionType = ExpressionLocationType.Filter,
                Id = questionId
            };
        }

        public static ExpressionLocation CategoricalQuestionFilter(Guid questionId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Question,
                ExpressionType = ExpressionLocationType.CategoricalFilter,
                Id = questionId
            };
        }

        public static ExpressionLocation Variable(Guid variableId)
        {
            return new ExpressionLocation
            {
                ItemType = ExpressionLocationItemType.Variable,
                ExpressionType = ExpressionLocationType.Expression,
                Id = variableId
            };
        }

        public static ExpressionLocation QuestionValidation(Guid questionId, int? position) => new ExpressionLocation
        {
            ItemType = ExpressionLocationItemType.Question,
            ExpressionType = ExpressionLocationType.Validation,
            Id = questionId,
            ExpressionPosition = position
        };

        public static ExpressionLocation StaticTextValidation(Guid questionId, int? position) => new ExpressionLocation
        {
            ItemType = ExpressionLocationItemType.StaticText,
            ExpressionType = ExpressionLocationType.Validation,
            Id = questionId,
            ExpressionPosition = position
        };

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

        public ExpressionLocation(string? stringValue)
        {
            if (stringValue == null) throw new ArgumentNullException(nameof(stringValue));
            
            string[] expressionLocation = stringValue.Split(':');
            if (expressionLocation.Length != 3 && expressionLocation.Length != 4)
                throw new ArgumentException("stringValue");

            ItemType = (ExpressionLocationItemType)Enum.Parse(typeof (ExpressionLocationItemType), expressionLocation[0], true);

            ExpressionType = (ExpressionLocationType) Enum.Parse(typeof (ExpressionLocationType), expressionLocation[1], true);
            Id = Guid.Parse(expressionLocation[2]);

            if (expressionLocation.Length == 4)
                ExpressionPosition = int.Parse(expressionLocation[3]);
        }

        public override string ToString()
        {
            var result = String.Format("{0}:{1}:{2}", ItemType, ExpressionType, Id);
            if(this.ExpressionPosition.HasValue)
                result = String.Format("{0}:{1}", result, this.ExpressionPosition);

            return result;
        }

        public string Key => this.ToString();
    }
}
