namespace YummySoupExporter
{
    partial class YSEForm
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
            this.inputPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.inputBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.outputPath = new System.Windows.Forms.TextBox();
            this.exportBrowse = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.exportProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // inputPath
            // 
            this.inputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputPath.Location = new System.Drawing.Point(12, 28);
            this.inputPath.Name = "inputPath";
            this.inputPath.Size = new System.Drawing.Size(389, 20);
            this.inputPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Location of \"Library Database.SQL\":";
            // 
            // inputBrowse
            // 
            this.inputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.inputBrowse.Location = new System.Drawing.Point(405, 27);
            this.inputBrowse.Name = "inputBrowse";
            this.inputBrowse.Size = new System.Drawing.Size(75, 23);
            this.inputBrowse.TabIndex = 2;
            this.inputBrowse.Text = "Browse";
            this.inputBrowse.UseVisualStyleBackColor = true;
            this.inputBrowse.Click += new System.EventHandler(this.inputBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Folder to save .mcx files in:";
            // 
            // outputPath
            // 
            this.outputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPath.Location = new System.Drawing.Point(12, 89);
            this.outputPath.Name = "outputPath";
            this.outputPath.Size = new System.Drawing.Size(389, 20);
            this.outputPath.TabIndex = 4;
            // 
            // exportBrowse
            // 
            this.exportBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exportBrowse.Location = new System.Drawing.Point(405, 88);
            this.exportBrowse.Name = "exportBrowse";
            this.exportBrowse.Size = new System.Drawing.Size(75, 23);
            this.exportBrowse.TabIndex = 5;
            this.exportBrowse.Text = "Browse";
            this.exportBrowse.UseVisualStyleBackColor = true;
            this.exportBrowse.Click += new System.EventHandler(this.exportBrowse_Click);
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.Location = new System.Drawing.Point(497, 140);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 6;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // exportProgressBar
            // 
            this.exportProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exportProgressBar.Location = new System.Drawing.Point(12, 140);
            this.exportProgressBar.Name = "exportProgressBar";
            this.exportProgressBar.Size = new System.Drawing.Size(465, 23);
            this.exportProgressBar.TabIndex = 7;
            // 
            // YSEForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 177);
            this.Controls.Add(this.exportProgressBar);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.exportBrowse);
            this.Controls.Add(this.outputPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.inputBrowse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.inputPath);
            this.MaximumSize = new System.Drawing.Size(1000, 215);
            this.Name = "YSEForm";
            this.Text = "YummySoup Exporter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inputPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button inputBrowse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox outputPath;
        private System.Windows.Forms.Button exportBrowse;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.ProgressBar exportProgressBar;
    }
}

