using System.Windows.Forms;
namespace WinFormsSample
{
    partial class PleaseWaitForm
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
            this.statusLabel = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(9, 8);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(135, 13);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Export started. Plase wait...";
            
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 24);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(132, 15);
            this.progressBar.TabIndex = 1;
            // 
            // PleaseWaitForm
            // 
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(162, 53);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.statusLabel);
            this.Name = "PleaseWaitForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public ProgressBar progressBar;
        public Label statusLabel;

  
        
    }

}