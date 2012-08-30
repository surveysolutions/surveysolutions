using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Browsing.CAPI.Containers
{
    public partial class CAPISettings : //UserControl
        Screen
    {
        private bool modified = false;

        public CAPISettings(ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            ContentPanel.Controls.Add(this.tableLayoutPanel1);

            this.Text = AssemblyTitle;
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;

        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        private string CompileVersionInfo()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var time = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);

            return version
                 + " "
                 + ": " + time.Date.Year.ToString()
                 + "/" + time.Date.Month.ToString()
                 + "/" + time.Date.Day.ToString();

            //return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public string AssemblyVersion
        {
            get
            {
                return CompileVersionInfo();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            this.btnCancel.Enabled = false;
            this.modified = false;
        }

        protected override void OnHomeButtonClick(object sender, EventArgs e)
        {
            if (this.modified)
            {
                Properties.Settings.Default.Save();

                foreach (var screen in this.Holder.LoadedScreens)
                    screen.UpdateConfigDependencies();
            }

            this.btnCancel.Enabled = false;
            this.modified = false;

            base.OnHomeButtonClick(sender, e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.modified = true;
            this.btnCancel.Enabled = true;
        }
    }
}
