// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenNavigation.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The screen navigation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The screen navigation.
    /// </summary>
    public class ScreenNavigation
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenNavigation"/> class.
        /// </summary>
        public ScreenNavigation()
        {
            this.BreadCumbs = new List<CompleteGroupHeaders>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the bread cumbs.
        /// </summary>
        public List<CompleteGroupHeaders> BreadCumbs { get; set; }

        /// <summary>
        /// Gets or sets the current screen title.
        /// </summary>
        public string CurrentScreenTitle { get; set; }

        /// <summary>
        /// Gets or sets the next screen.
        /// </summary>
        public CompleteGroupHeaders NextScreen { get; set; }

        /// <summary>
        /// Gets or sets the prev screen.
        /// </summary>
        public CompleteGroupHeaders PrevScreen { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion
    }
}