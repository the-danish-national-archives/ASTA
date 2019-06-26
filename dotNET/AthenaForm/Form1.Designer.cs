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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.sipTextBox = new System.Windows.Forms.TextBox();
            this.sipButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.aipNameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.aipTextBox = new System.Windows.Forms.TextBox();
            this.aipButton = new System.Windows.Forms.Button();
            this.convertButton = new System.Windows.Forms.Button();
            this.outputRichTextBox = new System.Windows.Forms.RichTextBox();
            this.sipPathRequired = new System.Windows.Forms.ErrorProvider(this.components);
            this.aipNameRequired = new System.Windows.Forms.ErrorProvider(this.components);
            this.aipPathRequired = new System.Windows.Forms.ErrorProvider(this.components);
            this.nextForm = new System.Windows.Forms.Button();
            this.logButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.sipPathRequired)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aipNameRequired)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aipPathRequired)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "SIP Mappe:";
            // 
            // sipTextBox
            // 
            this.sipTextBox.Location = new System.Drawing.Point(146, 52);
            this.sipTextBox.Name = "sipTextBox";
            this.sipTextBox.Size = new System.Drawing.Size(738, 26);
            this.sipTextBox.TabIndex = 1;
            // 
            // sipButton
            // 
            this.sipButton.Location = new System.Drawing.Point(941, 49);
            this.sipButton.Name = "sipButton";
            this.sipButton.Size = new System.Drawing.Size(96, 32);
            this.sipButton.TabIndex = 2;
            this.sipButton.Text = "Browse";
            this.sipButton.UseVisualStyleBackColor = true;
            this.sipButton.Click += new System.EventHandler(this.sipButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "AIP Navn:";
            // 
            // aipNameTextBox
            // 
            this.aipNameTextBox.Location = new System.Drawing.Point(146, 122);
            this.aipNameTextBox.Name = "aipNameTextBox";
            this.aipNameTextBox.Size = new System.Drawing.Size(738, 26);
            this.aipNameTextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "AIP Mappe:";
            // 
            // aipTextBox
            // 
            this.aipTextBox.Location = new System.Drawing.Point(146, 198);
            this.aipTextBox.Name = "aipTextBox";
            this.aipTextBox.Size = new System.Drawing.Size(738, 26);
            this.aipTextBox.TabIndex = 6;
            // 
            // aipButton
            // 
            this.aipButton.Location = new System.Drawing.Point(941, 198);
            this.aipButton.Name = "aipButton";
            this.aipButton.Size = new System.Drawing.Size(93, 32);
            this.aipButton.TabIndex = 7;
            this.aipButton.Text = "Browse";
            this.aipButton.UseVisualStyleBackColor = true;
            this.aipButton.Click += new System.EventHandler(this.aipButton_Click);
            // 
            // convertButton
            // 
            this.convertButton.Location = new System.Drawing.Point(830, 272);
            this.convertButton.Name = "convertButton";
            this.convertButton.Size = new System.Drawing.Size(93, 32);
            this.convertButton.TabIndex = 8;
            this.convertButton.Text = "Convert";
            this.convertButton.UseVisualStyleBackColor = true;
            this.convertButton.Click += new System.EventHandler(this.convertButton_Click);
            // 
            // outputRichTextBox
            // 
            this.outputRichTextBox.Location = new System.Drawing.Point(42, 328);
            this.outputRichTextBox.Name = "outputRichTextBox";
            this.outputRichTextBox.Size = new System.Drawing.Size(992, 269);
            this.outputRichTextBox.TabIndex = 9;
            this.outputRichTextBox.Text = "";
            // 
            // sipPathRequired
            // 
            this.sipPathRequired.ContainerControl = this;
            // 
            // aipNameRequired
            // 
            this.aipNameRequired.ContainerControl = this;
            // 
            // aipPathRequired
            // 
            this.aipPathRequired.ContainerControl = this;
            // 
            // nextForm
            // 
            this.nextForm.Enabled = false;
            this.nextForm.Location = new System.Drawing.Point(941, 272);
            this.nextForm.Name = "nextForm";
            this.nextForm.Size = new System.Drawing.Size(93, 32);
            this.nextForm.TabIndex = 10;
            this.nextForm.Text = "Næste";
            this.nextForm.UseVisualStyleBackColor = true;
            this.nextForm.Click += new System.EventHandler(this.nextForm_Click);
            // 
            // logButton
            // 
            this.logButton.Enabled = false;
            this.logButton.Location = new System.Drawing.Point(52, 272);
            this.logButton.Name = "logButton";
            this.logButton.Size = new System.Drawing.Size(75, 32);
            this.logButton.TabIndex = 11;
            this.logButton.Text = "Log";
            this.logButton.UseVisualStyleBackColor = true;
            this.logButton.Click += new System.EventHandler(this.logButton_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 626);
            this.Controls.Add(this.logButton);
            this.Controls.Add(this.nextForm);
            this.Controls.Add(this.outputRichTextBox);
            this.Controls.Add(this.convertButton);
            this.Controls.Add(this.aipButton);
            this.Controls.Add(this.aipTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.aipNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.sipButton);
            this.Controls.Add(this.sipTextBox);
            this.Controls.Add(this.label1);
            this.Name = "Form2";
            this.Text = "ASTA - Athena";
            ((System.ComponentModel.ISupportInitialize)(this.sipPathRequired)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aipNameRequired)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aipPathRequired)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sipTextBox;
        private System.Windows.Forms.Button sipButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox aipNameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox aipTextBox;
        private System.Windows.Forms.Button aipButton;
        private System.Windows.Forms.Button convertButton;
        private System.Windows.Forms.RichTextBox outputRichTextBox;
        private System.Windows.Forms.ErrorProvider sipPathRequired;
        private System.Windows.Forms.ErrorProvider aipNameRequired;
        private System.Windows.Forms.ErrorProvider aipPathRequired;
        private System.Windows.Forms.Button nextForm;
        private System.Windows.Forms.Button logButton;
    }
}