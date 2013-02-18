using System;
using System.Reflection;
using System.Windows.Forms;
using Browsing.Common.Controls;
using Common;

namespace Browsing.Common.Containers
{
    public abstract partial class Settings : Screen
    {
        #region Members

        private bool modified = false;

        #endregion

        #region c-tor

        public Settings(ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            ContentPanel.Controls.Add(this.tableLayoutPanel1);

            this.Text = AssemblyTitle;
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;

            BindDefaultUrl(this.labelEndPoint, this.textEndPoint);

            ValidateUrl();

            this.btnCancel.Enabled = false;

        }

        #endregion

        #region Abstract and Virtual

        protected internal abstract void BindDefaultUrl(Label labelEndPoint,TextBox defaultUrlTextBox);
        protected internal abstract void ReloadSettings();
        protected internal abstract void SaveSettings();

        #endregion

        #region Helpers

        private string CompileVersionInfo()
        {
            var version = GetMainAssembly().GetName().Version;
            var time = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);

            return version
                 + " "
                 + ": " + time.Date.Year.ToString()
                 + "/" + time.Date.Month.ToString()
                 + "/" + time.Date.Day.ToString();

            //return GetMainAssembly().GetName().Version.ToString();
        }

        private Assembly GetMainAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ReloadSettings();
            //Properties.Settings.Default.Reload();

            this.btnCancel.Enabled = false;
            this.modified = false;
        }

        private void ValidateUrl()
        {
            this.textEndPoint.ForeColor =
                (string.IsNullOrEmpty(this.textEndPoint.Text) || UrlValidator.IsValid(this.textEndPoint.Text)) ?
                System.Drawing.Color.Black : System.Drawing.Color.Red;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.modified = true;
            this.btnCancel.Enabled = true;

            ValidateUrl();
        }

        #endregion

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = GetMainAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(GetMainAssembly().CodeBase);
            }
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
                object[] attributes = GetMainAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
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
                object[] attributes = GetMainAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
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
                object[] attributes = GetMainAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
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
                object[] attributes = GetMainAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

        #region Overriden

        protected override void OnHomeButtonClick(object sender, EventArgs e)
        {
            if (this.modified)
            {
                SaveSettings();
                //Properties.Settings.Default.Save();

                Holder.UpdateConfigDependencies();
            }

            this.btnCancel.Enabled = false;
            this.modified = false;

            base.OnHomeButtonClick(sender, e);
        }

        #endregion
    }
}
