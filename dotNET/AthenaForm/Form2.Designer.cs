namespace Rigsarkiv.AthenaForm
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.mainTablesListBox = new System.Windows.Forms.ListBox();
            this.dataValues = new System.Windows.Forms.DataGridView();
            this.variableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.variableType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.variableValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.prevButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.IndexButton = new System.Windows.Forms.Button();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.codeTablesListBox = new System.Windows.Forms.ListBox();
            this.rowLabel = new System.Windows.Forms.Label();
            this.searchButton = new System.Windows.Forms.Button();
            this.tableInfoLabel = new System.Windows.Forms.Label();
            this.rowErrorsLabel = new System.Windows.Forms.Label();
            this.tableErrorsLabel = new System.Windows.Forms.Label();
            this.reportButton = new System.Windows.Forms.Button();
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
            this.dataValues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
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
            this.dataValues.RowTemplate.ReadOnly = true;
            this.dataValues.Size = new System.Drawing.Size(745, 384);
            this.dataValues.TabIndex = 5;
            // 
            // variableName
            // 
            this.variableName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.variableName.HeaderText = "Variabelnavn";
            this.variableName.Name = "variableName";
            this.variableName.Width = 137;
            // 
            // variableType
            // 
            this.variableType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.variableType.HeaderText = "Datatype (orginal)";
            this.variableType.Name = "variableType";
            this.variableType.Width = 157;
            // 
            // variableValue
            // 
            this.variableValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.variableValue.HeaderText = "Værdi";
            this.variableValue.Name = "variableValue";
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.HeaderText = "Datatype (SQL)";
            this.columnType.Name = "columnType";
            this.columnType.Width = 143;
            // 
            // columnValue
            // 
            this.columnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnValue.HeaderText = "Værdi";
            this.columnValue.Name = "columnValue";
            // 
            // prevButton
            // 
            this.prevButton.Enabled = false;
            this.prevButton.Location = new System.Drawing.Point(726, 52);
            this.prevButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(71, 35);
            this.prevButton.TabIndex = 8;
            this.prevButton.Text = "Forrige";
            this.prevButton.UseVisualStyleBackColor = true;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Enabled = false;
            this.nextButton.Location = new System.Drawing.Point(805, 52);
            this.nextButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(71, 34);
            this.nextButton.TabIndex = 9;
            this.nextButton.Text = "Næste";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // IndexButton
            // 
            this.IndexButton.Location = new System.Drawing.Point(881, 493);
            this.IndexButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.IndexButton.Name = "IndexButton";
            this.IndexButton.Size = new System.Drawing.Size(158, 35);
            this.IndexButton.TabIndex = 10;
            this.IndexButton.Text = "Afslut konvertering";
            this.IndexButton.UseVisualStyleBackColor = true;
            this.IndexButton.Click += new System.EventHandler(this.IndexButton_Click);
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(923, 52);
            this.searchTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(148, 26);
            this.searchTextBox.TabIndex = 11;
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
            // rowLabel
            // 
            this.rowLabel.AutoSize = true;
            this.rowLabel.Location = new System.Drawing.Point(520, 66);
            this.rowLabel.Name = "rowLabel";
            this.rowLabel.Size = new System.Drawing.Size(73, 20);
            this.rowLabel.TabIndex = 19;
            this.rowLabel.Text = "rowLabel";
            // 
            // searchButton
            // 
            this.searchButton.Enabled = false;
            this.searchButton.Location = new System.Drawing.Point(1079, 52);
            this.searchButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(53, 35);
            this.searchButton.TabIndex = 20;
            this.searchButton.Text = "Søg";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // tableInfoLabel
            // 
            this.tableInfoLabel.AutoSize = true;
            this.tableInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableInfoLabel.Location = new System.Drawing.Point(519, 9);
            this.tableInfoLabel.Name = "tableInfoLabel";
            this.tableInfoLabel.Size = new System.Drawing.Size(165, 29);
            this.tableInfoLabel.TabIndex = 21;
            this.tableInfoLabel.Text = "tableInfoLabel";
            // 
            // rowErrorsLabel
            // 
            this.rowErrorsLabel.AutoSize = true;
            this.rowErrorsLabel.Location = new System.Drawing.Point(1149, 67);
            this.rowErrorsLabel.Name = "rowErrorsLabel";
            this.rowErrorsLabel.Size = new System.Drawing.Size(116, 20);
            this.rowErrorsLabel.TabIndex = 22;
            this.rowErrorsLabel.Text = "rowErrorsLabel";
            // 
            // tableErrorsLabel
            // 
            this.tableErrorsLabel.AutoSize = true;
            this.tableErrorsLabel.Location = new System.Drawing.Point(1139, 492);
            this.tableErrorsLabel.Name = "tableErrorsLabel";
            this.tableErrorsLabel.Size = new System.Drawing.Size(126, 20);
            this.tableErrorsLabel.TabIndex = 23;
            this.tableErrorsLabel.Text = "tableErrorsLabel";
            // 
            // reportButton
            // 
            this.reportButton.Location = new System.Drawing.Point(774, 492);
            this.reportButton.Name = "reportButton";
            this.reportButton.Size = new System.Drawing.Size(84, 36);
            this.reportButton.TabIndex = 24;
            this.reportButton.Text = "Report";
            this.reportButton.UseVisualStyleBackColor = true;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1297, 605);
            this.Controls.Add(this.reportButton);
            this.Controls.Add(this.tableErrorsLabel);
            this.Controls.Add(this.rowErrorsLabel);
            this.Controls.Add(this.tableInfoLabel);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.rowLabel);
            this.Controls.Add(this.codeTablesListBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.IndexButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.prevButton);
            this.Controls.Add(this.dataValues);
            this.Controls.Add(this.mainTablesListBox);
            this.Name = "Form2";
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
        private System.Windows.Forms.Button prevButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button IndexButton;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox codeTablesListBox;
        private System.Windows.Forms.Label rowLabel;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Label tableInfoLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableType;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnValue;
        private System.Windows.Forms.Label rowErrorsLabel;
        private System.Windows.Forms.Label tableErrorsLabel;
        private System.Windows.Forms.Button reportButton;
    }
}

