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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Registration));
            this.registrationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.usbStatusPanel = new Browsing.Common.Containers.UsbStatusPanel();
            this.registrationButton = new Browsing.Common.Controls.FlatButton();
            this.authorizatonGroupBox = new System.Windows.Forms.GroupBox();
            this.authorizedDevicesListView = new System.Windows.Forms.ListView();
            this.registrationPanel.SuspendLayout();
            this.authorizatonGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // registrationPanel
            // 
            this.registrationPanel.ColumnCount = 5;
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.registrationPanel.Controls.Add(this.usbStatusPanel, 1, 3);
            this.registrationPanel.Controls.Add(this.registrationButton, 3, 3);
            this.registrationPanel.Controls.Add(this.authorizatonGroupBox, 1, 1);
            this.registrationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.registrationPanel.Location = new System.Drawing.Point(0, 0);
            this.registrationPanel.Margin = new System.Windows.Forms.Padding(4);
            this.registrationPanel.Name = "registrationPanel";
            this.registrationPanel.RowCount = 6;
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.registrationPanel.Size = new System.Drawing.Size(794, 554);
            this.registrationPanel.TabIndex = 1;
            // 
            // usbStatusPanel
            // 
            this.registrationPanel.SetColumnSpan(this.usbStatusPanel, 2);
            this.usbStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usbStatusPanel.Location = new System.Drawing.Point(29, 73);
            this.usbStatusPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 20);
            this.usbStatusPanel.Name = "usbStatusPanel";
            this.registrationPanel.SetRowSpan(this.usbStatusPanel, 3);
            this.usbStatusPanel.Size = new System.Drawing.Size(536, 526);
            this.usbStatusPanel.TabIndex = 0;
            // 
            // registrationButton
            // 
            this.registrationButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.registrationButton.Enabled = false;
            this.registrationButton.FlatAppearance.BorderSize = 0;
            this.registrationButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.registrationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.registrationButton.Image = ((System.Drawing.Image)(resources.GetObject("registrationButton.Image")));
            this.registrationButton.Location = new System.Drawing.Point(572, 72);
            this.registrationButton.Name = "registrationButton";
            this.registrationButton.Size = new System.Drawing.Size(194, 200);
            this.registrationButton.TabIndex = 0;
            this.registrationButton.Text = "Register";
            this.registrationButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.registrationButton.UseVisualStyleBackColor = true;
            this.registrationButton.Click += new System.EventHandler(this.registrationButton_Click);
            // 
            // authorizatonGroupBox
            // 
            this.authorizatonGroupBox.AutoSize = true;
            this.registrationPanel.SetColumnSpan(this.authorizatonGroupBox, 2);
            this.authorizatonGroupBox.Controls.Add(this.authorizedDevicesListView);
            this.authorizatonGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authorizatonGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.authorizatonGroupBox.Location = new System.Drawing.Point(28, 23);
            this.authorizatonGroupBox.MaximumSize = new System.Drawing.Size(0, 200);
            this.authorizatonGroupBox.Name = "authorizatonGroupBox";
            this.authorizatonGroupBox.Size = new System.Drawing.Size(538, 23);
            this.authorizatonGroupBox.TabIndex = 2;
            this.authorizatonGroupBox.TabStop = false;
            this.authorizatonGroupBox.Text = "Authorized devices";
            // 
            // authorizedDevicesListView
            // 
            this.authorizedDevicesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authorizedDevicesListView.Location = new System.Drawing.Point(3, 20);
            this.authorizedDevicesListView.MaximumSize = new System.Drawing.Size(4, 200);
            this.authorizedDevicesListView.Name = "authorizedDevicesListView";
            this.authorizedDevicesListView.Size = new System.Drawing.Size(4, 0);
            this.authorizedDevicesListView.TabIndex = 1;
            this.authorizedDevicesListView.UseCompatibleStateImageBehavior = false;
            // 
            // Registration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.registrationPanel);
            this.Name = "Registration";
            this.Size = new System.Drawing.Size(794, 554);
            this.Controls.SetChildIndex(this.registrationPanel, 0);
            this.registrationPanel.ResumeLayout(false);
            this.registrationPanel.PerformLayout();
            this.authorizatonGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FlatButton registrationButton;
        private UsbStatusPanel usbStatusPanel;
        private TableLayoutPanel registrationPanel;
        private ListView authorizedDevicesListView;
        private GroupBox authorizatonGroupBox;
       
    }
}
