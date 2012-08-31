namespace Browsing.CAPI.Containers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncPanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelAvlUsb = new System.Windows.Forms.Label();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.resultLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.pullButton = new Browsing.CAPI.Controls.FlatButton();
            this.cancelButton = new Browsing.CAPI.Controls.FlatButton();
            this.pushButton = new Browsing.CAPI.Controls.FlatButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.92188F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.59375F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.28906F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.pullButton, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.pushButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.progressBar, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(30);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(512, 384);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // progressBar
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.progressBar, 3);
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(3, 208);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(506, 38);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.6F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.4F));
            this.tableLayoutPanel2.Controls.Add(this.labelAvlUsb, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.toolStripContainer1, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.resultLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.statusLabel, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 252);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(506, 129);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // labelAvlUsb
            // 
            this.labelAvlUsb.AutoSize = true;
            this.labelAvlUsb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAvlUsb.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelAvlUsb.Location = new System.Drawing.Point(193, 81);
            this.labelAvlUsb.Name = "labelAvlUsb";
            this.labelAvlUsb.Size = new System.Drawing.Size(310, 48);
            this.labelAvlUsb.TabIndex = 0;
            this.labelAvlUsb.Text = "Available USB";
            this.labelAvlUsb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(150, 2);
            this.toolStripContainer1.Location = new System.Drawing.Point(193, 51);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(150, 27);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // resultLabel
            // 
            this.resultLabel.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.resultLabel, 2);
            this.resultLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultLabel.Location = new System.Drawing.Point(3, 24);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(500, 24);
            this.resultLabel.TabIndex = 2;
            this.resultLabel.Text = "result";
            this.resultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.statusLabel, 2);
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Location = new System.Drawing.Point(3, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(500, 24);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "status";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pullButton
            // 
            this.pullButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pullButton.FlatAppearance.BorderSize = 0;
            this.pullButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pullButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pullButton.Image = ((System.Drawing.Image)(resources.GetObject("pullButton.Image")));
            this.pullButton.Location = new System.Drawing.Point(3, 138);
            this.pullButton.Name = "pullButton";
            this.pullButton.Size = new System.Drawing.Size(96, 64);
            this.pullButton.TabIndex = 0;
            this.pullButton.Text = "Pull";
            this.pullButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.pullButton.UseVisualStyleBackColor = true;
            this.pullButton.Click += new System.EventHandler(this.pullButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(105, 138);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(294, 64);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // pushButton
            // 
            this.pushButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pushButton.FlatAppearance.BorderSize = 0;
            this.pushButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pushButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pushButton.Image = ((System.Drawing.Image)(resources.GetObject("pushButton.Image")));
            this.pushButton.Location = new System.Drawing.Point(405, 138);
            this.pushButton.Name = "pushButton";
            this.pushButton.Size = new System.Drawing.Size(104, 64);
            this.pushButton.TabIndex = 2;
            this.pushButton.Text = "Push";
            this.pushButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pushButton.UseVisualStyleBackColor = true;
            this.pushButton.Click += new System.EventHandler(this.pushButton_Click);
            // 
            // SyncPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SyncPanel";
            this.Size = new System.Drawing.Size(512, 384);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
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
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.Label resultLabel;
        private System.Windows.Forms.Label statusLabel;
    }
}
