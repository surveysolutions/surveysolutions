// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandHandlerAttribute.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The command handler attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core
{
    using System;

    /// <summary>
    /// The command handler attribute.
    /// </summary>
    public class CommandHandlerAttribute : Attribute
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether ignore as event.
        /// </summary>
        public bool IgnoreAsEvent { get; set; }

        #endregion
    }
}