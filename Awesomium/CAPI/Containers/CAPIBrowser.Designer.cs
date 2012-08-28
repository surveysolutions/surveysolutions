

namespace Browsing.CAPI.Containers
{
    partial class CAPIBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CAPIBrowser));
            
            this.progressBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.progressBox)).BeginInit();
            this.SuspendLayout();
            // 
            // webView
            // 
            this.webView.BackColor = System.Drawing.SystemColors.Control;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 50);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(1440, 578);
           
            this.webView.TabIndex = 2;
            this.webView.BeginLoading += new Awesomium.Core.BeginLoadingEventHandler(this.webView_BeginLoading);
            this.webView.LoadCompleted += new System.EventHandler(this.webView_LoadCompleted);
            this.webView.ResourceRequest += new Awesomium.Core.ResourceRequestEventHandler(this.webView_ResourceRequest);
            // 
            // progressBox
            // 
            this.progressBox.BackColor = System.Drawing.Color.Transparent;
            this.progressBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBox.Image = ((System.Drawing.Image)(resources.GetObject("progressBox.Image")));
            this.progressBox.InitialImage = null;
            this.progressBox.Location = new System.Drawing.Point(0, 50);
            this.progressBox.Name = "progressBox";
            this.progressBox.Size = new System.Drawing.Size(1440, 578);
            this.progressBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.progressBox.TabIndex = 3;
            this.progressBox.TabStop = false;
            // 
            // CAPIBrowser
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.progressBox);
            this.Controls.Add(this.webView);
            this.Name = "CAPIBrowser";
            this.Size = new System.Drawing.Size(1440, 628);
            ((System.ComponentModel.ISupportInitialize)(this.progressBox)).EndInit();
            this.ResumeLayout(false);
        }

       

        #endregion

        private Awesomium.Windows.Forms.WebControl webView;
        private System.Windows.Forms.PictureBox progressBox;
    }
}
