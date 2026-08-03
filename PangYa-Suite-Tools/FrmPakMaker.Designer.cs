#nullable disable

namespace PangYa_Suite_Tools
{
    partial class FrmPakMaker
    {
        private System.ComponentModel.IContainer components = null;

        // Controles da Interface
        private System.Windows.Forms.Panel readerPanel;

        // Componentes da Aba 1 (Extração e Modificações)
        private System.Windows.Forms.TextBox txtPakPath;
        private System.Windows.Forms.Button btnBrowsePak;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblCurrentPath;
        private System.Windows.Forms.TreeView tvFolders;
        private System.Windows.Forms.ListView lstEntries;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colCompSize;
        private System.Windows.Forms.Button btnExtractSelected;
        private System.Windows.Forms.Button btnRemoveSelected;
        private System.Windows.Forms.Button btnExtractAll;
        private System.Windows.Forms.Button btnUpdatePak;
        private System.Windows.Forms.Button btnBatchExtract;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblEntries;
        private System.Windows.Forms.Label lblCompressionLevelSummary;
        private System.Windows.Forms.Label lblPakKeySummary;
        private System.Windows.Forms.GroupBox groupHeader;

        // Barra de Status Global e Progresso
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblPakKey;
        private System.Windows.Forms.ToolStripProgressBar progressBar1;
        private System.Windows.Forms.ToolStripButton btnCancelOperation;
        private System.Windows.Forms.ToolStripStatusLabel lblFilenameEncoding;
        private System.Windows.Forms.ToolStripComboBox cboFilenameEncoding;

        // Troca de chave XTEA
        private System.Windows.Forms.Label lblNewKey;
        private System.Windows.Forms.ComboBox cboNewRegion;
        private System.Windows.Forms.Button btnChangeKey;
        private ToolStripMenuItem _menuExtractSingle;
        private ToolStripMenuItem _menuRenameSingle;
        private ToolStripMenuItem _menuRemoveSingle;
        private ToolStripMenuItem _menuExtractFolder;
        private ToolStripMenuItem _menuRemoveFolder;
        private ToolStripMenuItem _menuRename;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            readerPanel = new Panel();
            groupHeader = new GroupBox();
            lblAuthor = new Label();
            lblVersion = new Label();
            lblEntries = new Label();
            lblCompressionLevelSummary = new Label();
            lblPakKeySummary = new Label();
            lblSearch = new Label();
            txtSearch = new TextBox();
            lblCurrentPath = new Label();
            tvFolders = new TreeView();
            btnExtractSelected = new Button();
            btnRemoveSelected = new Button();
            btnBatchExtract = new Button();
            btnUpdatePak = new Button();
            btnExtractAll = new Button();
            lstEntries = new ListView();
            colName = new ColumnHeader();
            colType = new ColumnHeader();
            colSize = new ColumnHeader();
            colCompSize = new ColumnHeader();
            btnBrowsePak = new Button();
            txtPakPath = new TextBox();
            lblNewKey = new Label();
            cboNewRegion = new ComboBox();
            btnChangeKey = new Button();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblPakKey = new ToolStripStatusLabel();
            progressBar1 = new ToolStripProgressBar();
            btnCancelOperation = new ToolStripButton();
            lblFilenameEncoding = new ToolStripStatusLabel();
            cboFilenameEncoding = new ToolStripComboBox();
            txtUpdateAuthor = new TextBox();
            toolTip1 = new ToolTip(components);
            readerPanel.SuspendLayout();
            groupHeader.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            //
            // readerPanel
            //
            readerPanel.Controls.Add(txtUpdateAuthor);
            readerPanel.Controls.Add(groupHeader);
            readerPanel.Controls.Add(lblSearch);
            readerPanel.Controls.Add(txtSearch);
            readerPanel.Controls.Add(lblCurrentPath);
            readerPanel.Controls.Add(tvFolders);
            readerPanel.Controls.Add(btnExtractSelected);
            readerPanel.Controls.Add(btnRemoveSelected);
            readerPanel.Controls.Add(btnBatchExtract);
            readerPanel.Controls.Add(btnUpdatePak);
            readerPanel.Controls.Add(btnExtractAll);
            readerPanel.Controls.Add(lstEntries);
            readerPanel.Controls.Add(btnBrowsePak);
            readerPanel.Controls.Add(txtPakPath);
            readerPanel.Controls.Add(lblNewKey);
            readerPanel.Controls.Add(cboNewRegion);
            readerPanel.Controls.Add(btnChangeKey);
            readerPanel.Dock = DockStyle.Fill;
            readerPanel.Location = new Point(9, 8);
            readerPanel.Margin = new Padding(3, 2, 3, 2);
            readerPanel.Name = "readerPanel";
            readerPanel.Padding = new Padding(9, 8, 9, 8);
            readerPanel.Size = new Size(756, 430);
            readerPanel.TabIndex = 0;
            // 
            // groupHeader
            // 
            groupHeader.Controls.Add(lblAuthor);
            groupHeader.Controls.Add(lblVersion);
            groupHeader.Controls.Add(lblEntries);
            groupHeader.Controls.Add(lblCompressionLevelSummary);
            groupHeader.Controls.Add(lblPakKeySummary);
            groupHeader.Location = new Point(11, 40);
            groupHeader.Margin = new Padding(3, 2, 3, 2);
            groupHeader.Name = "groupHeader";
            groupHeader.Padding = new Padding(3, 2, 3, 2);
            groupHeader.Size = new Size(724, 43);
            groupHeader.TabIndex = 2;
            groupHeader.TabStop = false;
            // 
            // lblAuthor
            // 
            lblAuthor.Location = new Point(13, 19);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(160, 15);
            lblAuthor.TabIndex = 0;
            // 
            // lblVersion
            // 
            lblVersion.Location = new Point(178, 19);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(105, 15);
            lblVersion.TabIndex = 1;
            // 
            // lblEntries
            // 
            lblEntries.Location = new Point(288, 19);
            lblEntries.Name = "lblEntries";
            lblEntries.Size = new Size(95, 15);
            lblEntries.TabIndex = 2;
            //
            // lblCompressionLevelSummary
            //
            lblCompressionLevelSummary.AutoEllipsis = true;
            lblCompressionLevelSummary.Location = new Point(388, 19);
            lblCompressionLevelSummary.Name = "lblCompressionLevelSummary";
            lblCompressionLevelSummary.Size = new Size(150, 15);
            lblCompressionLevelSummary.TabIndex = 3;
            //
            // lblPakKeySummary
            //
            lblPakKeySummary.AutoEllipsis = true;
            lblPakKeySummary.Location = new Point(543, 19);
            lblPakKeySummary.Name = "lblPakKeySummary";
            lblPakKeySummary.Size = new Size(167, 15);
            lblPakKeySummary.TabIndex = 4;
            // 
            // lblSearch
            // 
            lblSearch.Location = new Point(11, 91);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(95, 17);
            lblSearch.TabIndex = 3;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(108, 88);
            txtSearch.Margin = new Padding(3, 2, 3, 2);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Filtrar por nome do objeto/arquivo...";
            txtSearch.Size = new Size(225, 23);
            txtSearch.TabIndex = 4;
            // 
            // lblCurrentPath
            // 
            lblCurrentPath.Location = new Point(345, 91);
            lblCurrentPath.Name = "lblCurrentPath";
            lblCurrentPath.Size = new Size(390, 17);
            lblCurrentPath.TabIndex = 5;
            // 
            // tvFolders
            // 
            tvFolders.Location = new Point(11, 114);
            tvFolders.Margin = new Padding(3, 2, 3, 2);
            tvFolders.Name = "tvFolders";
            tvFolders.Size = new Size(200, 180);
            tvFolders.TabIndex = 6;
            // 
            // btnExtractSelected
            // 
            btnExtractSelected.BackColor = Color.FromArgb(23, 162, 184);
            btnExtractSelected.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExtractSelected.ForeColor = Color.White;
            btnExtractSelected.Location = new Point(11, 300);
            btnExtractSelected.Margin = new Padding(3, 2, 3, 2);
            btnExtractSelected.Name = "btnExtractSelected";
            btnExtractSelected.Size = new Size(175, 26);
            btnExtractSelected.TabIndex = 7;
            btnExtractSelected.UseVisualStyleBackColor = false;
            btnExtractSelected.Click += btnExtractSelected_Click;
            // 
            // btnRemoveSelected
            // 
            btnRemoveSelected.BackColor = Color.FromArgb(220, 53, 69);
            btnRemoveSelected.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRemoveSelected.ForeColor = Color.White;
            btnRemoveSelected.Location = new Point(192, 300);
            btnRemoveSelected.Margin = new Padding(3, 2, 3, 2);
            btnRemoveSelected.Name = "btnRemoveSelected";
            btnRemoveSelected.Size = new Size(175, 26);
            btnRemoveSelected.TabIndex = 8;
            btnRemoveSelected.UseVisualStyleBackColor = false;
            btnRemoveSelected.Click += btnRemoveSelected_Click;
            // 
            // btnBatchExtract
            // 
            btnBatchExtract.BackColor = Color.FromArgb(108, 117, 125);
            btnBatchExtract.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBatchExtract.ForeColor = Color.White;
            btnBatchExtract.Location = new Point(11, 332);
            btnBatchExtract.Margin = new Padding(3, 2, 3, 2);
            btnBatchExtract.Name = "btnBatchExtract";
            btnBatchExtract.Size = new Size(175, 28);
            btnBatchExtract.TabIndex = 9;
            btnBatchExtract.UseVisualStyleBackColor = false;
            btnBatchExtract.Click += btnBatchExtract_Click;
            // 
            // btnUpdatePak
            // 
            btnUpdatePak.BackColor = Color.FromArgb(255, 193, 7);
            btnUpdatePak.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnUpdatePak.ForeColor = Color.Black;
            btnUpdatePak.Location = new Point(410, 332);
            btnUpdatePak.Margin = new Padding(3, 2, 3, 2);
            btnUpdatePak.Name = "btnUpdatePak";
            btnUpdatePak.Size = new Size(160, 28);
            btnUpdatePak.TabIndex = 10;
            btnUpdatePak.UseVisualStyleBackColor = false;
            btnUpdatePak.Click += btnUpdatePak_Click;
            // 
            // btnExtractAll
            // 
            btnExtractAll.BackColor = Color.FromArgb(40, 167, 69);
            btnExtractAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExtractAll.ForeColor = Color.White;
            btnExtractAll.Location = new Point(586, 332);
            btnExtractAll.Margin = new Padding(3, 2, 3, 2);
            btnExtractAll.Name = "btnExtractAll";
            btnExtractAll.Size = new Size(149, 28);
            btnExtractAll.TabIndex = 11;
            btnExtractAll.UseVisualStyleBackColor = false;
            btnExtractAll.Click += btnExtractAll_Click;
            // 
            // lstEntries
            // 
            lstEntries.Columns.AddRange(new ColumnHeader[] { colName, colType, colSize, colCompSize });
            lstEntries.FullRowSelect = true;
            lstEntries.GridLines = true;
            lstEntries.Location = new Point(217, 114);
            lstEntries.Margin = new Padding(3, 2, 3, 2);
            lstEntries.Name = "lstEntries";
            lstEntries.Scrollable = true;
            lstEntries.Size = new Size(518, 180);
            lstEntries.TabIndex = 12;
            lstEntries.UseCompatibleStateImageBehavior = false;
            lstEntries.View = View.Details;
            // 
            // colName
            // 
            colName.Width = 220;
            // 
            // colType
            // 
            colType.Width = 70;
            // 
            // colSize
            // 
            colSize.Width = 110;
            // 
            // colCompSize
            // 
            colCompSize.Width = 100;
            // 
            // btnBrowsePak
            // 
            btnBrowsePak.Location = new Point(643, 10);
            btnBrowsePak.Margin = new Padding(3, 2, 3, 2);
            btnBrowsePak.Name = "btnBrowsePak";
            btnBrowsePak.Size = new Size(92, 22);
            btnBrowsePak.TabIndex = 1;
            btnBrowsePak.UseVisualStyleBackColor = true;
            btnBrowsePak.Click += btnBrowsePak_Click;
            // 
            // txtPakPath
            // 
            txtPakPath.Location = new Point(11, 11);
            txtPakPath.Margin = new Padding(3, 2, 3, 2);
            txtPakPath.Name = "txtPakPath";
            txtPakPath.PlaceholderText = "Arraste um arquivo .pak aqui ou clique em Buscar...";
            txtPakPath.ReadOnly = true;
            txtPakPath.Size = new Size(622, 23);
            txtPakPath.TabIndex = 0;
            // 
            // lblNewKey
            // 
            lblNewKey.Location = new Point(5, 368);
            lblNewKey.Name = "lblNewKey";
            lblNewKey.Size = new Size(130, 17);
            lblNewKey.TabIndex = 13;
            // 
            // cboNewRegion
            // 
            cboNewRegion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboNewRegion.Location = new Point(139, 365);
            cboNewRegion.Margin = new Padding(3, 2, 3, 2);
            cboNewRegion.Name = "cboNewRegion";
            cboNewRegion.Size = new Size(280, 23);
            cboNewRegion.TabIndex = 14;
            // 
            // btnChangeKey
            // 
            btnChangeKey.BackColor = Color.FromArgb(111, 66, 193);
            btnChangeKey.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnChangeKey.ForeColor = Color.White;
            btnChangeKey.Location = new Point(420, 364);
            btnChangeKey.Margin = new Padding(3, 2, 3, 2);
            btnChangeKey.Name = "btnChangeKey";
            btnChangeKey.Size = new Size(310, 26);
            btnChangeKey.TabIndex = 15;
            btnChangeKey.UseVisualStyleBackColor = false;
            btnChangeKey.Click += btnChangeKey_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar1, btnCancelOperation, lblPakKey, lblFilenameEncoding, cboFilenameEncoding });
            statusStrip1.Location = new Point(9, 438);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 12, 0);
            statusStrip1.Size = new Size(756, 23);
            statusStrip1.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(43, 18);
            lblStatus.Spring = true;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblPakKey
            //
            lblPakKey.Margin = new Padding(10, 0, 0, 0);
            lblPakKey.Name = "lblPakKey";
            // 
            // progressBar1
            // 
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(150, 17);
            //
            // btnCancelOperation
            //
            btnCancelOperation.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCancelOperation.Enabled = false;
            btnCancelOperation.Name = "btnCancelOperation";
            btnCancelOperation.Click += btnCancelOperation_Click;
            //
            // lblFilenameEncoding
            //
            lblFilenameEncoding.Margin = new Padding(10, 0, 0, 0);
            lblFilenameEncoding.Name = "lblFilenameEncoding";
            //
            // cboFilenameEncoding
            //
            cboFilenameEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFilenameEncoding.DropDownWidth = 360;
            cboFilenameEncoding.Name = "cboFilenameEncoding";
            cboFilenameEncoding.Size = new Size(175, 23);
            cboFilenameEncoding.SelectedIndexChanged += cboFilenameEncoding_SelectedIndexChanged;
            // 
            // txtUpdateAuthor
            // 
            txtUpdateAuthor.Location = new Point(241, 336);
            txtUpdateAuthor.Name = "txtUpdateAuthor";
            txtUpdateAuthor.Size = new Size(163, 23);
            txtUpdateAuthor.TabIndex = 18;
            toolTip1.SetToolTip(txtUpdateAuthor, "Atualize o autor do pak.\r\n");
            // 
            // FrmPakMaker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(774, 487);
            Controls.Add(readerPanel);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.Sizable;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = true;
            MinimumSize = new Size(800, 600);
            Name = "FrmPakMaker";
            Padding = new Padding(9, 8, 9, 26);
            SizeGripStyle = SizeGripStyle.Show;
            Text = "PakManager - Interface";
            readerPanel.ResumeLayout(false);
            readerPanel.PerformLayout();
            groupHeader.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private TextBox txtUpdateAuthor;
        private ToolTip toolTip1;
    }
}
