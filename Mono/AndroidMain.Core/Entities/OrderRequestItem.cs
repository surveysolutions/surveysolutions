// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderRequestItem.cs" company="">
//   
// </copyright>
// <summary>
//   The order request item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities
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
}