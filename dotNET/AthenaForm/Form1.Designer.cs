namespace Rigsarkiv.Athena
{
    partial class Form1
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.mainTablesListBox = new System.Windows.Forms.ListBox();
            this.dataValues = new System.Windows.Forms.DataGridView();
            this.variableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.variableType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.variableValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.previewButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.codeTablesListBox = new System.Windows.Forms.ListBox();
            this.previewProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataValues)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // mainTablesListBox
            // 
            this.mainTablesListBox.FormattingEnabled = true;
            this.mainTablesListBox.ItemHeight = 20;
            this.mainTablesListBox.Location = new System.Drawing.Point(16, 97);
            this.mainTablesListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mainTablesListBox.Name = "mainTablesListBox";
            this.mainTablesListBox.Size = new System.Drawing.Size(223, 384);
            this.mainTablesListBox.TabIndex = 3;
            this.mainTablesListBox.SelectedIndexChanged += new System.EventHandler(this.mainTablesListBox_SelectedIndexChanged);
            // 
            // dataValues
            // 
            this.dataValues.AllowUserToAddRows = false;
            this.dataValues.AllowUserToDeleteRows = false;
            this.dataValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.variableName,
            this.variableType,
            this.variableValue,
            this.columnType,
            this.columnValue});
            this.dataValues.Location = new System.Drawing.Point(520, 97);
            this.dataValues.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataValues.Name = "dataValues";
            this.dataValues.ReadOnly = true;
            this.dataValues.Size = new System.Drawing.Size(745, 384);
            this.dataValues.TabIndex = 5;
            // 
            // variableName
            // 
            this.variableName.HeaderText = "Variabelnavn";
            this.variableName.Name = "variableName";
            this.variableName.ReadOnly = true;
            // 
            // variableType
            // 
            this.variableType.HeaderText = "Datatype (orginal)";
            this.variableType.Name = "variableType";
            this.variableType.ReadOnly = true;
            // 
            // variableValue
            // 
            this.variableValue.HeaderText = "Værdi";
            this.variableValue.Name = "variableValue";
            this.variableValue.ReadOnly = true;
            // 
            // columnType
            // 
            this.columnType.HeaderText = "Datatype (SQL)";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            // 
            // columnValue
            // 
            this.columnValue.HeaderText = "Værdi";
            this.columnValue.Name = "columnValue";
            this.columnValue.ReadOnly = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(655, 492);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 35);
            this.button1.TabIndex = 6;
            this.button1.Text = "Auto Kørsel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(16, 504);
            this.previewButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(112, 35);
            this.previewButton.TabIndex = 7;
            this.previewButton.Text = "Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(786, 492);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 35);
            this.button3.TabIndex = 8;
            this.button3.Text = "Forrige";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(908, 492);
            this.button4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(112, 35);
            this.button4.TabIndex = 9;
            this.button4.Text = "Næste";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(1114, 492);
            this.button5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(112, 35);
            this.button5.TabIndex = 10;
            this.button5.Text = "Konvertér";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(989, 66);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(148, 26);
            this.textBox2.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(943, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "Søg";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(179, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 29);
            this.label1.TabIndex = 13;
            this.label1.Text = "TABELLER";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(66, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 20);
            this.label4.TabIndex = 15;
            this.label4.Text = "Hovedtabeller";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(323, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 20);
            this.label5.TabIndex = 16;
            this.label5.Text = "Kodetabeller";
            // 
            // codeTablesListBox
            // 
            this.codeTablesListBox.FormattingEnabled = true;
            this.codeTablesListBox.ItemHeight = 20;
            this.codeTablesListBox.Location = new System.Drawing.Point(269, 97);
            this.codeTablesListBox.Name = "codeTablesListBox";
            this.codeTablesListBox.Size = new System.Drawing.Size(218, 384);
            this.codeTablesListBox.TabIndex = 17;
            this.codeTablesListBox.SelectedIndexChanged += new System.EventHandler(this.codeTablesListBox_SelectedIndexChanged);
            // 
            // previewProgressBar
            // 
            this.previewProgressBar.Location = new System.Drawing.Point(16, 556);
            this.previewProgressBar.Name = "previewProgressBar";
            this.previewProgressBar.Size = new System.Drawing.Size(471, 23);
            this.previewProgressBar.TabIndex = 18;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1297, 605);
            this.Controls.Add(this.previewProgressBar);
            this.Controls.Add(this.codeTablesListBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataValues);
            this.Controls.Add(this.mainTablesListBox);
            this.Name = "Form1";
            this.Text = "ASTA - Athena";
            ((System.ComponentModel.ISupportInitialize)(this.dataValues)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ListBox mainTablesListBox;
        private System.Windows.Forms.DataGridView dataValues;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableType;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnValue;
        private System.Windows.Forms.ListBox codeTablesListBox;
        private System.Windows.Forms.ProgressBar previewProgressBar;
    }
}

