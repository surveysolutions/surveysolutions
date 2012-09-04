using System;
using System.Reflection;
using Browsing.Common.Controls;
using Browsing.Common.Containers;

namespace Browsing.CAPI.Containers
{
    public partial class CAPISettings : Settings
    {
        public CAPISettings(ScreenHolder holder)
            : base(holder)
        {
            InitializeComponent();
        }

        protected override void BindDefaultUrl(System.Windows.Forms.Label labelEndPoint, System.Windows.Forms.TextBox defaultUrlTextBox)
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
