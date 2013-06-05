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
            this.generateCapiEvents = new System.Windows.Forms.CheckBox();
            this.defaultDatabaseName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // generate
            // 
            this.generate.Location = new System.Drawing.Point(438, 399);
            this.generate.Name = "generate";
            this.generate.Size = new System.Drawing.Size(108, 66);
            this.generate.TabIndex = 0;
            this.generate.Text = "Generate";
            this.generate.UseVisualStyleBackColor = true;
            this.generate.Click += new System.EventHandler(this.generate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 127);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of surveys";
            // 
            // surveys_amount
            // 
            this.surveys_amount.Location = new System.Drawing.Point(163, 127);
            this.surveys_amount.Name = "surveys_amount";
            this.surveys_amount.Size = new System.Drawing.Size(257, 20);
            this.surveys_amount.TabIndex = 2;
            this.surveys_amount.Text = "2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Questionnaire template file";
            // 
            // templatePath
            // 
            this.templatePath.Location = new System.Drawing.Point(163, 92);
            this.templatePath.Name = "templatePath";
            this.templatePath.Size = new System.Drawing.Size(257, 20);
            this.templatePath.TabIndex = 4;
            this.templatePath.Text = "C:\\Users\\Вячеслав\\Downloads\\sl.txt";
            this.templatePath.Enter += new System.EventHandler(this.templatePath_Enter);
            // 
            // supervisorsCount
            // 
            this.supervisorsCount.Location = new System.Drawing.Point(163, 167);
            this.supervisorsCount.Name = "supervisorsCount";
            this.supervisorsCount.Size = new System.Drawing.Size(257, 20);
            this.supervisorsCount.TabIndex = 6;
            this.supervisorsCount.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 167);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Supervisors count";
            // 
            // interviewersCount
            // 
            this.interviewersCount.Location = new System.Drawing.Point(163, 208);
            this.interviewersCount.Name = "interviewersCount";
            this.interviewersCount.Size = new System.Drawing.Size(257, 20);
            this.interviewersCount.TabIndex = 8;
            this.interviewersCount.Text = "2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 208);
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
            this.generateSupervisorEvents.Location = new System.Drawing.Point(15, 66);
            this.generateSupervisorEvents.Name = "generateSupervisorEvents";
            this.generateSupervisorEvents.Size = new System.Drawing.Size(163, 17);
            this.generateSupervisorEvents.TabIndex = 9;
            this.generateSupervisorEvents.Text = "Generate supervisor\'s events";
            this.generateSupervisorEvents.UseVisualStyleBackColor = true;
            // 
            // clearDatabase
            // 
            this.clearDatabase.AutoSize = true;
            this.clearDatabase.Checked = true;
            this.clearDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.clearDatabase.Location = new System.Drawing.Point(15, 36);
            this.clearDatabase.Name = "clearDatabase";
            this.clearDatabase.Size = new System.Drawing.Size(97, 17);
            this.clearDatabase.TabIndex = 10;
            this.clearDatabase.Text = "Clear database";
            this.clearDatabase.UseVisualStyleBackColor = true;
            // 
            // generateCapiEvents
            // 
            this.generateCapiEvents.AutoSize = true;
            this.generateCapiEvents.Checked = true;
            this.generateCapiEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.generateCapiEvents.Location = new System.Drawing.Point(15, 253);
            this.generateCapiEvents.Name = "generateCapiEvents";
            this.generateCapiEvents.Size = new System.Drawing.Size(139, 17);
            this.generateCapiEvents.TabIndex = 11;
            this.generateCapiEvents.Text = "Generate CAPI\'s events";
            this.generateCapiEvents.UseVisualStyleBackColor = true;
            // 
            // defaultDatabaseName
            // 
            this.defaultDatabaseName.Enabled = false;
            this.defaultDatabaseName.Location = new System.Drawing.Point(163, 13);
            this.defaultDatabaseName.Name = "defaultDatabaseName";
            this.defaultDatabaseName.Size = new System.Drawing.Size(257, 20);
            this.defaultDatabaseName.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Database name";
            // 
            // LoadTestDataGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 477);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.defaultDatabaseName);
            this.Controls.Add(this.generateCapiEvents);
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
            this.Name = "LoadTestDataGenerator";
            this.Text = "Load Test Data Generator";
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
        private System.Windows.Forms.CheckBox generateCapiEvents;
        private System.Windows.Forms.TextBox defaultDatabaseName;
        private System.Windows.Forms.Label label5;
    }
}

