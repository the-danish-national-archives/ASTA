namespace Rigsarkiv.AthenaForm
{
    partial class Form3
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
            this.logButton = new System.Windows.Forms.Button();
            this.titlelabel = new System.Windows.Forms.Label();
            this.tablesDataGridView = new System.Windows.Forms.DataGridView();
            this.TextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BeforeColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AfterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.reportButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.rowsDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.columnsDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Variable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Errors = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.tablesDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // logButton
            // 
            this.logButton.Location = new System.Drawing.Point(33, 548);
            this.logButton.Name = "logButton";
            this.logButton.Size = new System.Drawing.Size(75, 32);
            this.logButton.TabIndex = 12;
            this.logButton.Text = "Log";
            this.logButton.UseVisualStyleBackColor = true;
            this.logButton.Click += new System.EventHandler(this.logButton_Click);
            // 
            // titlelabel
            // 
            this.titlelabel.AutoSize = true;
            this.titlelabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titlelabel.Location = new System.Drawing.Point(28, 29);
            this.titlelabel.Name = "titlelabel";
            this.titlelabel.Size = new System.Drawing.Size(307, 29);
            this.titlelabel.TabIndex = 14;
            this.titlelabel.Text = "{0} - Status for Konvertering";
            // 
            // tablesDataGridView
            // 
            this.tablesDataGridView.AllowUserToAddRows = false;
            this.tablesDataGridView.AllowUserToDeleteRows = false;
            this.tablesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tablesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TextColumn,
            this.BeforeColumn1,
            this.AfterColumn});
            this.tablesDataGridView.Location = new System.Drawing.Point(522, 85);
            this.tablesDataGridView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tablesDataGridView.Name = "tablesDataGridView";
            this.tablesDataGridView.ReadOnly = true;
            this.tablesDataGridView.RowHeadersVisible = false;
            this.tablesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.tablesDataGridView.Size = new System.Drawing.Size(466, 102);
            this.tablesDataGridView.TabIndex = 15;
            // 
            // TextColumn
            // 
            this.TextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TextColumn.FillWeight = 60F;
            this.TextColumn.HeaderText = "";
            this.TextColumn.Name = "TextColumn";
            this.TextColumn.ReadOnly = true;
            // 
            // BeforeColumn1
            // 
            this.BeforeColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.BeforeColumn1.FillWeight = 20F;
            this.BeforeColumn1.HeaderText = "Før";
            this.BeforeColumn1.Name = "BeforeColumn1";
            this.BeforeColumn1.ReadOnly = true;
            // 
            // AfterColumn
            // 
            this.AfterColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.AfterColumn.FillWeight = 20F;
            this.AfterColumn.HeaderText = "Efter";
            this.AfterColumn.Name = "AfterColumn";
            this.AfterColumn.ReadOnly = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(518, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 20);
            this.label4.TabIndex = 16;
            this.label4.Text = "Antal tabeller";
            // 
            // reportButton
            // 
            this.reportButton.Enabled = false;
            this.reportButton.Location = new System.Drawing.Point(33, 478);
            this.reportButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.reportButton.Name = "reportButton";
            this.reportButton.Size = new System.Drawing.Size(169, 35);
            this.reportButton.TabIndex = 17;
            this.reportButton.Text = "Konverteringsrapport";
            this.reportButton.UseVisualStyleBackColor = true;
            this.reportButton.Click += new System.EventHandler(this.reportButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(518, 192);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.TabIndex = 19;
            this.label2.Text = "Antal rækker";
            // 
            // rowsDataGridView
            // 
            this.rowsDataGridView.AllowUserToAddRows = false;
            this.rowsDataGridView.AllowUserToDeleteRows = false;
            this.rowsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.rowsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3});
            this.rowsDataGridView.Location = new System.Drawing.Point(522, 217);
            this.rowsDataGridView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rowsDataGridView.Name = "rowsDataGridView";
            this.rowsDataGridView.ReadOnly = true;
            this.rowsDataGridView.RowHeadersVisible = false;
            this.rowsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.rowsDataGridView.Size = new System.Drawing.Size(466, 157);
            this.rowsDataGridView.TabIndex = 18;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.FillWeight = 60F;
            this.dataGridViewTextBoxColumn1.HeaderText = "Tabel";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.FillWeight = 20F;
            this.dataGridViewTextBoxColumn2.HeaderText = "Før";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.FillWeight = 20F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Efter";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(220, 386);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(206, 20);
            this.label3.TabIndex = 21;
            this.label3.Text = "Optælling af konverteringfejl";
            // 
            // columnsDataGridView
            // 
            this.columnsDataGridView.AllowUserToAddRows = false;
            this.columnsDataGridView.AllowUserToDeleteRows = false;
            this.columnsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.columnsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn4,
            this.Variable,
            this.DataType,
            this.Errors});
            this.columnsDataGridView.Location = new System.Drawing.Point(224, 411);
            this.columnsDataGridView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.columnsDataGridView.Name = "columnsDataGridView";
            this.columnsDataGridView.ReadOnly = true;
            this.columnsDataGridView.RowHeadersVisible = false;
            this.columnsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.columnsDataGridView.Size = new System.Drawing.Size(764, 169);
            this.columnsDataGridView.TabIndex = 20;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.FillWeight = 40F;
            this.dataGridViewTextBoxColumn4.HeaderText = "Tabel";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // Variable
            // 
            this.Variable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Variable.FillWeight = 30F;
            this.Variable.HeaderText = "Variabel";
            this.Variable.Name = "Variable";
            this.Variable.ReadOnly = true;
            // 
            // DataType
            // 
            this.DataType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DataType.FillWeight = 15F;
            this.DataType.HeaderText = "Datatype";
            this.DataType.Name = "DataType";
            this.DataType.ReadOnly = true;
            // 
            // Errors
            // 
            this.Errors.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Errors.FillWeight = 15F;
            this.Errors.HeaderText = "Antal fejl";
            this.Errors.Name = "Errors";
            this.Errors.ReadOnly = true;
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 872);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.columnsDataGridView);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rowsDataGridView);
            this.Controls.Add(this.reportButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tablesDataGridView);
            this.Controls.Add(this.titlelabel);
            this.Controls.Add(this.logButton);
            this.Name = "Form3";
            this.Text = "ASTA";
            this.Shown += new System.EventHandler(this.Form3_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.tablesDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button logButton;
        private System.Windows.Forms.Label titlelabel;
        private System.Windows.Forms.DataGridView tablesDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn TextColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn BeforeColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn AfterColumn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button reportButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView rowsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView columnsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Variable;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Errors;
    }
}