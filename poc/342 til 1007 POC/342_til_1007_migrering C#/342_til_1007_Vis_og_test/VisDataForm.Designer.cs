namespace _342_til_1007_vis_test_konverter
{
    partial class VisDataForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisDataForm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Feltnavn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Datatype = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data342 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data1007 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.checkBoxStopVedFejl = new System.Windows.Forms.CheckBox();
            this.buttonFromStart = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Feltnavn,
            this.Datatype,
            this.Data342,
            this.Data1007});
            this.dataGridView1.Location = new System.Drawing.Point(12, 29);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 60;
            this.dataGridView1.Size = new System.Drawing.Size(924, 555);
            this.dataGridView1.TabIndex = 0;
            // 
            // Feltnavn
            // 
            this.Feltnavn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Feltnavn.HeaderText = "Feltnavn";
            this.Feltnavn.Name = "Feltnavn";
            this.Feltnavn.ReadOnly = true;
            this.Feltnavn.Width = 73;
            // 
            // Datatype
            // 
            this.Datatype.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Datatype.HeaderText = "Datatype";
            this.Datatype.Name = "Datatype";
            this.Datatype.ReadOnly = true;
            this.Datatype.Width = 75;
            // 
            // Data342
            // 
            this.Data342.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Data342.FillWeight = 50F;
            this.Data342.HeaderText = "Data342";
            this.Data342.Name = "Data342";
            this.Data342.ReadOnly = true;
            // 
            // Data1007
            // 
            this.Data1007.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Data1007.FillWeight = 50F;
            this.Data1007.HeaderText = "Data1007";
            this.Data1007.Name = "Data1007";
            this.Data1007.ReadOnly = true;
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(450, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(62, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Næste";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button2
            // 
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button2.Location = new System.Drawing.Point(538, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(54, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Auto";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(596, 5);
            this.progressBar1.Minimum = 1;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(100, 18);
            this.progressBar1.TabIndex = 3;
            this.progressBar1.Value = 50;
            this.progressBar1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.progressBar1_MouseDown);
            this.progressBar1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.progressBar1_MouseMove);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(274, 20);
            this.textBox1.TabIndex = 4;
            // 
            // checkBoxStopVedFejl
            // 
            this.checkBoxStopVedFejl.AutoSize = true;
            this.checkBoxStopVedFejl.Checked = true;
            this.checkBoxStopVedFejl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxStopVedFejl.Location = new System.Drawing.Point(716, 7);
            this.checkBoxStopVedFejl.Name = "checkBoxStopVedFejl";
            this.checkBoxStopVedFejl.Size = new System.Drawing.Size(85, 17);
            this.checkBoxStopVedFejl.TabIndex = 5;
            this.checkBoxStopVedFejl.Text = "Stop ved fejl";
            this.checkBoxStopVedFejl.UseVisualStyleBackColor = true;
            // 
            // buttonFromStart
            // 
            this.buttonFromStart.Image = ((System.Drawing.Image)(resources.GetObject("buttonFromStart.Image")));
            this.buttonFromStart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFromStart.Location = new System.Drawing.Point(292, 3);
            this.buttonFromStart.Name = "buttonFromStart";
            this.buttonFromStart.Size = new System.Drawing.Size(68, 23);
            this.buttonFromStart.TabIndex = 6;
            this.buttonFromStart.Text = "Fra start";
            this.buttonFromStart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFromStart.UseVisualStyleBackColor = true;
            this.buttonFromStart.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button4.Location = new System.Drawing.Point(381, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(63, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "Tilbage";
            this.button4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // VisDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 596);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.buttonFromStart);
            this.Controls.Add(this.checkBoxStopVedFejl);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "VisDataForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VisDataForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VisDataForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Feltnavn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Datatype;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data342;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data1007;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox checkBoxStopVedFejl;
        private System.Windows.Forms.Button buttonFromStart;
        private System.Windows.Forms.Button button4;
    }
}