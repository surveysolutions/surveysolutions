namespace Browsing.Supervisor.Containers
{
    partial class SyncHQProcessPage
    {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncHQProcessPage));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHolder = new System.Windows.Forms.Panel();
            this.btnUnderConstruction = new Browsing.Supervisor.Controls.FlatButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlHolder.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 920F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.Controls.Add(this.pnlHolder, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(920, 200);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlHolder
            // 
            this.pnlHolder.Controls.Add(this.btnUnderConstruction);
            this.pnlHolder.Location = new System.Drawing.Point(3, 3);
            this.pnlHolder.Name = "pnlHolder";
            this.pnlHolder.Size = new System.Drawing.Size(914, 194);
            this.pnlHolder.TabIndex = 0;
            // 
            // btnConstruction
            // 
            this.btnUnderConstruction.FlatAppearance.BorderSize = 0;
            this.btnUnderConstruction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUnderConstruction.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUnderConstruction.Image = ((System.Drawing.Image)(resources.GetObject("Construction.Image")));
            this.btnUnderConstruction.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnUnderConstruction.Location = new System.Drawing.Point(270, 0);
            this.btnUnderConstruction.Margin = new System.Windows.Forms.Padding(0);
            this.btnUnderConstruction.Name = "btnConstruction";
            this.btnUnderConstruction.Size = new System.Drawing.Size(220, 200);
            this.btnUnderConstruction.TabIndex = 3;
            this.btnUnderConstruction.Text = "Under construction";
            this.btnUnderConstruction.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnUnderConstruction.UseVisualStyleBackColor = true;

            // 
            // SupervisorMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(920, 200);
            this.Name = "Synchronization";
            this.Size = new System.Drawing.Size(920, 200);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlHolder.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.FlatButton btnUnderConstruction;
        private System.Windows.Forms.Panel pnlHolder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
