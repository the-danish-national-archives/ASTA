namespace Athena_II_REM
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
            this.richTextBoxMetadata = new System.Windows.Forms.RichTextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonSkabAV = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.labelFD = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.shellTreeView1 = new Jam.Shell.ShellTreeView();
            this.button7 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBoxLOG = new System.Windows.Forms.RichTextBox();
            this.labelLog = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shellTreeView1)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBoxMetadata
            // 
            this.richTextBoxMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBoxMetadata.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMetadata.Location = new System.Drawing.Point(251, 33);
            this.richTextBoxMetadata.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxMetadata.Name = "richTextBoxMetadata";
            this.richTextBoxMetadata.Size = new System.Drawing.Size(320, 548);
            this.richTextBoxMetadata.TabIndex = 0;
            this.richTextBoxMetadata.Text = "";
            this.richTextBoxMetadata.WordWrap = false;
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeView1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView1.Location = new System.Drawing.Point(584, 33);
            this.treeView1.Margin = new System.Windows.Forms.Padding(2);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(333, 547);
            this.treeView1.TabIndex = 2;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(584, 9);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(60, 20);
            this.button3.TabIndex = 4;
            this.button3.Text = "Parse";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(647, 9);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(60, 20);
            this.button1.TabIndex = 5;
            this.button1.Text = "Udvid";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(711, 9);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(60, 20);
            this.button2.TabIndex = 6;
            this.button2.Text = "Kollaps";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 586);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1273, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(1258, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonSkabAV
            // 
            this.buttonSkabAV.Location = new System.Drawing.Point(848, 9);
            this.buttonSkabAV.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSkabAV.Name = "buttonSkabAV";
            this.buttonSkabAV.Size = new System.Drawing.Size(69, 20);
            this.buttonSkabAV.TabIndex = 8;
            this.buttonSkabAV.Text = "Skab 1007";
            this.buttonSkabAV.UseVisualStyleBackColor = true;
            this.buttonSkabAV.Click += new System.EventHandler(this.buttonSkabAV_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(338, 9);
            this.button5.Margin = new System.Windows.Forms.Padding(2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 20);
            this.button5.TabIndex = 9;
            this.button5.Text = "Gem som ...";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // labelFD
            // 
            this.labelFD.AutoSize = true;
            this.labelFD.Location = new System.Drawing.Point(16, 13);
            this.labelFD.Name = "labelFD";
            this.labelFD.Size = new System.Drawing.Size(98, 13);
            this.labelFD.TabIndex = 11;
            this.labelFD.Text = "<vælg FD. mappe>";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(251, 9);
            this.button6.Margin = new System.Windows.Forms.Padding(2);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(83, 20);
            this.button6.TabIndex = 12;
            this.button6.Text = "Åben datafil ...";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "txt";
            this.openFileDialog1.Filter = "Tekst|*.txt|Alle filer|*.*";
            this.openFileDialog1.InitialDirectory = "c:\\";
            this.openFileDialog1.Title = "Vælg fil ...";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Tekst|*.txt|Alle filer|*.*";
            this.saveFileDialog1.InitialDirectory = "c:\\";
            this.saveFileDialog1.Title = "Vælg fil ...";
            // 
            // shellTreeView1
            // 
            this.shellTreeView1.ChangeDelay = 500;
            this.shellTreeView1.FileSystemOnly = true;
            this.shellTreeView1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.shellTreeView1.Location = new System.Drawing.Point(9, 33);
            this.shellTreeView1.Margin = new System.Windows.Forms.Padding(2);
            this.shellTreeView1.Name = "shellTreeView1";
            this.shellTreeView1.RootedAt = Jam.Shell.ShellFolder.Drives;
            this.shellTreeView1.RootedAtFileSystemFolder = "";
            this.shellTreeView1.SelectedPath = "C:\\Dropbox\\SA Forskningsdata";
            this.shellTreeView1.ShowColorCompressed = System.Drawing.Color.Empty;
            this.shellTreeView1.ShowColorEncrypted = System.Drawing.Color.Empty;
            this.shellTreeView1.ShowRecycleBin = false;
            this.shellTreeView1.Size = new System.Drawing.Size(228, 380);
            this.shellTreeView1.Sorted = true;
            this.shellTreeView1.SpecialFolder = Jam.Shell.ShellFolder.FileSystemFolder;
            this.shellTreeView1.TabIndex = 13;
            this.shellTreeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.shellTreeView1_AfterSelect);
            // 
            // button7
            // 
            this.button7.Enabled = false;
            this.button7.Location = new System.Drawing.Point(485, 9);
            this.button7.Margin = new System.Windows.Forms.Padding(2);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(60, 20);
            this.button7.TabIndex = 14;
            this.button7.Text = "Åben ...";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Visible = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(9, 447);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(228, 134);
            this.listBox1.TabIndex = 15;
            this.listBox1.Click += new System.EventHandler(this.listBox1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 430);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Datasæt";
            // 
            // richTextBoxLOG
            // 
            this.richTextBoxLOG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxLOG.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxLOG.Location = new System.Drawing.Point(930, 32);
            this.richTextBoxLOG.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxLOG.Name = "richTextBoxLOG";
            this.richTextBoxLOG.Size = new System.Drawing.Size(332, 548);
            this.richTextBoxLOG.TabIndex = 17;
            this.richTextBoxLOG.Text = "";
            this.richTextBoxLOG.WordWrap = false;
            // 
            // labelLog
            // 
            this.labelLog.AutoSize = true;
            this.labelLog.Location = new System.Drawing.Point(927, 13);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(29, 13);
            this.labelLog.TabIndex = 18;
            this.labelLog.Text = "LOG";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1273, 608);
            this.Controls.Add(this.labelLog);
            this.Controls.Add(this.richTextBoxLOG);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.richTextBoxMetadata);
            this.Controls.Add(this.shellTreeView1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.labelFD);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.buttonSkabAV);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.treeView1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(830, 398);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Athena II";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shellTreeView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxMetadata;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button buttonSkabAV;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label labelFD;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private Jam.Shell.ShellTreeView shellTreeView1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBoxLOG;
        private System.Windows.Forms.Label labelLog;
    }
}

