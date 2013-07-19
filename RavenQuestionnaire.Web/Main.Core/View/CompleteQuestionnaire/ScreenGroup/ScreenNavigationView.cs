using System.Collections.Generic;
using System.Linq;
using Main.Core.View.Group;

namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
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
