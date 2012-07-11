using System;
using QApp.ViewModel;
using System.Windows;
using System.Threading;
using DevExpress.Utils;
using DevExpress.Xpf.Core;
using System.Globalization;
using System.Windows.Threading;
using DevExpress.RealtorWorld.Xpf;
using MainPage = QApp.View.MainPage;
using System.Windows.Media.Animation;
using DevExpress.RealtorWorld.Xpf.View;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using ViewsRegistration = QApp.View.ViewsRegistration;
using ModulesRegistration = QApp.ViewModel.ModulesRegistration;

namespace QApp
{
   public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Initializer.Init();
            NCQRSInit.RebuildReadLayer();
            StartupBase.Run<QStartup>(null);
            //NCQRSInit.RebuildReadLayer();
        }
    }

    public class QStartup: StartupBase
    {
      
            protected override Application CreateApplication(Application app)
            {
                app = base.CreateApplication(app);
                if (app == null) return null;
                app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = AssemblyHelper.GetResourceUri(typeof(Startup).Assembly, "Themes/Common.xaml") });
                
                
                app.DispatcherUnhandledException += delegate(object sender, DispatcherUnhandledExceptionEventArgs args)
                                                        {
                                                            MessageBox.Show(args.Exception.Message, "Exception Caught",
                                                            MessageBoxButton.OK, MessageBoxImage.Error);
                                                            args.Handled = true;
                                                        };
                return app;
            }
            protected override bool DoStartup()
            {
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(200));
                if (!base.DoStartup()) return false;
                Theme metropolisDarkTheme = GetTheme("MetropolisDark");
                ThemeManager.ApplicationThemeName = metropolisDarkTheme.Name;
                Title = "Q";
                Icon = ImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(Startup).Assembly, "Images/AppIcon.ico"));
                Width = 1200.0;
                Height = 550.0;
                MinWidth = 400.0;
                MinHeight = 350.0;
                ExitAtRequest = true;
                SetCultureInfo();
                //DataSource.Current = new FilesDataSource();
                WaitScreenHelperBase.Current = new WaitScreenHelper<WaitWindow>();
                ModulesRegistration.RegisterModules();
                ViewsRegistration.RegisterViews();
                return true;
            }


            protected override UIElement CreateMainElement()
            {
                #if CLICKONCE&&DEBUG
                MessageBox.Show("Is's Time To Debug!!!");
                #endif

                MainPage mainPage = new MainPage();

                MainScreenData data = new MainScreenData();
                //CompletedQuestionnariesData data = new CompletedQuestionnariesData();
                mainPage.DataContext = ModulesManager.CreateModule(null, data, null);
               
                return mainPage;
            }


            protected override Window CreateMainWindow()
            {
                return new MainWindow();
            }
            void SetCultureInfo()
            {
                CultureInfo demoCI = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                demoCI.NumberFormat.CurrencySymbol = "$";
                Thread.CurrentThread.CurrentCulture = demoCI;
            }
            Theme GetTheme(string name)
            {
                Theme theme = Theme.FindTheme(name);
                if (theme == null)
                {
                    theme = new Theme(name) { IsStandard = true };
                    Theme.RegisterTheme(theme);
                }
                return theme;
            }
        }
}
