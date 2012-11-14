using System.Windows.Forms;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    partial class RegPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegPanel));
            this.registrationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.usbStatusPanel = new Browsing.Common.Containers.UsbStatusPanel();
            this.registrationButtonSecondPhase = new Browsing.Common.Controls.FlatButton();
            this.registrationButtonFirstPhase = new Browsing.Common.Controls.FlatButton();
            this.registrationPanel.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // registrationPanel
            // 
            this.registrationPanel.ColumnCount = 3;
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.registrationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.registrationPanel.Controls.Add(this.usbStatusPanel, 0, 0);
            this.registrationPanel.Controls.Add(this.buttonsPanel, 2, 0);
            this.registrationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.registrationPanel.Location = new System.Drawing.Point(0, 0);
            this.registrationPanel.Margin = new System.Windows.Forms.Padding(4);
            this.registrationPanel.Name = "registrationPanel";
            this.registrationPanel.RowCount = 1;
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.registrationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 554F));
            this.registrationPanel.Size = new System.Drawing.Size(794, 554);
            this.registrationPanel.TabIndex = 1;
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.ColumnCount = 1;
            this.buttonsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.buttonsPanel.Controls.Add(this.registrationButtonSecondPhase, 0, 2);
            this.buttonsPanel.Controls.Add(this.registrationButtonFirstPhase, 0, 0);
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsPanel.Location = new System.Drawing.Point(597, 3);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.RowCount = 3;
            this.buttonsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.buttonsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.buttonsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.buttonsPanel.Size = new System.Drawing.Size(194, 548);
            this.buttonsPanel.TabIndex = 3;
            // 
            // usbStatusPanel
            // 
            this.registrationPanel.SetColumnSpan(this.usbStatusPanel, 2);
            this.usbStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usbStatusPanel.Location = new System.Drawing.Point(4, 4);
            this.usbStatusPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 20);
            this.usbStatusPanel.Name = "usbStatusPanel";
            this.usbStatusPanel.Size = new System.Drawing.Size(586, 530);
            this.usbStatusPanel.TabIndex = 0;
            // 
            // registrationButtonSecondPhase
            // 
            this.registrationButtonSecondPhase.Dock = System.Windows.Forms.DockStyle.Top;
            this.registrationButtonSecondPhase.Enabled = false;
            this.registrationButtonSecondPhase.FlatAppearance.BorderSize = 0;
            this.registrationButtonSecondPhase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.registrationButtonSecondPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.registrationButtonSecondPhase.Image = ((System.Drawing.Image)(resources.GetObject("registrationButtonSecondPhase.Image")));
            this.registrationButtonSecondPhase.Location = new System.Drawing.Point(3, 287);
            this.registrationButtonSecondPhase.Name = "registrationButtonSecondPhase";
            this.registrationButtonSecondPhase.Size = new System.Drawing.Size(194, 200);
            this.registrationButtonSecondPhase.TabIndex = 1;
            this.registrationButtonSecondPhase.Text = "Finalize";
            this.registrationButtonSecondPhase.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.registrationButtonSecondPhase.UseVisualStyleBackColor = true;
            // 
            // registrationButtonFirstPhase
            // 
            this.registrationButtonFirstPhase.Dock = System.Windows.Forms.DockStyle.Top;
            this.registrationButtonFirstPhase.Enabled = false;
            this.registrationButtonFirstPhase.FlatAppearance.BorderSize = 0;
            this.registrationButtonFirstPhase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.registrationButtonFirstPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.registrationButtonFirstPhase.Image = ((System.Drawing.Image)(resources.GetObject("registrationButtonFirstPhase.Image")));
            this.registrationButtonFirstPhase.Location = new System.Drawing.Point(3, 3);
            this.registrationButtonFirstPhase.Name = "registrationButtonFirstPhase";
            this.registrationButtonFirstPhase.Size = new System.Drawing.Size(194, 200);
            this.registrationButtonFirstPhase.TabIndex = 0;
            this.registrationButtonFirstPhase.Text = "Register";
            this.registrationButtonFirstPhase.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.registrationButtonFirstPhase.UseVisualStyleBackColor = true;
            // 
            // RegPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.registrationPanel);
            this.Name = "RegPanel";
            this.Size = new System.Drawing.Size(794, 554);
            this.registrationPanel.ResumeLayout(false);
            this.buttonsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FlatButton registrationButtonFirstPhase;
        private TableLayoutPanel registrationPanel;
        private TableLayoutPanel buttonsPanel;
        private FlatButton registrationButtonSecondPhase;
        private UsbStatusPanel usbStatusPanel;
       
    }
}
