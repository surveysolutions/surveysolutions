// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionType.cs" company="">
//   
// </copyright>
// <summary>
//   The question type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Main.Core.Entities.SubEntities
{
    public enum QuestionType
    {
        SingleOption = 0, 

        [Obsolete]
        YesNo = 1, 

        [Obsolete]
        DropDownList = 2, 

        MultyOption = 3, 

        Numeric = 4, 

        DateTime = 5, 

        [Obsolete]
        GpsCoordinates = 6, 

        Text = 7, 

        AutoPropagate = 8, 
    }
}