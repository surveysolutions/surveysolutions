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
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webView
            // 
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
            this.pullToolStripMenuItem.Size = new System.Drawing.Size(50, 40);
            this.pullToolStripMenuItem.Click += new System.EventHandler(this.pullToolStripMenuItem_Click);
            // 
            // pullToolStripMenuItem1
            // 
            this.pushToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.pushToolStripMenuItem.AutoSize = false;
            this.pushToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 1F);
            this.pushToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pullToolStripMenuItem1.Image")));
            this.pushToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.pushToolStripMenuItem.Name = "pullToolStripMenuItem1";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(50, 40);
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.pushToolStripMenuItem_Click);
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 874);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

      

        private StatusStrip statusStrip1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem pullToolStripMenuItem;
        private ToolStripMenuItem pushToolStripMenuItem;
        private WebControl webView;
    }
}

