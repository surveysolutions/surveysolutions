// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnswerOrder.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The order.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    /// <summary>
    /// The order.
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// The as is.
        /// </summary>
        AsIs, 

        /// <summary>
        /// The random.
        /// </summary>
        Random, 

        /// <summary>
        /// The az.
        /// </summary>
        AZ, 

        /// <summary>
        /// The za.
        /// </summary>
        ZA, 

        /// <summary>
        /// The min max.
        /// </summary>
        MinMax, 

        /// <summary>
        /// The max min.
        /// </summary>
        MaxMin
    }
}