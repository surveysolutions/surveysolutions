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

        public ExpressionLocation() { }

        public ExpressionLocation(ExpressionLocationItemType itemType, 
            ExpressionLocationType expressionType,
            Guid? id,
            int? expressionPosition = null)
        {
            ItemType = itemType;
            ExpressionType = expressionType;
            Id = id ?? Guid.Empty;
            ExpressionPosition = expressionPosition;
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
        
        public static ExpressionLocation LookupTables()
            => new ExpressionLocation(
                    itemType : ExpressionLocationItemType.LookupTable,
                    expressionType : ExpressionLocationType.General,
                    id : Guid.Empty);
        

        public static ExpressionLocation Questionnaire(Guid questionnaireId)
            => new  ExpressionLocation(ExpressionLocationItemType.Questionnaire, ExpressionLocationType.General,
                 questionnaireId);
        

        public static ExpressionLocation GroupCondition(Guid groupId) 
            => new ExpressionLocation(ExpressionLocationItemType.Group, ExpressionLocationType.Condition, groupId);

        public static ExpressionLocation StaticTextCondition(Guid staticTextId) 
            => new ExpressionLocation(ExpressionLocationItemType.StaticText,ExpressionLocationType.Condition, staticTextId);

        public static ExpressionLocation LinkedQuestionFilter(Guid questionId)
            => new ExpressionLocation(ExpressionLocationItemType.Question, ExpressionLocationType.Filter, questionId);
        
        public static ExpressionLocation CategoricalQuestionFilter(Guid questionId)
            => new ExpressionLocation(ExpressionLocationItemType.Question, ExpressionLocationType.CategoricalFilter, questionId);
        
        public static ExpressionLocation Variable(Guid variableId)
            => new ExpressionLocation(ExpressionLocationItemType.Variable, ExpressionLocationType.Expression, variableId);
        
        public static ExpressionLocation QuestionValidation(Guid questionId, int? position) 
            => new ExpressionLocation(ExpressionLocationItemType.Question, ExpressionLocationType.Validation,
                questionId, position);

        public static ExpressionLocation StaticTextValidation(Guid questionId, int? position) 
            => new ExpressionLocation(ExpressionLocationItemType.StaticText, ExpressionLocationType.Validation,
                questionId, position);

        public static ExpressionLocation QuestionCondition(Guid questionId)
            => new ExpressionLocation(ExpressionLocationItemType.Question, ExpressionLocationType.Condition, questionId);
        

        public static ExpressionLocation RosterCondition(Guid rosterId)
            => new ExpressionLocation(ExpressionLocationItemType.Roster, ExpressionLocationType.Condition, rosterId);
        
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
