// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConditional.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The Conditional interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The Conditional interface.
    /// </summary>
    public interface IConditional
    {
       /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        string ConditionExpression { get; set; }
    }
}
