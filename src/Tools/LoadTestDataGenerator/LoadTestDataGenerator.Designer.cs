namespace LoadTestDataGenerator
{
    partial class LoadTestDataGenerator
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
            this.generate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.surveys_amount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.templatePath = new System.Windows.Forms.TextBox();
            this.supervisorsCount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.interviewersCount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.chkSetAnswers = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ctrlProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.chkGenerateSnapshoots = new System.Windows.Forms.CheckBox();
            this.chkHeadquarter = new System.Windows.Forms.CheckBox();
            this.txtHQName = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // generate
            // 
            this.generate.Location = new System.Drawing.Point(260, 265);
            this.generate.Name = "generate";
            this.generate.Size = new System.Drawing.Size(160, 51);
            this.generate.TabIndex = 0;
            this.generate.Text = "Generate";
            this.generate.UseVisualStyleBackColor = true;
            this.generate.Click += new System.EventHandler(this.generate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 142);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of surveys";
            // 
            // surveys_amount
            // 
            this.surveys_amount.Location = new System.Drawing.Point(163, 142);
            this.surveys_amount.Name = "surveys_amount";
            this.surveys_amount.Size = new System.Drawing.Size(257, 20);
            this.surveys_amount.TabIndex = 2;
            this.surveys_amount.Text = "2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 107);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Questionnaire template file";
            // 
            // templatePath
            // 
            this.templatePath.Location = new System.Drawing.Point(163, 107);
            this.templatePath.Name = "templatePath";
            this.templatePath.Size = new System.Drawing.Size(257, 20);
            this.templatePath.TabIndex = 4;
            this.templatePath.Text = "C:\\Users\\Вячеслав\\Downloads\\sl.txt";
            this.templatePath.Enter += new System.EventHandler(this.templatePath_Enter);
            // 
            // supervisorsCount
            // 
            this.supervisorsCount.Location = new System.Drawing.Point(163, 182);
            this.supervisorsCount.Name = "supervisorsCount";
            this.supervisorsCount.Size = new System.Drawing.Size(257, 20);
            this.supervisorsCount.TabIndex = 6;
            this.supervisorsCount.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 182);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Supervisors count";
            // 
            // interviewersCount
            // 
            this.interviewersCount.Location = new System.Drawing.Point(163, 223);
            this.interviewersCount.Name = "interviewersCount";
            this.interviewersCount.Size = new System.Drawing.Size(257, 20);
            this.interviewersCount.TabIndex = 8;
            this.interviewersCount.Text = "2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 223);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Interviewers count";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(18, 13);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(163, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Generate supervisor\'s events";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(260, 43);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(97, 17);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "Clear database";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // chkSetAnswers
            // 
            this.chkSetAnswers.AutoSize = true;
            this.chkSetAnswers.Location = new System.Drawing.Point(18, 43);
            this.chkSetAnswers.Name = "chkSetAnswers";
            this.chkSetAnswers.Size = new System.Drawing.Size(189, 17);
            this.chkSetAnswers.TabIndex = 11;
            this.chkSetAnswers.Text = "Set answers for featured questions";
            this.chkSetAnswers.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctrlProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 387);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(435, 22);
            this.statusStrip1.TabIndex = 12;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ctrlProgress
            // 
            this.ctrlProgress.AutoSize = false;
            this.ctrlProgress.Name = "ctrlProgress";
            this.ctrlProgress.Size = new System.Drawing.Size(410, 16);
            this.ctrlProgress.Step = 1;
            this.ctrlProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // chkGenerateSnapshoots
            // 
            this.chkGenerateSnapshoots.AutoSize = true;
            this.chkGenerateSnapshoots.Checked = true;
            this.chkGenerateSnapshoots.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateSnapshoots.Location = new System.Drawing.Point(260, 13);
            this.chkGenerateSnapshoots.Name = "chkGenerateSnapshoots";
            this.chkGenerateSnapshoots.Size = new System.Drawing.Size(127, 17);
            this.chkGenerateSnapshoots.TabIndex = 13;
            this.chkGenerateSnapshoots.Text = "Generate snapshoots";
            this.chkGenerateSnapshoots.UseVisualStyleBackColor = true;
            // 
            // chkHeadquarter
            // 
            this.chkHeadquarter.AutoSize = true;
            this.chkHeadquarter.Checked = true;
            this.chkHeadquarter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHeadquarter.Enabled = false;
            this.chkHeadquarter.Location = new System.Drawing.Point(18, 76);
            this.chkHeadquarter.Name = "chkHeadquarter";
            this.chkHeadquarter.Size = new System.Drawing.Size(130, 17);
            this.chkHeadquarter.TabIndex = 14;
            this.chkHeadquarter.Text = "Generate headquarter";
            this.chkHeadquarter.UseVisualStyleBackColor = true;
            this.chkHeadquarter.CheckedChanged += new System.EventHandler(this.chkHeadquarter_CheckedChanged);
            // 
            // txtHQName
            // 
            this.txtHQName.Location = new System.Drawing.Point(163, 74);
            this.txtHQName.Name = "txtHQName";
            this.txtHQName.Size = new System.Drawing.Size(257, 20);
            this.txtHQName.TabIndex = 15;
            this.txtHQName.Text = "hq";
            // 
            // LoadTestDataGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 409);
            this.Controls.Add(this.txtHQName);
            this.Controls.Add(this.chkHeadquarter);
            this.Controls.Add(this.chkGenerateSnapshoots);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.chkSetAnswers);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.interviewersCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.supervisorsCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.templatePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.surveys_amount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.generate);
            this.MaximizeBox = false;
            this.Name = "LoadTestDataGenerator";
            this.Text = "Load Test Data Generator";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button generate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox surveys_amount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox templatePath;
        private System.Windows.Forms.TextBox supervisorsCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox interviewersCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox chkSetAnswers;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar ctrlProgress;
        private System.Windows.Forms.CheckBox chkGenerateSnapshoots;
        private System.Windows.Forms.CheckBox chkHeadquarter;
        private System.Windows.Forms.TextBox txtHQName;
    }
}

