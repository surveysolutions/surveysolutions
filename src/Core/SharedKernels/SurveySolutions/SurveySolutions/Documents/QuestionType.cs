﻿using System;

namespace Main.Core.Entities.SubEntities
{
    public enum QuestionType
    {
        SingleOption = 0,

        [Obsolete] YesNo = 1,

        [Obsolete] DropDownList = 2,

        MultyOption = 3,

        Numeric = 4,

        DateTime = 5,

        GpsCoordinates = 6,

        Text = 7,

        [Obsolete] AutoPropagate = 8,

        TextList = 9,

        QRBarcode = 10,

        Multimedia = 11
    }
}