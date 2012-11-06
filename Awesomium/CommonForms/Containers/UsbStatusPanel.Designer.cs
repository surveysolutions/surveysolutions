namespace Browsing.Common.Containers
{
    partial class UsbStatusPanel
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsbStatusPanel));
            this.tableLayoutPanel2GroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.resultLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.labelAvlUsb = new System.Windows.Forms.Label();
            this.usbStrip = new System.Windows.Forms.ToolStrip();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel2GroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2GroupBox
            // 
            this.tableLayoutPanel2GroupBox.Controls.Add(this.tableLayoutPanel2);
            this.tableLayoutPanel2GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2GroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tableLayoutPanel2GroupBox.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2GroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 16);
            this.tableLayoutPanel2GroupBox.Name = "tableLayoutPanel2GroupBox";
            this.tableLayoutPanel2GroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2GroupBox.Size = new System.Drawing.Size(794, 554);
            this.tableLayoutPanel2GroupBox.TabIndex = 6;
            this.tableLayoutPanel2GroupBox.TabStop = false;
            this.tableLayoutPanel2GroupBox.Text = "Current status";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 204F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.resultLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.statusLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelAvlUsb, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.usbStrip, 1, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 18);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(790, 534);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // resultLabel
            // 
            this.resultLabel.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.resultLabel, 2);
            this.resultLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultLabel.Location = new System.Drawing.Point(3, 34);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(784, 24);
            this.resultLabel.TabIndex = 2;
            this.resultLabel.Text = "result";
            this.resultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.statusLabel, 2);
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusLabel.Location = new System.Drawing.Point(3, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(784, 24);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "status";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // labelAvlUsb
            // 
            this.labelAvlUsb.AutoSize = true;
            this.labelAvlUsb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAvlUsb.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelAvlUsb.Location = new System.Drawing.Point(3, 76);
            this.labelAvlUsb.Margin = new System.Windows.Forms.Padding(3, 18, 3, 0);
            this.labelAvlUsb.Name = "labelAvlUsb";
            this.labelAvlUsb.Size = new System.Drawing.Size(198, 24);
            this.labelAvlUsb.TabIndex = 0;
            this.labelAvlUsb.Text = "Available USB drivers:";
            this.labelAvlUsb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // usbStrip
            // 
            this.usbStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.usbStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.usbStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.usbStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.usbStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.usbStrip.Location = new System.Drawing.Point(204, 58);
            this.usbStrip.Name = "usbStrip";
            this.usbStrip.ShowItemToolTips = false;
            this.usbStrip.Size = new System.Drawing.Size(1, 42);
            this.usbStrip.TabIndex = 4;
            this.usbStrip.Text = "toolStrip1";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "USB green_6486.png");
            this.imageList1.Images.SetKeyName(1, "USB red_6486.png");
            // 
            // UsbStatusPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2GroupBox);
            this.Name = "UsbStatusPanel";
            this.Size = new System.Drawing.Size(794, 554);
            this.tableLayoutPanel2GroupBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox tableLayoutPanel2GroupBox;
        private System.Windows.Forms.Label labelAvlUsb;
        private System.Windows.Forms.Label resultLabel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ToolStrip usbStrip;
        private System.Windows.Forms.ImageList imageList1;
        #endregion
        
    }
}
