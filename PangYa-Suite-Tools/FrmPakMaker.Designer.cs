namespace PangYa_Suite_Tools
{
    partial class FrmPakMaker
    {
        private System.ComponentModel.IContainer components = null;

        // Controles da Interface
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabExtract;
        private System.Windows.Forms.TabPage tabCreate;

        // Componentes da Aba 1 (Extração e Modificações)
        private System.Windows.Forms.TextBox txtPakPath;
        private System.Windows.Forms.Button btnBrowsePak;
        private System.Windows.Forms.ListView lstEntries;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colCompSize;
        private System.Windows.Forms.Button btnExtractAll;
        private System.Windows.Forms.Button btnUpdatePak;
        private System.Windows.Forms.Button btnBatchExtract;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblEntries;
        private System.Windows.Forms.GroupBox groupHeader;

        // Componentes da Aba 2 (Criação)
        private System.Windows.Forms.TextBox txtSourceFolder;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.ComboBox cboVersion;
        private System.Windows.Forms.ComboBox cboCompressType;
        private System.Windows.Forms.NumericUpDown numCompressLevel;
        private System.Windows.Forms.ComboBox cboRegion;
        private System.Windows.Forms.Button btnCreatePak;
        private System.Windows.Forms.Label lblVol;
        private System.Windows.Forms.Label lblComp;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label lblReg;

        // Barra de Status Global e Progresso
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar1;

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
            tabControl1 = new TabControl();
            tabExtract = new TabPage();
            groupHeader = new GroupBox();
            lblAuthor = new Label();
            lblVersion = new Label();
            lblEntries = new Label();
            btnExtractAll = new Button();
            btnUpdatePak = new Button();
            btnBatchExtract = new Button();
            lstEntries = new ListView();
            colName = new ColumnHeader();
            colType = new ColumnHeader();
            colSize = new ColumnHeader();
            colCompSize = new ColumnHeader();
            btnBrowsePak = new Button();
            txtPakPath = new TextBox();
            tabCreate = new TabPage();
            lblReg = new Label();
            lblLevel = new Label();
            lblComp = new Label();
            lblVol = new Label();
            btnCreatePak = new Button();
            cboRegion = new ComboBox();
            numCompressLevel = new NumericUpDown();
            cboCompressType = new ComboBox();
            cboVersion = new ComboBox();
            btnBrowseFolder = new Button();
            txtSourceFolder = new TextBox();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            progressBar1 = new ToolStripProgressBar();

            tabControl1.SuspendLayout();
            tabExtract.SuspendLayout();
            groupHeader.SuspendLayout();
            tabCreate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCompressLevel).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabExtract);
            tabControl1.Controls.Add(tabCreate);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(9, 8);
            tabControl1.Margin = new Padding(3, 2, 3, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(756, 397);
            tabControl1.TabIndex = 0;
            // 
            // tabExtract
            // 
            tabExtract.Controls.Add(groupHeader);
            tabExtract.Controls.Add(btnBatchExtract);
            tabExtract.Controls.Add(btnUpdatePak);
            tabExtract.Controls.Add(btnExtractAll);
            tabExtract.Controls.Add(lstEntries);
            tabExtract.Controls.Add(btnBrowsePak);
            tabExtract.Controls.Add(txtPakPath);
            tabExtract.Location = new Point(4, 24);
            tabExtract.Margin = new Padding(3, 2, 3, 2);
            tabExtract.Name = "tabExtract";
            tabExtract.Padding = new Padding(9, 8, 9, 8);
            tabExtract.Size = new Size(748, 369);
            tabExtract.TabIndex = 0;
            tabExtract.Text = "Leitor & Modificações";
            tabExtract.UseVisualStyleBackColor = true;
            // 
            // groupHeader
            // 
            groupHeader.Controls.Add(lblAuthor);
            groupHeader.Controls.Add(lblVersion);
            groupHeader.Controls.Add(lblEntries);
            groupHeader.Location = new Point(11, 40);
            groupHeader.Margin = new Padding(3, 2, 3, 2);
            groupHeader.Name = "groupHeader";
            groupHeader.Padding = new Padding(3, 2, 3, 2);
            groupHeader.Size = new Size(724, 43);
            groupHeader.TabIndex = 2;
            groupHeader.TabStop = false;
            groupHeader.Text = "Informações do Cabeçalho";
            // 
            // lblAuthor
            // 
            lblAuthor.Location = new Point(13, 19);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(219, 15);
            lblAuthor.TabIndex = 0;
            lblAuthor.Text = "Autor: ---";
            // 
            // lblVersion
            // 
            lblVersion.Location = new Point(262, 19);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(175, 15);
            lblVersion.TabIndex = 1;
            lblVersion.Text = "Versão: ---";
            // 
            // lblEntries
            // 
            lblEntries.Location = new Point(481, 19);
            lblEntries.Name = "lblEntries";
            lblEntries.Size = new Size(175, 15);
            lblEntries.TabIndex = 2;
            lblEntries.Text = "Entradas: ---";
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
            btnBatchExtract.TabIndex = 4;
            btnBatchExtract.Text = "📂 EXTRAIR PASTA (LOTE)";
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
            btnUpdatePak.TabIndex = 5;
            btnUpdatePak.Text = "🔄 INJETAR / ATUALIZAR";
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
            btnExtractAll.TabIndex = 6;
            btnExtractAll.Text = "💥 EXTRAIR ATUAL";
            btnExtractAll.UseVisualStyleBackColor = false;
            btnExtractAll.Click += btnExtractAll_Click;
            // 
            // lstEntries
            // 
            lstEntries.Columns.AddRange(new ColumnHeader[] { colName, colType, colSize, colCompSize });
            lstEntries.FullRowSelect = true;
            lstEntries.GridLines = true;
            lstEntries.Location = new Point(11, 92);
            lstEntries.Margin = new Padding(3, 2, 3, 2);
            lstEntries.Name = "lstEntries";
            lstEntries.Size = new Size(724, 234);
            lstEntries.TabIndex = 3;
            lstEntries.UseCompatibleStateImageBehavior = false;
            lstEntries.View = View.Details;
            // 
            // colName
            // 
            colName.Text = "Nome do Arquivo";
            colName.Width = 320;
            // 
            // colType
            // 
            colType.Text = "Tipo";
            colType.Width = 90;
            // 
            // colSize
            // 
            colSize.Text = "Tamanho Real";
            colSize.Width = 140;
            // 
            // colCompSize
            // 
            colCompSize.Text = "Tam. Compactado";
            colCompSize.Width = 140;
            // 
            // btnBrowsePak
            // 
            btnBrowsePak.Location = new Point(643, 10);
            btnBrowsePak.Margin = new Padding(3, 2, 3, 2);
            btnBrowsePak.Name = "btnBrowsePak";
            btnBrowsePak.Size = new Size(92, 22);
            btnBrowsePak.TabIndex = 1;
            btnBrowsePak.Text = "Buscar...";
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
            // tabCreate
            // 
            tabCreate.Controls.Add(lblReg);
            tabCreate.Controls.Add(lblLevel);
            tabCreate.Controls.Add(lblComp);
            tabCreate.Controls.Add(lblVol);
            tabCreate.Controls.Add(btnCreatePak);
            tabCreate.Controls.Add(cboRegion);
            tabCreate.Controls.Add(numCompressLevel);
            tabCreate.Controls.Add(cboCompressType);
            tabCreate.Controls.Add(cboVersion);
            tabCreate.Controls.Add(btnBrowseFolder);
            tabCreate.Controls.Add(txtSourceFolder);
            tabCreate.Location = new Point(4, 24);
            tabCreate.Margin = new Padding(3, 2, 3, 2);
            tabCreate.Name = "tabCreate";
            tabCreate.Padding = new Padding(18, 15, 18, 15);
            tabCreate.Size = new Size(748, 369);
            tabCreate.TabIndex = 1;
            tabCreate.Text = "Criar Novo PAK";
            tabCreate.UseVisualStyleBackColor = true;
            // 
            // lblReg
            // 
            lblReg.Location = new Point(389, 139);
            lblReg.Name = "lblReg";
            lblReg.Size = new Size(150, 17);
            lblReg.TabIndex = 0;
            lblReg.Text = "Região / Chaves (XTEA):";
            // 
            // lblLevel
            // 
            lblLevel.Location = new Point(20, 139);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(160, 17);
            lblLevel.TabIndex = 1;
            lblLevel.Text = "Nível de Compressão (0-9):";
            // 
            // lblComp
            // 
            lblComp.Location = new Point(389, 71);
            lblComp.Name = "lblComp";
            lblComp.Size = new Size(130, 17);
            lblComp.TabIndex = 2;
            lblComp.Text = "Tipo de Compressão:";
            // 
            // lblVol
            // 
            lblVol.Location = new Point(20, 71);
            lblVol.Name = "lblVol";
            lblVol.Size = new Size(120, 17);
            lblVol.TabIndex = 3;
            lblVol.Text = "Versão da Entrada:";
            // 
            // btnCreatePak
            // 
            btnCreatePak.BackColor = Color.FromArgb(0, 122, 204);
            btnCreatePak.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnCreatePak.ForeColor = Color.White;
            btnCreatePak.Location = new Point(20, 225);
            btnCreatePak.Margin = new Padding(3, 2, 3, 2);
            btnCreatePak.Name = "btnCreatePak";
            btnCreatePak.Size = new Size(706, 38);
            btnCreatePak.TabIndex = 10;
            btnCreatePak.Text = "📦 GERAR ARQUIVO .PAK";
            btnCreatePak.UseVisualStyleBackColor = false;
            btnCreatePak.Click += btnCreatePak_Click;
            // 
            // cboRegion
            // 
            cboRegion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRegion.Location = new Point(389, 158);
            cboRegion.Margin = new Padding(3, 2, 3, 2);
            cboRegion.Name = "cboRegion";
            cboRegion.Size = new Size(337, 23);
            cboRegion.TabIndex = 11;
            // 
            // numCompressLevel
            // 
            numCompressLevel.Location = new Point(20, 158);
            numCompressLevel.Margin = new Padding(3, 2, 3, 2);
            numCompressLevel.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            numCompressLevel.Name = "numCompressLevel";
            numCompressLevel.Size = new Size(324, 23);
            numCompressLevel.TabIndex = 12;
            numCompressLevel.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // cboCompressType
            // 
            cboCompressType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCompressType.Location = new Point(389, 90);
            cboCompressType.Margin = new Padding(3, 2, 3, 2);
            cboCompressType.Name = "cboCompressType";
            cboCompressType.Size = new Size(337, 23);
            cboCompressType.TabIndex = 13;
            // 
            // cboVersion
            // 
            cboVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVersion.Location = new Point(20, 90);
            cboVersion.Margin = new Padding(3, 2, 3, 2);
            cboVersion.Name = "cboVersion";
            cboVersion.Size = new Size(324, 23);
            cboVersion.TabIndex = 14;
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.Location = new Point(626, 31);
            btnBrowseFolder.Margin = new Padding(3, 2, 3, 2);
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new Size(101, 22);
            btnBrowseFolder.TabIndex = 1;
            btnBrowseFolder.Text = "Selecionar...";
            btnBrowseFolder.UseVisualStyleBackColor = true;
            btnBrowseFolder.Click += btnBrowseFolder_Click;
            // 
            // txtSourceFolder
            // 
            txtSourceFolder.Location = new Point(20, 32);
            txtSourceFolder.Margin = new Padding(3, 2, 3, 2);
            txtSourceFolder.Name = "txtSourceFolder";
            txtSourceFolder.PlaceholderText = "Arraste uma pasta aqui...";
            txtSourceFolder.ReadOnly = true;
            txtSourceFolder.Size = new Size(596, 23);
            txtSourceFolder.TabIndex = 0;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar1 });
            statusStrip1.Location = new Point(9, 405);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 12, 0);
            statusStrip1.Size = new Size(756, 22);
            statusStrip1.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(43, 17);
            lblStatus.Text = "Pronto";
            // 
            // progressBar1
            // 
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(150, 16);
            progressBar1.Visible = false;
            // 
            // FrmPakMaker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(774, 453);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "FrmPakMaker";
            Padding = new Padding(9, 8, 9, 26);
            Text = "LzPakTool - Interface Gráfica (Pangya)";
            tabControl1.ResumeLayout(false);
            tabExtract.ResumeLayout(false);
            tabExtract.PerformLayout();
            groupHeader.ResumeLayout(false);
            tabCreate.ResumeLayout(false);
            tabCreate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCompressLevel).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}