// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderRequestItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The order request item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    /// <summary>
    /// The order request item.
    /// </summary>
    public class OrderRequestItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public OrderDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        public string Field { get; set; }

        #endregion
    }

    /// <summary>
    /// The order direction.
    /// </summary>
    public enum OrderDirection
    {
        /// <summary>
        /// The asc.
        /// </summary>
        Asc = 0, 

        /// <summary>
        /// The desc.
        /// </summary>
        Desc = 1
    }
}