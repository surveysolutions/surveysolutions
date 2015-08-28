using System;

namespace Main.Core.Entities.SubEntities
{
    public enum QuestionType
    {
        SingleOption = 0,

        [Obsolete("should be removed after db check")]
        YesNo = 1,

        [Obsolete("should be removed after db check")]
        DropDownList = 2,

        MultyOption = 3,

        Numeric = 4,

        DateTime = 5,

        GpsCoordinates = 6,

        Text = 7,

        [Obsolete("should be removed after db check")]
        AutoPropagate = 8,

        TextList = 9,

        QRBarcode = 10,

        Multimedia = 11
    }
}