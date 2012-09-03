using System;
using System.Reflection;

namespace Browsing.CAPI.Containers
{
    public partial class CAPISettings : Browsing.Common.Containers.Settings
    {
        public CAPISettings(Browsing.Common.Containers.ScreenHolder holder)
            : base(holder)
        {
            InitializeComponent();
        }

        protected override void BindDefaultUrl(System.Windows.Forms.TextBox defaultUrlTextBox)
        {
            defaultUrlTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", Properties.Settings.Default, "EndpointExportPath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            defaultUrlTextBox.Text = Properties.Settings.Default.EndpointExportPath;
        }

        protected override void ReloadSettings()
        {
            Properties.Settings.Default.Reload();
        }

        protected override void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
    }
}
