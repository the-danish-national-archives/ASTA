namespace Rigsarkiv.StyxForm
{
    partial class Form2
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
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.removeButton = new System.Windows.Forms.Button();
            this.codeTablesListBox = new System.Windows.Forms.ListBox();
            this.mainTablesListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tableInfoLabel = new System.Windows.Forms.Label();
            this.convertButton = new System.Windows.Forms.Button();
            this.outputRichTextBox = new System.Windows.Forms.RichTextBox();
            this.reportButton = new System.Windows.Forms.Button();
            this.logButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(16, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(162, 29);
            this.label4.TabIndex = 17;
            this.label4.Text = "Byg relationer";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.removeButton);
            this.groupBox1.Controls.Add(this.codeTablesListBox);
            this.groupBox1.Controls.Add(this.mainTablesListBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(21, 102);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(674, 514);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "TABELLER";
            // 
            // removeButton
            // 
            this.removeButton.Enabled = false;
            this.removeButton.Location = new System.Drawing.Point(313, 267);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(51, 30);
            this.removeButton.TabIndex = 32;
            this.removeButton.Text = "<<";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // codeTablesListBox
            // 
            this.codeTablesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.codeTablesListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.codeTablesListBox.FormattingEnabled = true;
            this.codeTablesListBox.ItemHeight = 20;
            this.codeTablesListBox.Location = new System.Drawing.Point(387, 80);
            this.codeTablesListBox.Name = "codeTablesListBox";
            this.codeTablesListBox.Size = new System.Drawing.Size(271, 404);
            this.codeTablesListBox.TabIndex = 17;
            this.codeTablesListBox.SelectedIndexChanged += new System.EventHandler(this.codeTablesListBox_SelectedIndexChanged);
            // 
            // mainTablesListBox
            // 
            this.mainTablesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.mainTablesListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainTablesListBox.FormattingEnabled = true;
            this.mainTablesListBox.ItemHeight = 20;
            this.mainTablesListBox.Location = new System.Drawing.Point(16, 80);
            this.mainTablesListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainTablesListBox.Name = "mainTablesListBox";
            this.mainTablesListBox.Size = new System.Drawing.Size(273, 404);
            this.mainTablesListBox.TabIndex = 3;
            this.mainTablesListBox.SelectedIndexChanged += new System.EventHandler(this.mainTablesListBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 20);
            this.label1.TabIndex = 15;
            this.label1.Text = "Hovedtabeller";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(436, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 20);
            this.label5.TabIndex = 16;
            this.label5.Text = "Kodetabeller";
            // 
            // tableInfoLabel
            // 
            this.tableInfoLabel.AutoSize = true;
            this.tableInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableInfoLabel.Location = new System.Drawing.Point(16, 55);
            this.tableInfoLabel.Name = "tableInfoLabel";
            this.tableInfoLabel.Size = new System.Drawing.Size(165, 29);
            this.tableInfoLabel.TabIndex = 32;
            this.tableInfoLabel.Text = "tableInfoLabel";
            // 
            // convertButton
            // 
            this.convertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.convertButton.Location = new System.Drawing.Point(551, 631);
            this.convertButton.Name = "convertButton";
            this.convertButton.Size = new System.Drawing.Size(144, 32);
            this.convertButton.TabIndex = 33;
            this.convertButton.Text = "Konverter til DIP";
            this.convertButton.UseVisualStyleBackColor = true;
            this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
            // 
            // outputRichTextBox
            // 
            this.outputRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputRichTextBox.Location = new System.Drawing.Point(21, 679);
            this.outputRichTextBox.Name = "outputRichTextBox";
            this.outputRichTextBox.Size = new System.Drawing.Size(674, 180);
            this.outputRichTextBox.TabIndex = 34;
            this.outputRichTextBox.Text = "";
            // 
            // reportButton
            // 
            this.reportButton.Enabled = false;
            this.reportButton.Location = new System.Drawing.Point(572, 870);
            this.reportButton.Name = "reportButton";
            this.reportButton.Size = new System.Drawing.Size(123, 32);
            this.reportButton.TabIndex = 36;
            this.reportButton.Text = "Vis Rapport";
            this.reportButton.UseVisualStyleBackColor = true;
            this.reportButton.Click += new System.EventHandler(this.reportButton_Click);
            // 
            // logButton
            // 
            this.logButton.Enabled = false;
            this.logButton.Location = new System.Drawing.Point(456, 870);
            this.logButton.Name = "logButton";
            this.logButton.Size = new System.Drawing.Size(110, 32);
            this.logButton.TabIndex = 35;
            this.logButton.Text = "Vis Log";
            this.logButton.UseVisualStyleBackColor = true;
            this.logButton.Click += new System.EventHandler(this.logButton_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 907);
            this.Controls.Add(this.reportButton);
            this.Controls.Add(this.logButton);
            this.Controls.Add(this.outputRichTextBox);
            this.Controls.Add(this.convertButton);
            this.Controls.Add(this.tableInfoLabel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form2";
            this.Text = "ASTA";
            this.Shown += new System.EventHandler(this.Form2_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox codeTablesListBox;
        private System.Windows.Forms.ListBox mainTablesListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Label tableInfoLabel;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.RichTextBox outputRichTextBox;
        private System.Windows.Forms.Button reportButton;
        private System.Windows.Forms.Button logButton;
    }
}