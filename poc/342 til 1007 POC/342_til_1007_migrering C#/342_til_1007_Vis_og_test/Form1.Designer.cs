namespace _342_til_1007_vis_test_konverter
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
            this.shellTreeView1 = new Jam.Shell.ShellTreeView();
            this.listBoxDP = new System.Windows.Forms.ListBox();
            this.label_DP_liste = new System.Windows.Forms.Label();
            this.labelAV = new System.Windows.Forms.Label();
            this.labelTabeller = new System.Windows.Forms.Label();
            this.listBoxTable = new System.Windows.Forms.ListBox();
            this.labelGeninfo = new System.Windows.Forms.Label();
            this.listBoxGeninfo = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelSkaber = new System.Windows.Forms.Label();
            this.richTextBoxMetadata = new System.Windows.Forms.RichTextBox();
            this.labelMetadata = new System.Windows.Forms.Label();
            this.richTextBoxArkver = new System.Windows.Forms.RichTextBox();
            this.listBoxSkaber = new System.Windows.Forms.ListBox();
            this.labelDokumenter = new System.Windows.Forms.Label();
            this.listBoxDokumenter = new System.Windows.Forms.ListBox();
            this.buttonParse = new System.Windows.Forms.Button();
            this.buttonContext = new System.Windows.Forms.Button();
            this.buttonTabeller = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.buttonFolders = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelLog = new System.Windows.Forms.Label();
            this.richTextBoxLOG = new System.Windows.Forms.RichTextBox();
            this.labelFilmap = new System.Windows.Forms.Label();
            this.listBoxFilmap = new System.Windows.Forms.ListBox();
            this.comboBoxDest = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonSkab1007 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.buttonMetadataTekstSøg = new System.Windows.Forms.Button();
            this.textBoxMetadataSøg = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.shellTreeView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // shellTreeView1
            // 
            this.shellTreeView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.shellTreeView1.ChangeDelay = 500;
            this.shellTreeView1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.shellTreeView1.Location = new System.Drawing.Point(11, 25);
            this.shellTreeView1.Name = "shellTreeView1";
            this.shellTreeView1.RootedAt = Jam.Shell.ShellFolder.Drives;
            this.shellTreeView1.RootedAtFileSystemFolder = "";
            this.shellTreeView1.SelectedPath = "";
            this.shellTreeView1.ShowColorCompressed = System.Drawing.Color.Empty;
            this.shellTreeView1.ShowColorEncrypted = System.Drawing.Color.Empty;
            this.shellTreeView1.ShowNetHood = false;
            this.shellTreeView1.ShowRecycleBin = false;
            this.shellTreeView1.Size = new System.Drawing.Size(240, 659);
            this.shellTreeView1.Sorted = true;
            this.shellTreeView1.SpecialFolder = Jam.Shell.ShellFolder.Drives;
            this.shellTreeView1.TabIndex = 0;
            this.shellTreeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.shellTreeView1_AfterSelect);
            this.shellTreeView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.shellTreeView1_MouseClick);
            // 
            // listBoxDP
            // 
            this.listBoxDP.FormattingEnabled = true;
            this.listBoxDP.Location = new System.Drawing.Point(257, 425);
            this.listBoxDP.Name = "listBoxDP";
            this.listBoxDP.Size = new System.Drawing.Size(232, 108);
            this.listBoxDP.TabIndex = 1;
            // 
            // label_DP_liste
            // 
            this.label_DP_liste.AutoSize = true;
            this.label_DP_liste.Location = new System.Drawing.Point(254, 409);
            this.label_DP_liste.Name = "label_DP_liste";
            this.label_DP_liste.Size = new System.Drawing.Size(78, 13);
            this.label_DP_liste.TabIndex = 2;
            this.label_DP_liste.Text = "Datapakker (0)";
            // 
            // labelAV
            // 
            this.labelAV.AutoSize = true;
            this.labelAV.Location = new System.Drawing.Point(11, 9);
            this.labelAV.Name = "labelAV";
            this.labelAV.Size = new System.Drawing.Size(96, 13);
            this.labelAV.TabIndex = 3;
            this.labelAV.Text = "<Vælg AV mappe>";
            // 
            // labelTabeller
            // 
            this.labelTabeller.AutoSize = true;
            this.labelTabeller.Location = new System.Drawing.Point(753, 9);
            this.labelTabeller.Name = "labelTabeller";
            this.labelTabeller.Size = new System.Drawing.Size(60, 13);
            this.labelTabeller.TabIndex = 5;
            this.labelTabeller.Text = "Tabeller (0)";
            // 
            // listBoxTable
            // 
            this.listBoxTable.FormattingEnabled = true;
            this.listBoxTable.Location = new System.Drawing.Point(756, 25);
            this.listBoxTable.Name = "listBoxTable";
            this.listBoxTable.Size = new System.Drawing.Size(432, 199);
            this.listBoxTable.TabIndex = 4;
            this.listBoxTable.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listBoxTable_MouseClick);
            // 
            // labelGeninfo
            // 
            this.labelGeninfo.AutoSize = true;
            this.labelGeninfo.Location = new System.Drawing.Point(254, 243);
            this.labelGeninfo.Name = "labelGeninfo";
            this.labelGeninfo.Size = new System.Drawing.Size(118, 13);
            this.labelGeninfo.TabIndex = 7;
            this.labelGeninfo.Text = "Geninfo dokumenter (0)";
            // 
            // listBoxGeninfo
            // 
            this.listBoxGeninfo.FormattingEnabled = true;
            this.listBoxGeninfo.Location = new System.Drawing.Point(256, 261);
            this.listBoxGeninfo.Name = "listBoxGeninfo";
            this.listBoxGeninfo.Size = new System.Drawing.Size(493, 134);
            this.listBoxGeninfo.TabIndex = 6;
            this.listBoxGeninfo.Click += new System.EventHandler(this.listBoxGeninfo_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(254, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Arkver";
            // 
            // labelSkaber
            // 
            this.labelSkaber.AutoSize = true;
            this.labelSkaber.Location = new System.Drawing.Point(254, 145);
            this.labelSkaber.Name = "labelSkaber";
            this.labelSkaber.Size = new System.Drawing.Size(56, 13);
            this.labelSkaber.TabIndex = 11;
            this.labelSkaber.Text = "Skaber (0)";
            // 
            // richTextBoxMetadata
            // 
            this.richTextBoxMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBoxMetadata.Location = new System.Drawing.Point(756, 261);
            this.richTextBoxMetadata.Name = "richTextBoxMetadata";
            this.richTextBoxMetadata.Size = new System.Drawing.Size(432, 423);
            this.richTextBoxMetadata.TabIndex = 12;
            this.richTextBoxMetadata.Text = "";
            this.richTextBoxMetadata.WordWrap = false;
            // 
            // labelMetadata
            // 
            this.labelMetadata.AutoSize = true;
            this.labelMetadata.Location = new System.Drawing.Point(753, 245);
            this.labelMetadata.Name = "labelMetadata";
            this.labelMetadata.Size = new System.Drawing.Size(52, 13);
            this.labelMetadata.TabIndex = 13;
            this.labelMetadata.Text = "Metadata";
            // 
            // richTextBoxArkver
            // 
            this.richTextBoxArkver.Location = new System.Drawing.Point(257, 25);
            this.richTextBoxArkver.Name = "richTextBoxArkver";
            this.richTextBoxArkver.Size = new System.Drawing.Size(492, 109);
            this.richTextBoxArkver.TabIndex = 14;
            this.richTextBoxArkver.Text = "";
            this.richTextBoxArkver.WordWrap = false;
            // 
            // listBoxSkaber
            // 
            this.listBoxSkaber.FormattingEnabled = true;
            this.listBoxSkaber.Location = new System.Drawing.Point(256, 161);
            this.listBoxSkaber.Name = "listBoxSkaber";
            this.listBoxSkaber.Size = new System.Drawing.Size(493, 69);
            this.listBoxSkaber.TabIndex = 15;
            // 
            // labelDokumenter
            // 
            this.labelDokumenter.AutoSize = true;
            this.labelDokumenter.Location = new System.Drawing.Point(255, 547);
            this.labelDokumenter.Name = "labelDokumenter";
            this.labelDokumenter.Size = new System.Drawing.Size(80, 13);
            this.labelDokumenter.TabIndex = 17;
            this.labelDokumenter.Text = "Dokumenter (0)";
            // 
            // listBoxDokumenter
            // 
            this.listBoxDokumenter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxDokumenter.FormattingEnabled = true;
            this.listBoxDokumenter.Location = new System.Drawing.Point(258, 563);
            this.listBoxDokumenter.Name = "listBoxDokumenter";
            this.listBoxDokumenter.Size = new System.Drawing.Size(491, 121);
            this.listBoxDokumenter.TabIndex = 16;
            this.listBoxDokumenter.Click += new System.EventHandler(this.listBoxDokumenter_Click);
            // 
            // buttonParse
            // 
            this.buttonParse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonParse.Location = new System.Drawing.Point(1265, 632);
            this.buttonParse.Name = "buttonParse";
            this.buttonParse.Size = new System.Drawing.Size(62, 23);
            this.buttonParse.TabIndex = 18;
            this.buttonParse.Text = "Parse alle";
            this.buttonParse.UseVisualStyleBackColor = true;
            this.buttonParse.Click += new System.EventHandler(this.buttonParseAlle_Click);
            // 
            // buttonContext
            // 
            this.buttonContext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonContext.Location = new System.Drawing.Point(1265, 661);
            this.buttonContext.Name = "buttonContext";
            this.buttonContext.Size = new System.Drawing.Size(72, 23);
            this.buttonContext.TabIndex = 19;
            this.buttonContext.Text = "ContextDoc";
            this.buttonContext.UseVisualStyleBackColor = true;
            this.buttonContext.Click += new System.EventHandler(this.buttonContext_Click);
            // 
            // buttonTabeller
            // 
            this.buttonTabeller.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTabeller.Location = new System.Drawing.Point(1341, 661);
            this.buttonTabeller.Name = "buttonTabeller";
            this.buttonTabeller.Size = new System.Drawing.Size(54, 23);
            this.buttonTabeller.TabIndex = 20;
            this.buttonTabeller.Text = "Tabeller";
            this.buttonTabeller.UseVisualStyleBackColor = true;
            this.buttonTabeller.Click += new System.EventHandler(this.buttonTabeller_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(1399, 661);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(74, 23);
            this.button4.TabIndex = 21;
            this.button4.Text = "Dokumenter";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // buttonFolders
            // 
            this.buttonFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFolders.Location = new System.Drawing.Point(1205, 661);
            this.buttonFolders.Name = "buttonFolders";
            this.buttonFolders.Size = new System.Drawing.Size(56, 23);
            this.buttonFolders.TabIndex = 22;
            this.buttonFolders.Text = "Folders";
            this.buttonFolders.UseVisualStyleBackColor = true;
            this.buttonFolders.Click += new System.EventHandler(this.buttonFolders_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 693);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1604, 22);
            this.statusStrip1.TabIndex = 24;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(750, 16);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(837, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // labelLog
            // 
            this.labelLog.AutoSize = true;
            this.labelLog.Location = new System.Drawing.Point(1201, 9);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(29, 13);
            this.labelLog.TabIndex = 26;
            this.labelLog.Text = "LOG";
            // 
            // richTextBoxLOG
            // 
            this.richTextBoxLOG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxLOG.Location = new System.Drawing.Point(1204, 25);
            this.richTextBoxLOG.Name = "richTextBoxLOG";
            this.richTextBoxLOG.Size = new System.Drawing.Size(388, 589);
            this.richTextBoxLOG.TabIndex = 25;
            this.richTextBoxLOG.Text = "";
            this.richTextBoxLOG.WordWrap = false;
            // 
            // labelFilmap
            // 
            this.labelFilmap.AutoSize = true;
            this.labelFilmap.Location = new System.Drawing.Point(512, 409);
            this.labelFilmap.Name = "labelFilmap";
            this.labelFilmap.Size = new System.Drawing.Size(94, 13);
            this.labelFilmap.TabIndex = 28;
            this.labelFilmap.Text = "Filer og mapper (0)";
            // 
            // listBoxFilmap
            // 
            this.listBoxFilmap.FormattingEnabled = true;
            this.listBoxFilmap.Location = new System.Drawing.Point(515, 425);
            this.listBoxFilmap.Name = "listBoxFilmap";
            this.listBoxFilmap.Size = new System.Drawing.Size(232, 108);
            this.listBoxFilmap.TabIndex = 27;
            // 
            // comboBoxDest
            // 
            this.comboBoxDest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxDest.FormattingEnabled = true;
            this.comboBoxDest.Items.AddRange(new object[] {
            "C:\\",
            "D:\\",
            "E:\\",
            "F:\\",
            "G:\\",
            "H:\\",
            "I:\\",
            "J:\\",
            "K:\\",
            "L:\\",
            "M:\\",
            "N:\\",
            "O:\\",
            "P:\\",
            "R:\\",
            "S:\\",
            "T:\\",
            "U:\\",
            "V:\\",
            "W:\\",
            "X:\\",
            "Y:\\",
            "Z:\\",
            "Q:\\"});
            this.comboBoxDest.Location = new System.Drawing.Point(1204, 634);
            this.comboBoxDest.Name = "comboBoxDest";
            this.comboBoxDest.Size = new System.Drawing.Size(43, 21);
            this.comboBoxDest.TabIndex = 29;
            this.comboBoxDest.SelectedIndexChanged += new System.EventHandler(this.comboBoxDest_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1201, 617);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Destination";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(1477, 661);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(57, 23);
            this.button1.TabIndex = 31;
            this.button1.Text = "FileIndex";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonSkab1007
            // 
            this.buttonSkab1007.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSkab1007.Location = new System.Drawing.Point(1540, 661);
            this.buttonSkab1007.Name = "buttonSkab1007";
            this.buttonSkab1007.Size = new System.Drawing.Size(68, 23);
            this.buttonSkab1007.TabIndex = 32;
            this.buttonSkab1007.Text = "Skab 1007";
            this.buttonSkab1007.UseVisualStyleBackColor = true;
            this.buttonSkab1007.Click += new System.EventHandler(this.buttonSkab1007_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1390, 620);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(95, 23);
            this.button3.TabIndex = 33;
            this.button3.Text = "Vis LOG fejlliste";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // buttonMetadataTekstSøg
            // 
            this.buttonMetadataTekstSøg.Location = new System.Drawing.Point(1145, 237);
            this.buttonMetadataTekstSøg.Name = "buttonMetadataTekstSøg";
            this.buttonMetadataTekstSøg.Size = new System.Drawing.Size(43, 22);
            this.buttonMetadataTekstSøg.TabIndex = 35;
            this.buttonMetadataTekstSøg.Text = "Søg";
            this.buttonMetadataTekstSøg.UseVisualStyleBackColor = true;
            this.buttonMetadataTekstSøg.Click += new System.EventHandler(this.buttonMetadataTekstSøg_Click);
            // 
            // textBoxMetadataSøg
            // 
            this.textBoxMetadataSøg.Location = new System.Drawing.Point(829, 237);
            this.textBoxMetadataSøg.Name = "textBoxMetadataSøg";
            this.textBoxMetadataSøg.Size = new System.Drawing.Size(310, 20);
            this.textBoxMetadataSøg.TabIndex = 36;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1604, 715);
            this.Controls.Add(this.textBoxMetadataSøg);
            this.Controls.Add(this.buttonMetadataTekstSøg);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.buttonSkab1007);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxDest);
            this.Controls.Add(this.labelFilmap);
            this.Controls.Add(this.listBoxFilmap);
            this.Controls.Add(this.labelLog);
            this.Controls.Add(this.richTextBoxLOG);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.buttonFolders);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.buttonTabeller);
            this.Controls.Add(this.buttonContext);
            this.Controls.Add(this.buttonParse);
            this.Controls.Add(this.labelDokumenter);
            this.Controls.Add(this.listBoxDokumenter);
            this.Controls.Add(this.listBoxSkaber);
            this.Controls.Add(this.richTextBoxArkver);
            this.Controls.Add(this.labelMetadata);
            this.Controls.Add(this.richTextBoxMetadata);
            this.Controls.Add(this.labelSkaber);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelGeninfo);
            this.Controls.Add(this.listBoxGeninfo);
            this.Controls.Add(this.labelTabeller);
            this.Controls.Add(this.listBoxTable);
            this.Controls.Add(this.labelAV);
            this.Controls.Add(this.label_DP_liste);
            this.Controls.Add(this.listBoxDP);
            this.Controls.Add(this.shellTreeView1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "342 til 1007 Vis, test og konverter";
            ((System.ComponentModel.ISupportInitialize)(this.shellTreeView1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Jam.Shell.ShellTreeView shellTreeView1;
        private System.Windows.Forms.ListBox listBoxDP;
        private System.Windows.Forms.Label label_DP_liste;
        private System.Windows.Forms.Label labelAV;
        private System.Windows.Forms.Label labelTabeller;
        private System.Windows.Forms.ListBox listBoxTable;
        private System.Windows.Forms.Label labelGeninfo;
        private System.Windows.Forms.ListBox listBoxGeninfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelSkaber;
        private System.Windows.Forms.RichTextBox richTextBoxMetadata;
        private System.Windows.Forms.Label labelMetadata;
        private System.Windows.Forms.RichTextBox richTextBoxArkver;
        private System.Windows.Forms.ListBox listBoxSkaber;
        private System.Windows.Forms.Label labelDokumenter;
        private System.Windows.Forms.ListBox listBoxDokumenter;
        private System.Windows.Forms.Button buttonParse;
        private System.Windows.Forms.Button buttonContext;
        private System.Windows.Forms.Button buttonTabeller;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button buttonFolders;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label labelLog;
        private System.Windows.Forms.RichTextBox richTextBoxLOG;
        private System.Windows.Forms.Label labelFilmap;
        private System.Windows.Forms.ListBox listBoxFilmap;
        private System.Windows.Forms.ComboBox comboBoxDest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonSkab1007;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button buttonMetadataTekstSøg;
        private System.Windows.Forms.TextBox textBoxMetadataSøg;
    }
}

