using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gat.Controls;

namespace CapiDataGenerator
{
    public class AppSettings
    {
        private static AppSettings _instance;

        public static AppSettings Instance
        {
            get { return _instance ?? (_instance = new AppSettings()); }
        }

        private AppSettings()
        {
            // Initializing Open Dialog
            var openDialog = new OpenDialogView();
            this.OpenDialog = (OpenDialogViewModel)openDialog.DataContext;
            this.OpenDialog.Owner = Application.Current.MainWindow;
            this.OpenDialog.StartupLocation = WindowStartupLocation.CenterScreen;
        }

        public OpenDialogViewModel OpenDialog { get; private set; }
    }
}
