namespace PangYa_Suite_Tools
{
    partial class FrmPakDiff
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.grpDirectories = new System.Windows.Forms.GroupBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.txtSourceClient = new System.Windows.Forms.TextBox();
            this.btnBrowseSource = new System.Windows.Forms.Button();
            this.lblCompare = new System.Windows.Forms.Label();
            this.txtCompareClient = new System.Windows.Forms.TextBox();
            this.btnBrowseCompare = new System.Windows.Forms.Button();
            this.grpMode = new System.Windows.Forms.GroupBox();
            this.rbDifferences = new System.Windows.Forms.RadioButton();
            this.rbIdentical = new System.Windows.Forms.RadioButton();
            this.btnCompare = new System.Windows.Forms.Button();
            this.lstDiffFiles = new System.Windows.Forms.ListView();
            this.colFile = new System.Windows.Forms.ColumnHeader();
            this.colPak = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.btnExtractSelected = new System.Windows.Forms.Button();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.prgBar = new System.Windows.Forms.ProgressBar();
            this.grpDirectories.SuspendLayout();
            this.grpMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDirectories
            // 
            this.grpDirectories.Controls.Add(this.btnBrowseCompare);
            this.grpDirectories.Controls.Add(this.txtCompareClient);
            this.grpDirectories.Controls.Add(this.lblCompare);
            this.grpDirectories.Controls.Add(this.btnBrowseSource);
            this.grpDirectories.Controls.Add(this.txtSourceClient);
            this.grpDirectories.Controls.Add(this.lblSource);
            this.grpDirectories.Location = new System.Drawing.Point(12, 12);
            this.grpDirectories.Name = "grpDirectories";
            this.grpDirectories.Size = new System.Drawing.Size(760, 105);
            this.grpDirectories.TabIndex = 0;
            this.grpDirectories.TabStop = false;
            this.grpDirectories.Text = " Client Directories / Diretórios dos Clientes ";
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(15, 32);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(157, 15);
            this.lblSource.TabIndex = 0;
            this.lblSource.Text = "Target/Source (Extract From):";
            // 
            // txtSourceClient
            // 
            this.txtSourceClient.Location = new System.Drawing.Point(190, 29);
            this.txtSourceClient.Name = "txtSourceClient";
            this.txtSourceClient.Size = new System.Drawing.Size(465, 23);
            this.txtSourceClient.TabIndex = 1;
            // 
            // btnBrowseSource
            // 
            this.btnBrowseSource.Location = new System.Drawing.Point(661, 28);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(84, 25);
            this.btnBrowseSource.TabIndex = 2;
            this.btnBrowseSource.Text = "...";
            this.btnBrowseSource.UseVisualStyleBackColor = true;
            this.btnBrowseSource.Click += new System.EventHandler(this.BtnBrowseSource_Click);
            // 
            // lblCompare
            // 
            this.lblCompare.AutoSize = true;
            this.lblCompare.Location = new System.Drawing.Point(15, 68);
            this.lblCompare.Name = "lblCompare";
            this.lblCompare.Size = new System.Drawing.Size(155, 15);
            this.lblCompare.TabIndex = 3;
            this.lblCompare.Text = "Your Client (To Compare):";
            // 
            // txtCompareClient
            // 
            this.txtCompareClient.Location = new System.Drawing.Point(190, 65);
            this.txtCompareClient.Name = "txtCompareClient";
            this.txtCompareClient.Size = new System.Drawing.Size(465, 23);
            this.txtCompareClient.TabIndex = 4;
            // 
            // btnBrowseCompare
            // 
            this.btnBrowseCompare.Location = new System.Drawing.Point(661, 64);
            this.btnBrowseCompare.Name = "btnBrowseCompare";
            this.btnBrowseCompare.Size = new System.Drawing.Size(84, 25);
            this.btnBrowseCompare.TabIndex = 5;
            this.btnBrowseCompare.Text = "...";
            this.btnBrowseCompare.UseVisualStyleBackColor = true;
            this.btnBrowseCompare.Click += new System.EventHandler(this.BtnBrowseCompare_Click);
            // 
            // grpMode
            // 
            this.grpMode.Controls.Add(this.btnCompare);
            this.grpMode.Controls.Add(this.rbIdentical);
            this.grpMode.Controls.Add(this.rbDifferences);
            this.grpMode.Location = new System.Drawing.Point(12, 128);
            this.grpMode.Name = "grpMode";
            this.grpMode.Size = new System.Drawing.Size(200, 160);
            this.grpMode.TabIndex = 1;
            this.grpMode.TabStop = false;
            this.grpMode.Text = " Comparison Mode ";
            // 
            // rbDifferences
            // 
            this.rbDifferences.AutoSize = true;
            this.rbDifferences.Checked = true;
            this.rbDifferences.Location = new System.Drawing.Point(18, 35);
            this.rbDifferences.Name = "rbDifferences";
            this.rbDifferences.Size = new System.Drawing.Size(150, 19);
            this.rbDifferences.TabIndex = 0;
            this.rbDifferences.TabStop = true;
            this.rbDifferences.Text = "Differences / New Files";
            this.rbDifferences.UseVisualStyleBackColor = true;
            // 
            // rbIdentical
            // 
            this.rbIdentical.AutoSize = true;
            this.rbIdentical.Location = new System.Drawing.Point(18, 65);
            this.rbIdentical.Name = "rbIdentical";
            this.rbIdentical.Size = new System.Drawing.Size(142, 19);
            this.rbIdentical.TabIndex = 1;
            this.rbIdentical.Text = "Identical Files (Equals)";
            this.rbIdentical.UseVisualStyleBackColor = true;
            // 
            // btnCompare
            // 
            this.btnCompare.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCompare.Location = new System.Drawing.Point(18, 105);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(164, 38);
            this.btnCompare.TabIndex = 2;
            this.btnCompare.Text = "🔍 Compare / Comparar";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.BtnCompare_Click);
            // 
            // lstDiffFiles
            // 
            this.lstDiffFiles.CheckBoxes = true;
            this.lstDiffFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFile,
            this.colPak,
            this.colStatus});
            this.lstDiffFiles.FullRowSelect = true;
            this.lstDiffFiles.GridLines = true;
            this.lstDiffFiles.Location = new System.Drawing.Point(225, 135);
            this.lstDiffFiles.Name = "lstDiffFiles";
            this.lstDiffFiles.Size = new System.Drawing.Size(547, 340);
            this.lstDiffFiles.TabIndex = 2;
            this.lstDiffFiles.UseCompatibleStateImageBehavior = false;
            this.lstDiffFiles.View = System.Windows.Forms.View.Details;
            // 
            // colFile
            // 
            this.colFile.Text = "File Path / Caminho do Arquivo";
            this.colFile.Width = 320;
            // 
            // colPak
            // 
            this.colPak.Text = "Source PAK";
            this.colPak.Width = 110;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 90;
            // 
            // btnExtractSelected
            // 
            this.btnExtractSelected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExtractSelected.Location = new System.Drawing.Point(12, 432);
            this.btnExtractSelected.Name = "btnExtractSelected";
            this.btnExtractSelected.Size = new System.Drawing.Size(200, 43);
            this.btnExtractSelected.TabIndex = 3;
            this.btnExtractSelected.Text = "📦 Extract Selected\r\nExtrair Selecionados";
            this.btnExtractSelected.UseVisualStyleBackColor = true;
            this.btnExtractSelected.Click += new System.EventHandler(this.BtnExtractSelected_Click);
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Location = new System.Drawing.Point(225, 485);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(147, 19);
            this.chkSelectAll.TabIndex = 4;
            this.chkSelectAll.Text = "Select All / Selecionar Tudo";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.ChkSelectAll_CheckedChanged);
            // 
            // prgBar
            // 
            this.prgBar.Location = new System.Drawing.Point(400, 486);
            this.prgBar.Name = "prgBar";
            this.prgBar.Size = new System.Drawing.Size(372, 16);
            this.prgBar.TabIndex = 5;
            // 
            // FrmPakDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 516);
            this.Controls.Add(this.prgBar);
            this.Controls.Add(this.chkSelectAll);
            this.Controls.Add(this.btnExtractSelected);
            this.Controls.Add(this.lstDiffFiles);
            this.Controls.Add(this.grpMode);
            this.Controls.Add(this.grpDirectories);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmPakDiff";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PAK Multi-Compare & Sync Tool";
            this.grpDirectories.ResumeLayout(false);
            this.grpDirectories.PerformLayout();
            this.grpMode.ResumeLayout(false);
            this.grpMode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDirectories;
        private System.Windows.Forms.Button btnBrowseCompare;
        private System.Windows.Forms.TextBox txtCompareClient;
        private System.Windows.Forms.Label lblCompare;
        private System.Windows.Forms.Button btnBrowseSource;
        private System.Windows.Forms.TextBox txtSourceClient;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.GroupBox grpMode;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.RadioButton rbIdentical;
        private System.Windows.Forms.RadioButton rbDifferences;
        private System.Windows.Forms.ListView lstDiffFiles;
        private System.Windows.Forms.ColumnHeader colFile;
        private System.Windows.Forms.ColumnHeader colPak;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Button btnExtractSelected;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.ProgressBar prgBar;
    }
}