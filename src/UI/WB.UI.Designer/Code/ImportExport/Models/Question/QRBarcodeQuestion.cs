using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class QRBarcodeQuestion : AbstractQuestion
    {
        public override QuestionType QuestionType
        {
            get
            {
                return QuestionType.QRBarcode;
            }
            set
            {
            }
        }
    }
}
