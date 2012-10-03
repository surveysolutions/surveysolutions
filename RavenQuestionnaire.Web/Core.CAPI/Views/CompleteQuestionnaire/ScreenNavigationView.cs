// -----------------------------------------------------------------------
// <copyright file="ScreenNavigationView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.View.Group;

namespace Core.CAPI.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenNavigationView
    {
        public ScreenNavigationView(IEnumerable<CompleteGroupHeaders> menu, ScreenNavigation navigation)
        {
            Menu = menu.ToArray();
            NavigationContent = navigation;
        }

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        public CompleteGroupHeaders[] Menu { get; set; }

        public ScreenNavigation NavigationContent { get; set; }
    }
}
