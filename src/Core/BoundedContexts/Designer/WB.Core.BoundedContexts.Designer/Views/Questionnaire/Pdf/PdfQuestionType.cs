using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public enum PdfQuestionType
    {
        SingleOption = 0,

        MultyOption = 3,

        Numeric = 4,

        DateTime = 5,

        GpsCoordinates = 6,

        Text = 7,

        TextList = 9,

        QRBarcode = 10,

        Multimedia = 11,

        YesNo = 12,
    }
}