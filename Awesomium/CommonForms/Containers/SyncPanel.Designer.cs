using System.Collections.Generic;

namespace Browsing.Common.Containers
{
    partial class SyncPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncPanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.usbStatusPanel = new Browsing.Common.Containers.UsbStatusPanel();
            this.pullButton = new Browsing.Common.Controls.FlatButton();
            this.cancelButton = new Browsing.Common.Controls.FlatButton();
            this.pushButton = new Browsing.Common.Controls.FlatButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.Controls.Add(this.usbStatusPanel, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.pullButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.pushButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.progressBar, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(40, 37, 40, 37);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1059, 682);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // progressBar
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.progressBar, 3);
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(37, 295);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(985, 30);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // usbStatusPanel
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.usbStatusPanel, 3);
            this.usbStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usbStatusPanel.Location = new System.Drawing.Point(38, 346);
            this.usbStatusPanel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 15);
            this.usbStatusPanel.Name = "usbStatusPanel";
            this.usbStatusPanel.Size = new System.Drawing.Size(983, 321);
            this.usbStatusPanel.TabIndex = 0;
            // 
            // pullButton
            // 
            this.pullButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.pullButton.FlatAppearance.BorderSize = 0;
            this.pullButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pullButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pullButton.Image = ((System.Drawing.Image)(resources.GetObject("pullButton.Image")));
            this.pullButton.Location = new System.Drawing.Point(37, 29);
            this.pullButton.Margin = new System.Windows.Forms.Padding(4);
            this.pullButton.Name = "pullButton";
            this.pullButton.Size = new System.Drawing.Size(259, 246);
            this.pullButton.TabIndex = 0;
            this.pullButton.Text = "Pull";
            this.pullButton.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.pullButton.UseVisualStyleBackColor = true;
            this.pullButton.Click += new System.EventHandler(this.pullButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(304, 29);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(451, 246);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // pushButton
            // 
            this.pushButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.pushButton.FlatAppearance.BorderSize = 0;
            this.pushButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pushButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pushButton.Image = ((System.Drawing.Image)(resources.GetObject("pushButton.Image")));
            this.pushButton.Location = new System.Drawing.Point(763, 29);
            this.pushButton.Margin = new System.Windows.Forms.Padding(4);
            this.pushButton.Name = "pushButton";
            this.pushButton.Size = new System.Drawing.Size(259, 246);
            this.pushButton.TabIndex = 2;
            this.pushButton.Text = "Push";
            this.pushButton.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.pushButton.UseVisualStyleBackColor = true;
            this.pushButton.Click += new System.EventHandler(this.pushButton_Click);
            // 
            // SyncPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SyncPanel";
            this.Size = new System.Drawing.Size(1059, 682);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
       
        private Controls.FlatButton pullButton;
        private Controls.FlatButton cancelButton;
        private Controls.FlatButton pushButton;
        private System.Windows.Forms.ProgressBar progressBar;
        
        public UsbStatusPanel usbStatusPanel;

    }
}
