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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.usbStatusPanel = new Browsing.Common.Containers.UsbStatusPanel();
            this.registrationButton = new Browsing.Common.Controls.FlatButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Controls.Add(this.usbStatusPanel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.registrationButton, 3, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(794, 554);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // usbStatusPanel
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.usbStatusPanel, 2);
            this.usbStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usbStatusPanel.Location = new System.Drawing.Point(29, 24);
            this.usbStatusPanel.Margin = new System.Windows.Forms.Padding(4);
            this.usbStatusPanel.Name = "usbStatusPanel";
            this.tableLayoutPanel1.SetRowSpan(this.usbStatusPanel, 2);
            this.usbStatusPanel.Size = new System.Drawing.Size(536, 526);
            this.usbStatusPanel.TabIndex = 0;
            // 
            // registrationButton
            // 
            this.registrationButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.registrationButton.FlatAppearance.BorderSize = 0;
            this.registrationButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.registrationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.registrationButton.Image = ((System.Drawing.Image)(resources.GetObject("registrationButton.Image")));
            this.registrationButton.Location = new System.Drawing.Point(572, 23);
            this.registrationButton.Name = "registrationButton";
            this.registrationButton.Size = new System.Drawing.Size(194, 200);
            this.registrationButton.TabIndex = 0;
            this.registrationButton.Text = "Register";
            this.registrationButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.registrationButton.UseVisualStyleBackColor = true;
            this.registrationButton.Click += new System.EventHandler(this.registrationButton_Click);
            // 
            // Registration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Registration";
            this.Size = new System.Drawing.Size(794, 554);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FlatButton registrationButton;
        private UsbStatusPanel usbStatusPanel;
        private TableLayoutPanel tableLayoutPanel1;
       
    }
}
