namespace Browsing.Supervisor.Containers
{
    partial class SupervisorBrowser
    {
        #region Properties

        private System.ComponentModel.IContainer components = null;
        
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SupervisorBrowser));

            // 
            // Browser
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Name = "Browser";
            this.Size = new System.Drawing.Size(1440, 628);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
