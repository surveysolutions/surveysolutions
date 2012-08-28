namespace Browsing.CAPI.Containers
{
    partial class CAPISynchronization
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CAPISynchronization));
            this.btnPull = new Browsing.CAPI.Controls.FlatButton();
            this.btnPush = new Browsing.CAPI.Controls.FlatButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.btnCancel = new Browsing.CAPI.Controls.FlatButton();
            this.SuspendLayout();
            // 
            // btnPull
            // 
            this.btnPull.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnPull.FlatAppearance.BorderSize = 0;
            this.btnPull.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPull.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.btnPull.Image = ((System.Drawing.Image)(resources.GetObject("btnPull.Image")));
            this.btnPull.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnPull.Location = new System.Drawing.Point(0, 58);
            this.btnPull.Margin = new System.Windows.Forms.Padding(0);
            this.btnPull.Name = "btnPull";
            this.btnPull.Size = new System.Drawing.Size(120, 120);
            this.btnPull.TabIndex = 0;
            this.btnPull.Text = "Pull";
            this.btnPull.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnPull.UseVisualStyleBackColor = true;
            this.btnPull.Click += new System.EventHandler(this.btnPull_Click);
            // 
            // btnPush
            // 
            this.btnPush.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnPush.FlatAppearance.BorderSize = 0;
            this.btnPush.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPush.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.btnPush.Image = ((System.Drawing.Image)(resources.GetObject("btnPush.Image")));
            this.btnPush.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnPush.Location = new System.Drawing.Point(280, 58);
            this.btnPush.Margin = new System.Windows.Forms.Padding(0);
            this.btnPush.Name = "btnPush";
            this.btnPush.Size = new System.Drawing.Size(120, 120);
            this.btnPush.TabIndex = 1;
            this.btnPush.Text = "Push";
            this.btnPush.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnPush.UseVisualStyleBackColor = true;
            this.btnPush.Click += new System.EventHandler(this.btnPush_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 178);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(400, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.Location = new System.Drawing.Point(136, 58);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 120);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CAPISynchronization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            //this.Controls.Add(this.flatButton1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPush);
            this.Controls.Add(this.btnPull);
            this.Controls.Add(this.statusStrip1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CAPISynchronization";
            this.Size = new System.Drawing.Size(400, 200);
            this.Load += new System.EventHandler(this.CAPISynchronization_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.FlatButton btnPush;
        protected internal Controls.FlatButton btnPull;
        private Controls.FlatButton btnCancel;
    }
}
