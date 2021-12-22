using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities.Question
{
    public class QRBarcodeQuestion : AbstractQuestion, IQRBarcodeQuestion {


        public override QuestionType QuestionType => QuestionType.QRBarcode;
    }
}
