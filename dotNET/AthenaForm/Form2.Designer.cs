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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.mainTablesListBox = new System.Windows.Forms.ListBox();
            this.dataValues = new System.Windows.Forms.DataGridView();
            this.variableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.variableType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.variableValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Differences = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Errors = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.nextErrorButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.valueRichTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataValues)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // mainTablesListBox
            // 
            this.mainTablesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.mainTablesListBox.FormattingEnabled = true;
            this.mainTablesListBox.Location = new System.Drawing.Point(11, 63);
            this.mainTablesListBox.Name = "mainTablesListBox";
            this.mainTablesListBox.Size = new System.Drawing.Size(150, 225);
            this.mainTablesListBox.TabIndex = 3;
            this.mainTablesListBox.SelectedIndexChanged += new System.EventHandler(this.mainTablesListBox_SelectedIndexChanged);
            // 
            // dataValues
            // 
            this.dataValues.AllowUserToAddRows = false;
            this.dataValues.AllowUserToDeleteRows = false;
            this.dataValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataValues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.variableName,
            this.variableType,
            this.variableValue,
            this.columnType,
            this.columnValue,
            this.Differences,
            this.Errors});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataValues.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataValues.Location = new System.Drawing.Point(347, 63);
            this.dataValues.Name = "dataValues";
            this.dataValues.RowHeadersVisible = false;
            this.dataValues.RowTemplate.ReadOnly = true;
            this.dataValues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataValues.Size = new System.Drawing.Size(651, 292);
            this.dataValues.TabIndex = 5;
            this.dataValues.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataValues_CellClick);
            this.dataValues.SelectionChanged += new System.EventHandler(this.dataValues_SelectionChanged);
            // 
            // variableName
            // 
            this.variableName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.variableName.HeaderText = "Variabelnavn";
            this.variableName.Name = "variableName";
            this.variableName.ReadOnly = true;
            this.variableName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.variableName.Width = 75;
            // 
            // variableType
            // 
            this.variableType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.variableType.HeaderText = "Datatype (orginal)";
            this.variableType.Name = "variableType";
            this.variableType.ReadOnly = true;
            this.variableType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.variableType.Width = 96;
            // 
            // variableValue
            // 
            this.variableValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.variableValue.HeaderText = "Værdi";
            this.variableValue.Name = "variableValue";
            this.variableValue.ReadOnly = true;
            this.variableValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.HeaderText = "Datatype (SQL)";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            this.columnType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnType.Width = 86;
            // 
            // columnValue
            // 
            this.columnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnValue.HeaderText = "Værdi";
            this.columnValue.Name = "columnValue";
            this.columnValue.ReadOnly = true;
            this.columnValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Differences
            // 
            this.Differences.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Differences.FillWeight = 35F;
            this.Differences.HeaderText = "Forskelle";
            this.Differences.Name = "Differences";
            this.Differences.ReadOnly = true;
            this.Differences.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Errors
            // 
            this.Errors.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Errors.FillWeight = 35F;
            this.Errors.HeaderText = "Fejl";
            this.Errors.Name = "Errors";
            this.Errors.ReadOnly = true;
            this.Errors.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // prevButton
            // 
            this.prevButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.prevButton.Enabled = false;
            this.prevButton.Location = new System.Drawing.Point(484, 34);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(47, 23);
            this.prevButton.TabIndex = 8;
            this.prevButton.Text = "Forrige";
            this.prevButton.UseVisualStyleBackColor = true;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.nextButton.Enabled = false;
            this.nextButton.Location = new System.Drawing.Point(537, 34);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(47, 22);
            this.nextButton.TabIndex = 9;
            this.nextButton.Text = "Næste";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // IndexButton
            // 
            this.IndexButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.IndexButton.Location = new System.Drawing.Point(347, 361);
            this.IndexButton.Name = "IndexButton";
            this.IndexButton.Size = new System.Drawing.Size(105, 23);
            this.IndexButton.TabIndex = 10;
            this.IndexButton.Text = "Afslut konvertering";
            this.IndexButton.UseVisualStyleBackColor = true;
            this.IndexButton.Click += new System.EventHandler(this.IndexButton_Click);
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.searchTextBox.Location = new System.Drawing.Point(761, 34);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(100, 20);
            this.searchTextBox.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(119, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 20);
            this.label1.TabIndex = 13;
            this.label1.Text = "TABELLER";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 43);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Hovedtabeller";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(215, 43);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Kodetabeller";
            // 
            // codeTablesListBox
            // 
            this.codeTablesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.codeTablesListBox.FormattingEnabled = true;
            this.codeTablesListBox.Location = new System.Drawing.Point(179, 63);
            this.codeTablesListBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.codeTablesListBox.Name = "codeTablesListBox";
            this.codeTablesListBox.Size = new System.Drawing.Size(147, 225);
            this.codeTablesListBox.TabIndex = 17;
            this.codeTablesListBox.SelectedIndexChanged += new System.EventHandler(this.codeTablesListBox_SelectedIndexChanged);
            // 
            // rowLabel
            // 
            this.rowLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rowLabel.AutoSize = true;
            this.rowLabel.Location = new System.Drawing.Point(347, 43);
            this.rowLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.rowLabel.Name = "rowLabel";
            this.rowLabel.Size = new System.Drawing.Size(50, 13);
            this.rowLabel.TabIndex = 19;
            this.rowLabel.Text = "rowLabel";
            // 
            // searchButton
            // 
            this.searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.searchButton.Enabled = false;
            this.searchButton.Location = new System.Drawing.Point(865, 34);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(35, 23);
            this.searchButton.TabIndex = 20;
            this.searchButton.Text = "Søg";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // tableInfoLabel
            // 
            this.tableInfoLabel.AutoSize = true;
            this.tableInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableInfoLabel.Location = new System.Drawing.Point(346, 6);
            this.tableInfoLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.tableInfoLabel.Name = "tableInfoLabel";
            this.tableInfoLabel.Size = new System.Drawing.Size(111, 20);
            this.tableInfoLabel.TabIndex = 21;
            this.tableInfoLabel.Text = "tableInfoLabel";
            // 
            // rowErrorsLabel
            // 
            this.rowErrorsLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rowErrorsLabel.AutoSize = true;
            this.rowErrorsLabel.Location = new System.Drawing.Point(912, 44);
            this.rowErrorsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.rowErrorsLabel.Name = "rowErrorsLabel";
            this.rowErrorsLabel.Size = new System.Drawing.Size(77, 13);
            this.rowErrorsLabel.TabIndex = 22;
            this.rowErrorsLabel.Text = "rowErrorsLabel";
            // 
            // tableErrorsLabel
            // 
            this.tableErrorsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableErrorsLabel.AutoSize = true;
            this.tableErrorsLabel.Location = new System.Drawing.Point(912, 366);
            this.tableErrorsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.tableErrorsLabel.Name = "tableErrorsLabel";
            this.tableErrorsLabel.Size = new System.Drawing.Size(83, 13);
            this.tableErrorsLabel.TabIndex = 23;
            this.tableErrorsLabel.Text = "tableErrorsLabel";
            // 
            // nextErrorButton
            // 
            this.nextErrorButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.nextErrorButton.Enabled = false;
            this.nextErrorButton.Location = new System.Drawing.Point(643, 35);
            this.nextErrorButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nextErrorButton.Name = "nextErrorButton";
            this.nextErrorButton.Size = new System.Drawing.Size(66, 20);
            this.nextErrorButton.TabIndex = 25;
            this.nextErrorButton.Text = "Næste fejl";
            this.nextErrorButton.UseVisualStyleBackColor = true;
            this.nextErrorButton.Click += new System.EventHandler(this.nextErrorButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 296);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Værdi";
            // 
            // valueRichTextBox
            // 
            this.valueRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueRichTextBox.Location = new System.Drawing.Point(8, 311);
            this.valueRichTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.valueRichTextBox.Name = "valueRichTextBox";
            this.valueRichTextBox.Size = new System.Drawing.Size(318, 74);
            this.valueRichTextBox.TabIndex = 27;
            this.valueRichTextBox.Text = "";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 393);
            this.Controls.Add(this.valueRichTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nextErrorButton);
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
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form2";
            this.Text = "ASTA - {0} - Kontrol af konvertering";
            this.Shown += new System.EventHandler(this.Form2_Shown);
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
        private System.Windows.Forms.Label rowErrorsLabel;
        private System.Windows.Forms.Label tableErrorsLabel;
        private System.Windows.Forms.Button nextErrorButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableType;
        private System.Windows.Forms.DataGridViewTextBoxColumn variableValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Differences;
        private System.Windows.Forms.DataGridViewTextBoxColumn Errors;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox valueRichTextBox;
    }
}

