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
            this.generateSupervisorEvents = new System.Windows.Forms.CheckBox();
            this.clearDatabase = new System.Windows.Forms.CheckBox();
            this.chkSetAnswers = new System.Windows.Forms.CheckBox();
            this.ctrlProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.generateCapiEvents = new System.Windows.Forms.CheckBox();
            this.defaultDatabaseName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkGenerateSnapshoots = new System.Windows.Forms.CheckBox();
            this.chkHeadquarter = new System.Windows.Forms.CheckBox();
            this.txtHQName = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.eventsStatistics = new System.Windows.Forms.ListBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // generate
            // 
            this.generate.Location = new System.Drawing.Point(12, 275);
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
            this.label1.Location = new System.Drawing.Point(21, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of surveys";
            // 
            // surveys_amount
            // 
            this.surveys_amount.Location = new System.Drawing.Point(179, 147);
            this.surveys_amount.Name = "surveys_amount";
            this.surveys_amount.Size = new System.Drawing.Size(257, 20);
            this.surveys_amount.TabIndex = 2;
            this.surveys_amount.Text = "2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Questionnaire template file";
            // 
            // templatePath
            // 
            this.templatePath.Location = new System.Drawing.Point(179, 117);
            this.templatePath.Name = "templatePath";
            this.templatePath.Size = new System.Drawing.Size(257, 20);
            this.templatePath.TabIndex = 4;
            this.templatePath.Text = "C:\\Users\\Вячеслав\\Downloads\\sl.txt";
            this.templatePath.Enter += new System.EventHandler(this.templatePath_Enter);
            // 
            // supervisorsCount
            // 
            this.supervisorsCount.Location = new System.Drawing.Point(179, 177);
            this.supervisorsCount.Name = "supervisorsCount";
            this.supervisorsCount.Size = new System.Drawing.Size(257, 20);
            this.supervisorsCount.TabIndex = 6;
            this.supervisorsCount.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 184);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Supervisors count";
            // 
            // interviewersCount
            // 
            this.interviewersCount.Location = new System.Drawing.Point(179, 207);
            this.interviewersCount.Name = "interviewersCount";
            this.interviewersCount.Size = new System.Drawing.Size(257, 20);
            this.interviewersCount.TabIndex = 8;
            this.interviewersCount.Text = "2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 214);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Interviewers count";
            // 
            // generateSupervisorEvents
            // 
            this.generateSupervisorEvents.AutoSize = true;
            this.generateSupervisorEvents.Checked = true;
            this.generateSupervisorEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.generateSupervisorEvents.Location = new System.Drawing.Point(21, 42);
            this.generateSupervisorEvents.Name = "generateSupervisorEvents";
            this.generateSupervisorEvents.Size = new System.Drawing.Size(163, 17);
            this.generateSupervisorEvents.TabIndex = 9;
            this.generateSupervisorEvents.Text = "Generate supervisor\'s events";
            this.generateSupervisorEvents.UseVisualStyleBackColor = true;
            // 
            // clearDatabase
            // 
            this.clearDatabase.AutoSize = true;
            this.clearDatabase.Location = new System.Drawing.Point(21, 19);
            this.clearDatabase.Name = "clearDatabase";
            this.clearDatabase.Size = new System.Drawing.Size(97, 17);
            this.clearDatabase.TabIndex = 10;
            this.clearDatabase.Text = "Clear database";
            this.clearDatabase.UseVisualStyleBackColor = true;
            // 
            // chkSetAnswers
            // 
            this.chkSetAnswers.AutoSize = true;
            this.chkSetAnswers.Checked = true;
            this.chkSetAnswers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSetAnswers.Location = new System.Drawing.Point(232, 42);
            this.chkSetAnswers.Name = "chkSetAnswers";
            this.chkSetAnswers.Size = new System.Drawing.Size(160, 17);
            this.chkSetAnswers.TabIndex = 11;
            this.chkSetAnswers.Text = "Generate featured questions";
            this.chkSetAnswers.UseVisualStyleBackColor = true;
            // 
            // ctrlProgress
            // 
            this.ctrlProgress.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ctrlProgress.AutoSize = false;
            this.ctrlProgress.Name = "ctrlProgress";
            this.ctrlProgress.Size = new System.Drawing.Size(760, 16);
            this.ctrlProgress.Step = 1;
            this.ctrlProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // generateCapiEvents
            // 
            this.generateCapiEvents.AutoSize = true;
            this.generateCapiEvents.Checked = true;
            this.generateCapiEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.generateCapiEvents.Location = new System.Drawing.Point(232, 19);
            this.generateCapiEvents.Name = "generateCapiEvents";
            this.generateCapiEvents.Size = new System.Drawing.Size(139, 17);
            this.generateCapiEvents.TabIndex = 11;
            this.generateCapiEvents.Text = "Generate CAPI\'s events";
            this.generateCapiEvents.UseVisualStyleBackColor = true;
            // 
            // defaultDatabaseName
            // 
            this.defaultDatabaseName.Enabled = false;
            this.defaultDatabaseName.Location = new System.Drawing.Point(536, 13);
            this.defaultDatabaseName.Name = "defaultDatabaseName";
            this.defaultDatabaseName.Size = new System.Drawing.Size(224, 20);
            this.defaultDatabaseName.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(448, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Database name";
            // 
            // chkGenerateSnapshoots
            // 
            this.chkGenerateSnapshoots.AutoSize = true;
            this.chkGenerateSnapshoots.Checked = true;
            this.chkGenerateSnapshoots.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateSnapshoots.Location = new System.Drawing.Point(21, 65);
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
            this.chkHeadquarter.Location = new System.Drawing.Point(21, 90);
            this.chkHeadquarter.Name = "chkHeadquarter";
            this.chkHeadquarter.Size = new System.Drawing.Size(130, 17);
            this.chkHeadquarter.TabIndex = 14;
            this.chkHeadquarter.Text = "Generate headquarter";
            this.chkHeadquarter.UseVisualStyleBackColor = true;
            this.chkHeadquarter.CheckedChanged += new System.EventHandler(this.chkHeadquarter_CheckedChanged);
            // 
            // txtHQName
            // 
            this.txtHQName.Location = new System.Drawing.Point(179, 87);
            this.txtHQName.Name = "txtHQName";
            this.txtHQName.Size = new System.Drawing.Size(257, 20);
            this.txtHQName.TabIndex = 15;
            this.txtHQName.Text = "hq";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctrlProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 340);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(781, 22);
            this.statusStrip1.TabIndex = 12;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lstLog
            // 
            this.lstLog.FormattingEnabled = true;
            this.lstLog.Location = new System.Drawing.Point(451, 166);
            this.lstLog.Name = "lstLog";
            this.lstLog.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstLog.Size = new System.Drawing.Size(309, 160);
            this.lstLog.TabIndex = 17;
            // 
            // eventsStatistics
            // 
            this.eventsStatistics.FormattingEnabled = true;
            this.eventsStatistics.Location = new System.Drawing.Point(451, 39);
            this.eventsStatistics.Name = "eventsStatistics";
            this.eventsStatistics.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.eventsStatistics.Size = new System.Drawing.Size(309, 121);
            this.eventsStatistics.TabIndex = 18;
            // 
            // LoadTestDataGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 362);
            this.Controls.Add(this.eventsStatistics);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.defaultDatabaseName);
            this.Controls.Add(this.txtHQName);
            this.Controls.Add(this.chkHeadquarter);
            this.Controls.Add(this.generateCapiEvents);
            this.Controls.Add(this.chkGenerateSnapshoots);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.chkSetAnswers);
            this.Controls.Add(this.clearDatabase);
            this.Controls.Add(this.generateSupervisorEvents);
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
            this.MaximumSize = new System.Drawing.Size(797, 401);
            this.MinimumSize = new System.Drawing.Size(797, 401);
            this.Name = "LoadTestDataGenerator";
            this.Text = "Load Test Data Generator";
            this.Load += new System.EventHandler(this.LoadTestDataGenerator_Load);
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
        private System.Windows.Forms.CheckBox generateSupervisorEvents;
        private System.Windows.Forms.CheckBox clearDatabase;
        private System.Windows.Forms.CheckBox chkSetAnswers;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar ctrlProgress;
        private System.Windows.Forms.CheckBox generateCapiEvents;
        private System.Windows.Forms.TextBox defaultDatabaseName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkGenerateSnapshoots;
        private System.Windows.Forms.CheckBox chkHeadquarter;
        private System.Windows.Forms.TextBox txtHQName;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.ListBox eventsStatistics;
    }
}

