using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.CAPI.Properties;

namespace Browsing.CAPI.Forms
{
    partial class WebForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebForm));
            this.webView = new Awesomium.Windows.Forms.WebControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.pullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripCancelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBox = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBox)).BeginInit();
            this.SuspendLayout();
            // 
            // webView
            // 
            this.webView.BackColor = System.Drawing.SystemColors.Control;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 44);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(1600, 830);
            this.webView.Source = new System.Uri("http://google.com", System.UriKind.Absolute);
            this.webView.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 852);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1600, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pullToolStripMenuItem,
            this.toolStripCancelMenuItem,
            this.toolStripSettingsMenuItem,
            this.pushToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1600, 44);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // pullToolStripMenuItem
            // 
            this.pullToolStripMenuItem.AutoSize = false;
            this.pullToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pullToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pullToolStripMenuItem.Image")));
            this.pullToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.pullToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.pullToolStripMenuItem.Name = "pullToolStripMenuItem";
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(94, 40);
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.pullToolStripMenuItem_Click);
            // 
            // toolStripCancelMenuItem
            // 
            this.toolStripCancelMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripCancelMenuItem.AutoSize = false;
            this.toolStripCancelMenuItem.Enabled = false;
            this.toolStripCancelMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripCancelMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripCancelMenuItem.Image")));
            this.toolStripCancelMenuItem.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolStripCancelMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripCancelMenuItem.Name = "toolStripCancelMenuItem";
            this.toolStripCancelMenuItem.Size = new System.Drawing.Size(94, 40);
            this.toolStripCancelMenuItem.Click += new System.EventHandler(this.toolStripCancelMenuItem_Click);
            // 
            // toolStripSettingsMenuItem
            // 
            this.toolStripSettingsMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSettingsMenuItem.AutoSize = false;
            this.toolStripSettingsMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripSettingsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSettingsMenuItem.Image")));
            this.toolStripSettingsMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripSettingsMenuItem.Name = "toolStripSettingsMenuItem";
            this.toolStripSettingsMenuItem.Size = new System.Drawing.Size(94, 40);
            this.toolStripSettingsMenuItem.Click += new System.EventHandler(this.toolStripSettingsMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.pushToolStripMenuItem.AutoSize = false;
            this.pushToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 1F);
            this.pushToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pushToolStripMenuItem.Image")));
            this.pushToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(50, 40);
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.pushToolStripMenuItem_Click);
            // 
            // progressBox
            // 
            this.progressBox.BackColor = System.Drawing.Color.Transparent;
            this.progressBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBox.Image = ((System.Drawing.Image)(resources.GetObject("progressBox.Image")));
            this.progressBox.InitialImage = null;
            this.progressBox.Location = new System.Drawing.Point(0, 44);
            this.progressBox.Name = "progressBox";
            this.progressBox.Size = new System.Drawing.Size(1600, 808);
            this.progressBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.progressBox.TabIndex = 2;
            this.progressBox.TabStop = false;
            this.progressBox.Visible = false;
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 874);
            this.Controls.Add(this.progressBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.webView);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "WebForm";
            this.Text = "CAPI";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

      

        private StatusStrip statusStrip1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem pullToolStripMenuItem;
        private ToolStripMenuItem pushToolStripMenuItem;
        private WebControl webView;
        private ToolStripMenuItem toolStripSettingsMenuItem;
        private ToolStripMenuItem toolStripCancelMenuItem;
        private PictureBox progressBox;
    }
}

