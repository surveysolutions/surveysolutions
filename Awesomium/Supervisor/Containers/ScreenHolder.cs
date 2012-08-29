using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Browsing.Supervisor.Containers
{
    public partial class ScreenHolder : Panel
    {

        #region Properties

        private IList<Screen> loadedScreens;

        public IList<Screen> LoadedScreens
        {
            get { return this.loadedScreens; }
        }

        #endregion

        #region Constructor

        public ScreenHolder()
        {
            this.loadedScreens = new List<Screen>();

        }

        public ScreenHolder(IList<Screen> screens)
        {
            this.loadedScreens = screens;
        }

        #endregion

        #region Methods

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

        #endregion
    }
}
