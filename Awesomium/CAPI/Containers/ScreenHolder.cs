using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Browsing.CAPI.Containers
{
    public class ScreenHolder : Panel
    {
        public ScreenHolder()
        {
            this.loadedScreens = new List<Screen>();

        }
        public ScreenHolder(IList<Screen> screens)
        {
            this.loadedScreens = screens;
        }

        public void Redirect(Screen screen)
        {
            if (!this.LoadedScreens.Contains(screen))
                throw new ArgumentException("screen wasn't loaded in holder");
            this.Controls.Clear();
            screen.AutoSize = true;
            screen.Dock = System.Windows.Forms.DockStyle.Fill;
            screen.Name = screen.Name;
            this.Controls.Add(screen);
        }

        private IList<Screen> loadedScreens;

        public IList<Screen> LoadedScreens
        {
            get { return this.loadedScreens; }
        }
    }
}
