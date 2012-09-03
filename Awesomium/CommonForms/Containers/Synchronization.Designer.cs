namespace Browsing.Common.Containers
{
    partial class Synchronization
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Synchronization));
            this.syncPanel = new Browsing.Common.Containers.SyncPanel();
            this.SuspendLayout();
            // 
            // syncPanel1
            // 
            this.syncPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.syncPanel.Location = new System.Drawing.Point(0, 0);
            this.syncPanel.Name = "syncPanel1";
            this.syncPanel.Size = new System.Drawing.Size(600, 500);
            this.syncPanel.TabIndex = 3;
            // 
            // CAPISynchronization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.syncPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CAPISynchronization";
            this.Size = new System.Drawing.Size(600, 500);
            this.Controls.SetChildIndex(this.syncPanel, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private SyncPanel syncPanel;
    }
}
