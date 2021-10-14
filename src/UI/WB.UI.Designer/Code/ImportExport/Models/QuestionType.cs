using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public enum QuestionType
    {
        SingleOption = 0,

        [Obsolete("db contains at least one questionnaire")]
        YesNo = 1,

        MultyOption = 3,

        Numeric = 4,

        DateTime = 5,

        GpsCoordinates = 6,

        Text = 7,

        [Obsolete("db contains a bunch of them")]
        AutoPropagate = 8,

        TextList = 9,

        QRBarcode = 10,

        Multimedia = 11,

        Area = 12,

        Audio = 13
    }
}