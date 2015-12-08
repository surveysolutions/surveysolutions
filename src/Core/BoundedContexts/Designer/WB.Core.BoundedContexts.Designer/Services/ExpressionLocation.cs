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

        public ExpressionLocation(ExpressionLocationItemType itemType = ExpressionLocationItemType.Question, ExpressionLocationType expressionType = ExpressionLocationType.General, Guid? id = null)
        {
            this.ItemType = itemType;
            this.ExpressionType = this.ExpressionType;
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
    }
}
