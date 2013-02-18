// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionType.cs" company="">
//   
// </copyright>
// <summary>
//   The question type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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
        SingleOption, 

        /// <summary>
        /// The yes no.
        /// </summary>
        YesNo, 

        /// <summary>
        /// The drop down list.
        /// </summary>
        DropDownList, 

        /// <summary>
        /// The multy option.
        /// </summary>
        MultyOption, 

        /// <summary>
        /// The numeric.
        /// </summary>
        Numeric, 

        /// <summary>
        /// The date time.
        /// </summary>
        DateTime, 

        /// <summary>
        /// The gps coordinates.
        /// </summary>
        GpsCoordinates, 

        /// <summary>
        /// The text.
        /// </summary>
        Text, 

        ///// <summary>
        ///// The percentage.
        ///// </summary>
        //Percentage, 

        /// <summary>
        /// The auto propagate.
        /// </summary>
        AutoPropagate, 
    }
}