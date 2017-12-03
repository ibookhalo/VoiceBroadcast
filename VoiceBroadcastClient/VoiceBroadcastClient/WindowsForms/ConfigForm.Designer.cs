namespace VoiceBroadcastClient
{
    partial class ConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.tbServerIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nudServerPort = new System.Windows.Forms.NumericUpDown();
            this.ok = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbClientName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbOutput = new System.Windows.Forms.ComboBox();
            this.cbInput = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudServerPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbServerIP
            // 
            this.tbServerIP.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbServerIP.Location = new System.Drawing.Point(130, 87);
            this.tbServerIP.Name = "tbServerIP";
            this.tbServerIP.Size = new System.Drawing.Size(275, 33);
            this.tbServerIP.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(17, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 26);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server-IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 26);
            this.label2.TabIndex = 2;
            this.label2.Text = "Server-Port";
            // 
            // nudServerPort
            // 
            this.nudServerPort.Font = new System.Drawing.Font("Arial Narrow", 11F);
            this.nudServerPort.Location = new System.Drawing.Point(130, 134);
            this.nudServerPort.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudServerPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.nudServerPort.Name = "nudServerPort";
            this.nudServerPort.Size = new System.Drawing.Size(275, 33);
            this.nudServerPort.TabIndex = 3;
            this.nudServerPort.Value = new decimal(new int[] {
            6666,
            0,
            0,
            0});
            // 
            // ok
            // 
            this.ok.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ok.Location = new System.Drawing.Point(113, 405);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(213, 48);
            this.ok.TabIndex = 4;
            this.ok.Text = "Übernehmen";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(18, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 26);
            this.label3.TabIndex = 6;
            this.label3.Text = "Clientname";
            // 
            // tbClientName
            // 
            this.tbClientName.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbClientName.Location = new System.Drawing.Point(131, 42);
            this.tbClientName.Name = "tbClientName";
            this.tbClientName.Size = new System.Drawing.Size(275, 33);
            this.tbClientName.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbOutput);
            this.groupBox1.Controls.Add(this.cbInput);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(12, 221);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(425, 158);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Audio";
            // 
            // cbOutput
            // 
            this.cbOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOutput.Font = new System.Drawing.Font("Arial Narrow", 11F);
            this.cbOutput.FormattingEnabled = true;
            this.cbOutput.Location = new System.Drawing.Point(127, 91);
            this.cbOutput.Name = "cbOutput";
            this.cbOutput.Size = new System.Drawing.Size(275, 34);
            this.cbOutput.TabIndex = 14;
            // 
            // cbInput
            // 
            this.cbInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInput.Font = new System.Drawing.Font("Arial Narrow", 11F);
            this.cbInput.FormattingEnabled = true;
            this.cbInput.Location = new System.Drawing.Point(127, 41);
            this.cbInput.Name = "cbInput";
            this.cbInput.Size = new System.Drawing.Size(275, 34);
            this.cbInput.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 26);
            this.label5.TabIndex = 12;
            this.label5.Text = "Ausgang";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial Narrow", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(15, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 26);
            this.label4.TabIndex = 11;
            this.label4.Text = "Eingang";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.tbServerIP);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.tbClientName);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.nudServerPort);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(425, 192);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 461);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Einstellungen";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.nudServerPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbServerIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudServerPort;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbClientName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbOutput;
        private System.Windows.Forms.ComboBox cbInput;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}