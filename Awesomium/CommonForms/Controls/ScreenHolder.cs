using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Browsing.Common.Containers;

namespace Browsing.Common.Controls
{
    public class ScreenHolder : Panel
    {
        public ScreenHolder()
        {
            this.loadedScreens = new List<Containers.Screen>();

        }
        public ScreenHolder(IList<Containers.Screen> screens)
        {
            this.loadedScreens = screens;
        }

        public void Redirect(Containers.Screen screen)
        {
            if (!this.LoadedScreens.Contains(screen))
                throw new ArgumentException("screen wasn't loaded in holder");
            this.Controls.Clear();
            screen.AutoSize = true;
            screen.Dock = System.Windows.Forms.DockStyle.Fill;
            screen.Name = screen.Name;

            this.Controls.Add(screen);

            screen.ValidateContent();
        }

        private IList<Containers.Screen> loadedScreens;

        public IList<Containers.Screen> LoadedScreens
        {
            get { return this.loadedScreens; }
        }
    }
}
