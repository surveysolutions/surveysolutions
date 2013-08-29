namespace Core.Supervisor.Views.Survey
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenNavigationView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenNavigationView"/> class.
        /// </summary>
        /// <param name="menu">
        /// The menu.
        /// </param>
        /// <param name="navigation">
        /// The navigation.
        /// </param>
        public ScreenNavigationView(IEnumerable<DetailsMenuItem> menu, ScreenNavigation navigation)
        {
            this.Menu = menu.ToArray();
            this.NavigationContent = navigation;
        }

        /// <summary>
        /// Gets or sets the menu.
        /// </summary>
        public DetailsMenuItem[] Menu { get; set; }

        /// <summary>
        /// Gets or sets NavigationContent.
        /// </summary>
        public ScreenNavigation NavigationContent { get; set; }
    }
}
