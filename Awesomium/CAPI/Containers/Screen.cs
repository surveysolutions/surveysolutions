using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Browsing.CAPI.Containers
{
    public class Screen : UserControl
    {
        private readonly ScreenHolder holder;
        private System.Windows.Forms.Panel menuPanel;
        private Browsing.CAPI.Controls.FlatButton homeButton;

        #region C-tor

        public Screen(ScreenHolder holder, bool menuIsVisible)
        {
            InitializeComponent();

            this.menuPanel.Visible = menuIsVisible;

            this.holder = holder;
            this.holder.LoadedScreens.Add(this);
        }

        #endregion

        #region Protected Properties

        protected ScreenHolder Holder
        {
            get { return this.holder; }
        }

        protected Panel MenuPanel
        {
            get { return this.menuPanel; }
        }

        #endregion


        #region Helpers

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CAPIBrowser));

            this.menuPanel = new System.Windows.Forms.Panel();
            this.homeButton = new Browsing.CAPI.Controls.FlatButton();
            this.menuPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuPanel
            // 
            this.menuPanel.Controls.Add(this.homeButton);
            this.menuPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuPanel.Location = new System.Drawing.Point(0, 0);
            this.menuPanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuPanel.Name = "panel1";
            this.menuPanel.Size = new System.Drawing.Size(1440, 50);
            this.menuPanel.TabIndex = 1;
            // 
            // homeButton
            // 
            this.homeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.homeButton.BackColor = System.Drawing.Color.Transparent;
            this.homeButton.FlatAppearance.BorderSize = 0;
            this.homeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.homeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.homeButton.Image = ((System.Drawing.Image)(resources.GetObject("homeButton.Image")));
            this.homeButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.homeButton.Location = new System.Drawing.Point(2, 2);
            this.homeButton.Margin = new System.Windows.Forms.Padding(0);
            this.homeButton.Name = "homeButton";
            this.homeButton.Size = new System.Drawing.Size(100, 44);
            this.homeButton.TabIndex = 0;
            this.homeButton.Text = "Back";
            this.homeButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.homeButton.UseVisualStyleBackColor = false;
            this.homeButton.Click += new EventHandler(homeButton_Click);
            // 
            // CAPIBrowser
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.menuPanel);
            this.Name = "CAPIBrowser";
            this.Size = new System.Drawing.Size(1440, 628);
            this.menuPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        #region Handlers

        void homeButton_Click(object sender, EventArgs e)
        {
            OnHomeButtonClick(sender, e);
        }

        internal void UpdateConfigDependencies()
        {
            OnUpdateConfigDependencies();
        }

        #endregion

        #region Virtual

        protected virtual void OnHomeButtonClick(object sender, EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPIMain));
        }

        protected virtual void OnUpdateConfigDependencies()
        {
        }

        #endregion
    }
}
