using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Browsing.Common.Containers;

namespace Browsing.Common.Controls
{
    public class ScreenHolder : Panel
    {
        public ScreenHolder()
            : this(new List<Containers.Screen>())
        {
        }

        public ScreenHolder(IList<Containers.Screen> screens)
        {
            this.loadedScreens = screens;
        }

        public void Redirect(Containers.Screen screen)
        {
            if (!this.LoadedScreens.Contains(screen))
                throw new ArgumentException("screen wasn't loaded in holder");

            if (this.Controls.Count > 0)
            {
                var prevScreen = this.Controls[0] as Containers.Screen;
                if (prevScreen != null)
                    prevScreen.LeaveScreen();
            }

            this.Controls.Clear();

            screen.AutoSize = true;
            screen.Dock = System.Windows.Forms.DockStyle.Fill;
            screen.Name = screen.Name;

            this.Controls.Add(screen);

            screen.EnterScreen();
        }

        private IList<Containers.Screen> loadedScreens;

        public IList<Containers.Screen> LoadedScreens
        {
            get { return this.loadedScreens; }
        }

        public void UpdateConfigDependencies()
        {
            foreach (var screen in this.LoadedScreens)
                screen.UpdateConfigDependencies();
        }

        internal void NavigateBrowser(bool singlePage, string url)
        {
            var browser = this.LoadedScreens.FirstOrDefault(s => s is Browser) as Browser;
            browser.SetMode(singlePage, url);
            Redirect(browser);
        }

        internal void NavigateSettings()
        {
            var settings = this.LoadedScreens.FirstOrDefault(s => s is Settings) as Settings;
            Redirect(settings);
        }

        internal void NavigateRegistration()
        {
            var registration = this.LoadedScreens.FirstOrDefault(s => s is Registration) as Registration;
            Redirect(registration);
        }

        internal void NavigateSynchronization()
        {
            var synchronization = this.LoadedScreens.FirstOrDefault(s => s is Browsing.Common.Containers.Synchronization);
            Redirect(synchronization);
        }

        internal void NavigateMain()
        {
            var main = this.LoadedScreens.FirstOrDefault(s => s is Main);
            Redirect(main);
        }

        internal void AddScreen(Containers.Screen screen)
        {
            this.LoadedScreens.Add(screen);
        }
    }
}
