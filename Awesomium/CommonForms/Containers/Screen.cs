using System;
using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public class Screen : UserControl
    {
        private readonly ScreenHolder holder;
        private System.Windows.Forms.Panel menuPanel;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel contentPanel;
        private Browsing.Common.Controls.FlatButton homeButton;

        #region C-tor

        public Screen() : this(null, true)
        {
        }

        public Screen(ScreenHolder holder, bool menuIsVisible)
        {
            InitializeComponent();

            this.menuPanel.Visible = menuIsVisible;

            this.holder = holder;
            if (this.holder != null)
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

        protected Panel ContentPanel
        {
            get { return this.contentPanel; }
        }

        #endregion

        #region Helpers

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Screen));
            this.menuPanel = new System.Windows.Forms.Panel();
            this.homeButton = new Browsing.Common.Controls.FlatButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.menuPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuPanel
            // 
            this.menuPanel.Controls.Add(this.homeButton);
            this.menuPanel.Location = new System.Drawing.Point(0, 0);
            this.menuPanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuPanel.Name = "menuPanel";
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
            this.homeButton.Click += new System.EventHandler(this.homeButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.menuPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.contentPanel, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1440, 628);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // contentPanel
            // 
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(3, 53);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(1434, 572);
            this.contentPanel.TabIndex = 2;
            // 
            // Screen
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Screen";
            this.Size = new System.Drawing.Size(1440, 628);
            this.menuPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
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
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is Main));
        }

        protected virtual void OnUpdateConfigDependencies()
        {
        }

        #endregion
    }
}
