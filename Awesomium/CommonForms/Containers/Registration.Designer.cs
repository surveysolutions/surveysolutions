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
            this.regContent = new System.Windows.Forms.TableLayoutPanel();
            this.regPanel = new Browsing.Common.Containers.RegPanel();
            this.authorizedGroupBox = new System.Windows.Forms.GroupBox();
            this.regContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // registrationPanel
            // 
            this.regContent.ColumnCount = 5;
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.regContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.regContent.Controls.Add(this.regPanel, 2, 4);
            this.regContent.Controls.Add(this.authorizedGroupBox, 2, 1);
            this.regContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.regContent.Location = new System.Drawing.Point(0, 0);
            this.regContent.Margin = new System.Windows.Forms.Padding(4);
            this.regContent.Name = "registrationPanel";
            this.regContent.RowCount = 6;
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.regContent.Size = new System.Drawing.Size(794, 554);
            this.regContent.TabIndex = 1;
            // 
            // regPanel
            // 
            this.regContent.SetColumnSpan(this.regPanel, 2);
            this.regPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.regPanel.Location = new System.Drawing.Point(28, 149);
            this.regPanel.Name = "regPanel";
            this.regContent.SetRowSpan(this.regPanel, 2);
            this.regPanel.Size = new System.Drawing.Size(738, 402);
            this.regPanel.TabIndex = 3;
            // 
            // authorizedGroupBox
            // 
            this.regContent.SetColumnSpan(this.authorizedGroupBox, 2);
            this.authorizedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authorizedGroupBox.Location = new System.Drawing.Point(28, 23);
            this.authorizedGroupBox.Name = "authorizedGroupBox";
            this.authorizedGroupBox.Size = new System.Drawing.Size(738, 100);
            this.authorizedGroupBox.TabIndex = 4;
            this.authorizedGroupBox.TabStop = false;
            this.authorizedGroupBox.Text = "Authorized devices";
            this.authorizedGroupBox.Visible = false;
            // 
            // Registration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.regContent);
            this.Name = "Registration";
            this.Size = new System.Drawing.Size(794, 554);
            this.Controls.SetChildIndex(this.regContent, 0);
            this.regContent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel regContent;
        private RegPanel regPanel;
        private GroupBox authorizedGroupBox;
    }
}
