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
    /// <summary>
    /// The question type.
    /// </summary>
    public enum QuestionType
    {
        /// <summary>
        /// The single option.
        /// </summary>
        SingleOption = 0, 

        /// <summary>
        /// The yes no.
        /// </summary>
        [Obsolete]
        YesNo = 1, 

        /// <summary>
        /// The drop down list.
        /// </summary>
        [Obsolete]
        DropDownList = 2, 

        /// <summary>
        /// The multy option.
        /// </summary>
        MultyOption = 3, 

        /// <summary>
        /// The numeric.
        /// </summary>
        Numeric = 4, 

        /// <summary>
        /// The date time.
        /// </summary>
        DateTime = 5, 

        /// <summary>
        /// The gps coordinates.
        /// </summary>
        [Obsolete]
        GpsCoordinates = 6, 

        /// <summary>
        /// The text.
        /// </summary>
        Text = 7, 

        /// <summary>
        /// The auto propagate.
        /// </summary>
        AutoPropagate = 8, 
    }
}