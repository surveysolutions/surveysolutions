using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.Model;
using DevExpress.RealtorWorld.Xpf.View;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using DevExpress.Utils;
using DevExpress.Xpf.Core;

namespace DevExpress.RealtorWorld.Xpf {
    public class Program {
        [STAThread]
        static void Main(string[] args) {
            StartupBase.Run<Startup>(null);
        }
    }
    public class Startup : StartupBase {
        protected override Application CreateApplication(Application app) {
            app = base.CreateApplication(app);
            if(app == null) return null;
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = AssemblyHelper.GetResourceUri(typeof(Startup).Assembly, "Themes/Common.xaml") });
            return app;
        }
        protected override bool DoStartup() {
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(200));
            if(!base.DoStartup()) return false;
            Theme metropolisDarkTheme = GetTheme("MetropolisDark");
            ThemeManager.ApplicationThemeName = metropolisDarkTheme.Name;
            Title = "Realtor World";
            Icon = ImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(Startup).Assembly, "Images/AppIcon.ico"));
            Width = 1300.0;
            Height = 730.0;
            MinWidth = 1000.0;
            MinHeight = 600.0;
            ExitAtRequest = true;
            SetCultureInfo();
            DataSource.Current = new FilesDataSource();
            WaitScreenHelperBase.Current = new WaitScreenHelper<WaitWindow>();
            ModulesRegistration.RegisterModules();
            ViewsRegistration.RegisterViews();
            return true;
        }
        protected override UIElement CreateMainElement() {
#if CLICKONCE&&DEBUG
            MessageBox.Show("Is's Time To Debug!!!");
#endif
            MainPage mainPage = new MainPage();
            MainData data = new MainData();
            mainPage.DataContext = ModulesManager.CreateModule(null, data, null);
            return mainPage;
        }
        protected override Window CreateMainWindow() {
            return new MainWindow();
        }
        void SetCultureInfo() {
            CultureInfo demoCI = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            demoCI.NumberFormat.CurrencySymbol = "$";
            Thread.CurrentThread.CurrentCulture = demoCI;
        }
        Theme GetTheme(string name) {
            Theme theme = Theme.FindTheme(name);
            if(theme == null) {
                theme = new Theme(name) { IsStandard = true };
                Theme.RegisterTheme(theme);
            }
            return theme;
        }
    }
}
