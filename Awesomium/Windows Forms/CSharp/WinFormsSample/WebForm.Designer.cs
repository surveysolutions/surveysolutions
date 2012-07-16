using System.Windows.Forms;

namespace WinFormsSample
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
            this.SuspendLayout();
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 442);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WebForm";
            this.Text = "CAPI";
            this.Menu = new MainMenu();
                MenuItem sinh = new MenuItem("Export");
                sinh.Click += new System.EventHandler(this.sinh_click);
            this.Menu.MenuItems.Add(sinh);
            this.ResumeLayout(false);

        }

        #endregion
        private void sinh_click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            choose.CreatePortable();
        }
    }
}

