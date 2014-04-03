namespace Core.Supervisor.Views.Survey
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.View.Group;

    public class ScreenNavigationView
    {
        public ScreenNavigationView(IEnumerable<DetailsMenuItem> menu, ScreenNavigation navigation)
        {
            this.Menu = menu.ToArray();
            this.NavigationContent = navigation;
        }

        public DetailsMenuItem[] Menu { get; set; }
        public ScreenNavigation NavigationContent { get; set; }
    }
}
