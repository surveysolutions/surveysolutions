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
            this.label1.Location = new System.Drawing.Point(12, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of surveys";
            // 
            // surveys_amount
            // 
            this.surveys_amount.Location = new System.Drawing.Point(163, 104);
            this.surveys_amount.Name = "surveys_amount";
            this.surveys_amount.Size = new System.Drawing.Size(257, 20);
            this.surveys_amount.TabIndex = 2;
            this.surveys_amount.Text = "2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Questionnaire template file";
            // 
            // templatePath
            // 
            this.templatePath.Location = new System.Drawing.Point(163, 69);
            this.templatePath.Name = "templatePath";
            this.templatePath.Size = new System.Drawing.Size(257, 20);
            this.templatePath.TabIndex = 4;
            this.templatePath.Text = "C:\\Users\\Вячеслав\\Downloads\\sl.txt";
            this.templatePath.Enter += new System.EventHandler(this.templatePath_Enter);
            // 
            // supervisorsCount
            // 
            this.supervisorsCount.Location = new System.Drawing.Point(163, 144);
            this.supervisorsCount.Name = "supervisorsCount";
            this.supervisorsCount.Size = new System.Drawing.Size(257, 20);
            this.supervisorsCount.TabIndex = 6;
            this.supervisorsCount.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 144);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Supervisors count";
            // 
            // interviewersCount
            // 
            this.interviewersCount.Location = new System.Drawing.Point(163, 185);
            this.interviewersCount.Name = "interviewersCount";
            this.interviewersCount.Size = new System.Drawing.Size(257, 20);
            this.interviewersCount.TabIndex = 8;
            this.interviewersCount.Text = "2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Interviewers count";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(18, 43);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(163, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Generate supervisor\'s events";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(18, 13);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(97, 17);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "Clear database";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // LoadTestDataGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 477);
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
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}

