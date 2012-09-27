namespace Browsing.Common.Containers
{
    partial class SyncPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncPanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.resultLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.labelAvlUsb = new System.Windows.Forms.Label();
            this.usbStrip = new System.Windows.Forms.ToolStrip();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.usbImageList = new System.Windows.Forms.ImageList(this.components);
            this.pullButton = new Browsing.Common.Controls.FlatButton();
            this.cancelButton = new Browsing.Common.Controls.FlatButton();
            this.pushButton = new Browsing.Common.Controls.FlatButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.pullButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.pushButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.progressBar, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(40, 37, 40, 37);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1053, 615);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 272F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel2.Controls.Add(this.resultLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.statusLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelAvlUsb, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.usbStrip, 1, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tableLayoutPanel2.Location = new System.Drawing.Point(37, 345);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(979, 268);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // resultLabel
            // 
            this.resultLabel.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.resultLabel, 2);
            this.resultLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultLabel.Location = new System.Drawing.Point(4, 41);
            this.resultLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(971, 29);
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
            this.statusLabel.Location = new System.Drawing.Point(4, 0);
            this.statusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(971, 29);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "status";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // labelAvlUsb
            // 
            this.labelAvlUsb.AutoSize = true;
            this.labelAvlUsb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAvlUsb.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelAvlUsb.Location = new System.Drawing.Point(4, 92);
            this.labelAvlUsb.Margin = new System.Windows.Forms.Padding(4, 22, 4, 0);
            this.labelAvlUsb.Name = "labelAvlUsb";
            this.labelAvlUsb.Size = new System.Drawing.Size(264, 29);
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
            this.usbStrip.Location = new System.Drawing.Point(272, 70);
            this.usbStrip.Name = "usbStrip";
            this.usbStrip.ShowItemToolTips = false;
            this.usbStrip.Size = new System.Drawing.Size(1, 51);
            this.usbStrip.TabIndex = 4;
            this.usbStrip.Text = "toolStrip1";
            // 
            // progressBar
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.progressBar, 3);
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(37, 295);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(979, 30);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // usbImageList
            // 
            this.usbImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("usbImageList.ImageStream")));
            this.usbImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.usbImageList.Images.SetKeyName(0, "USB green_6486.png");
            this.usbImageList.Images.SetKeyName(1, "USB red_6486.png");
            // 
            // pullButton
            // 
            this.pullButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.pullButton.FlatAppearance.BorderSize = 0;
            this.pullButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pullButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pullButton.Image = ((System.Drawing.Image)(resources.GetObject("pullButton.Image")));
            this.pullButton.Location = new System.Drawing.Point(37, 29);
            this.pullButton.Margin = new System.Windows.Forms.Padding(4);
            this.pullButton.Name = "pullButton";
            this.pullButton.Size = new System.Drawing.Size(259, 246);
            this.pullButton.TabIndex = 0;
            this.pullButton.Text = "Pull";
            this.pullButton.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.pullButton.UseVisualStyleBackColor = true;
            this.pullButton.Click += new System.EventHandler(this.pullButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(304, 29);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(445, 246);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // pushButton
            // 
            this.pushButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.pushButton.FlatAppearance.BorderSize = 0;
            this.pushButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pushButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pushButton.Image = ((System.Drawing.Image)(resources.GetObject("pushButton.Image")));
            this.pushButton.Location = new System.Drawing.Point(757, 29);
            this.pushButton.Margin = new System.Windows.Forms.Padding(4);
            this.pushButton.Name = "pushButton";
            this.pushButton.Size = new System.Drawing.Size(259, 246);
            this.pushButton.TabIndex = 2;
            this.pushButton.Text = "Push";
            this.pushButton.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.pushButton.UseVisualStyleBackColor = true;
            this.pushButton.Click += new System.EventHandler(this.pushButton_Click);
            // 
            // SyncPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SyncPanel";
            this.Size = new System.Drawing.Size(1053, 615);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Controls.FlatButton pullButton;
        private Controls.FlatButton cancelButton;
        private Controls.FlatButton pushButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelAvlUsb;
        private System.Windows.Forms.Label resultLabel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ToolStrip usbStrip;
        private System.Windows.Forms.ImageList usbImageList;
    }
}
