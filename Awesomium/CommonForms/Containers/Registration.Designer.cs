using System.Windows.Forms;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    partial class Registration
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.regContent = new System.Windows.Forms.TableLayoutPanel();
            this.regPanel = new Browsing.Common.Containers.RegPanel();
            this.authorizedGroupBox = new System.Windows.Forms.GroupBox();
            this.authorizationList = new System.Windows.Forms.ListView();
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDevice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.regContent.SuspendLayout();
            this.authorizedGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // registrationPanel
            // 
            this.regContent.ColumnCount = 5;
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.regContent.Controls.Add(this.regPanel, 2, 4);
            this.regContent.Controls.Add(this.authorizedGroupBox, 2, 1);
            this.regContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.regContent.Location = new System.Drawing.Point(0, 0);
            this.regContent.Margin = new System.Windows.Forms.Padding(4);
            this.regContent.Name = "registrationPanel";
            this.regContent.RowCount = 6;
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.Size = new System.Drawing.Size(794, 554);
            this.regContent.TabIndex = 1;
            // 
            // regPanel
            // 
            this.regContent.SetColumnSpan(this.regPanel, 2);
            this.regPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.regPanel.Location = new System.Drawing.Point(28, 149);
            this.regPanel.Name = "regPanel";
            this.regContent.SetRowSpan(this.regPanel, 2);
            this.regPanel.Size = new System.Drawing.Size(738, 402);
            this.regPanel.TabIndex = 3;
            // 
            // authorizedGroupBox
            // 
            this.regContent.SetColumnSpan(this.authorizedGroupBox, 2);
            this.authorizedGroupBox.Controls.Add(this.authorizationList);
            this.authorizedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authorizedGroupBox.Location = new System.Drawing.Point(28, 23);
            this.authorizedGroupBox.Name = "authorizedGroupBox";
            this.authorizedGroupBox.Size = new System.Drawing.Size(738, 100);
            this.authorizedGroupBox.TabIndex = 4;
            this.authorizedGroupBox.TabStop = false;
            this.authorizedGroupBox.Text = "Authorized devices";
            this.authorizedGroupBox.Visible = false;
            // 
            // authorizationList
            // 
            this.authorizationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderDevice,
            this.columnHeaderDate});
            this.authorizationList.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.authorizationList.GridLines = true;
            this.authorizationList.Location = new System.Drawing.Point(3, 18);
            this.authorizationList.Name = "authorizationList";
            this.authorizationList.Size = new System.Drawing.Size(732, 79);
            this.authorizationList.TabIndex = 0;
            this.authorizationList.UseCompatibleStateImageBehavior = false;
            this.authorizationList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.DisplayIndex = 1;
            this.columnHeaderDate.Text = "Authorized";
            this.columnHeaderDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderDate.Width = 100;
            // 
            // columnHeaderDevice
            // 
            this.columnHeaderDevice.DisplayIndex = 0;
            this.columnHeaderDevice.Text = "Device";
            this.columnHeaderDevice.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;            
            // 
            // Registration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.regContent);
            this.Name = "Registration";
            this.Size = new System.Drawing.Size(794, 554);
            this.Controls.SetChildIndex(this.regContent, 0);
            this.regContent.ResumeLayout(false);
            this.authorizedGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel regContent;
        private RegPanel regPanel;
        private GroupBox authorizedGroupBox;
        private ListView authorizationList;
        private ColumnHeader columnHeaderDevice;
        private ColumnHeader columnHeaderDate;
    }
}
