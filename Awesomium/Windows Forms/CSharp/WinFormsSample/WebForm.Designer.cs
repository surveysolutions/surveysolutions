using System.Windows.Forms;
using System.Threading;
using System;
using Awesomium.Core;
using WinFormsSample.Properties;

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
            int deskHeight = Screen.PrimaryScreen.Bounds.Height;
            int deskWidth = Screen.PrimaryScreen.Bounds.Width;
            this.ClientSize = new System.Drawing.Size(deskWidth,deskHeight);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WebForm";
            this.Text = "CAPI";
            this.Menu = new MainMenu();
                MenuItem sinh = new MenuItem("Export");
            //sinh.Enabled = false;
                sinh.Click += new System.EventHandler(this.sinh_click);
            this.Menu.MenuItems.Add(sinh);
            
            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            
            this.ResumeLayout(false);

        }

        #endregion
        private void sinh_click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //choose.CreatePortable();
            string drive = getDrive();
            if (drive != null)
            {
                PleaseWaitForm pleaseWait = new PleaseWaitForm();

                // Display form modelessly

                try
                {
                    if (export.isActive())
                    {
                        try
                        {
                            export.Stop();
                        }
                        catch (Exception ex)
                        {
                            
                            throw ex;
                        }
                        
                    }
                    
                    try
                    {
                        export.Start(drive, webView);
                    }
                    catch (Exception ex)
                    {
                        
                        throw ex;
                    }
                    
                }
                catch (Exception ex)
                {
                    // MessageBox.Show("Export error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw ex;
                }
                
            }
        }
    }
}

