using System.Xml.Linq;

namespace PangYa_Suite_Tools
{
    partial class FrmUpdateList
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
            tabMain = new TabControl();
            tabDecrypt = new TabPage();
            pnlCryptoDrop = new Panel();
            lblDropHint = new Label();
            txtXmlViewer = new TextBox();
            tabGenerator = new TabPage();
            grpConfig = new GroupBox();
            txtClientPatchNum = new TextBox();
            lblClientPatchNum = new Label();
            txtUpdateListVer = new TextBox();
            lblUpdateListVer = new Label();
            txtPatchVersion = new TextBox();
            lblPatchVersion = new Label();
            cboFileKey = new ComboBox();
            lblFileKey = new Label();
            btnBrowseUpdate = new Button();
            txtUpdatePath = new TextBox();
            lblUpdatePath = new Label();
            btnBrowsePangya = new Button();
            txtPangyaPath = new TextBox();
            lblPangyaPath = new Label();
            btnToggleWatch = new Button();
            lblWatchStatus = new Label();
            txtLog = new TextBox();
            lblLog = new Label();
            statusStrip1 = new StatusStrip();
            lblLanguage = new ToolStripStatusLabel();
            cboLanguage = new ToolStripComboBox();
            tabMain.SuspendLayout();
            tabDecrypt.SuspendLayout();
            pnlCryptoDrop.SuspendLayout();
            tabGenerator.SuspendLayout();
            grpConfig.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabDecrypt);
            tabMain.Controls.Add(tabGenerator);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(784, 538);
            tabMain.TabIndex = 0;
            // 
            // tabDecrypt
            // 
            tabDecrypt.Controls.Add(pnlCryptoDrop);
            tabDecrypt.Controls.Add(txtXmlViewer);
            tabDecrypt.Location = new Point(4, 24);
            tabDecrypt.Name = "tabDecrypt";
            tabDecrypt.Padding = new Padding(3);
            tabDecrypt.Size = new Size(776, 510);
            tabDecrypt.TabIndex = 0;
            tabDecrypt.UseVisualStyleBackColor = true;
            // 
            // pnlCryptoDrop
            // 
            pnlCryptoDrop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlCryptoDrop.BackColor = Color.GhostWhite;
            pnlCryptoDrop.BorderStyle = BorderStyle.FixedSingle;
            pnlCryptoDrop.Controls.Add(lblDropHint);
            pnlCryptoDrop.Location = new Point(8, 6);
            pnlCryptoDrop.Name = "pnlCryptoDrop";
            pnlCryptoDrop.Size = new Size(760, 100);
            pnlCryptoDrop.TabIndex = 0;
            // 
            // lblDropHint
            // 
            lblDropHint.Dock = DockStyle.Fill;
            lblDropHint.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            lblDropHint.ForeColor = Color.RoyalBlue;
            lblDropHint.Location = new Point(0, 0);
            lblDropHint.Name = "lblDropHint";
            lblDropHint.Size = new Size(758, 98);
            lblDropHint.TabIndex = 0;
            lblDropHint.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtXmlViewer
            // 
            txtXmlViewer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtXmlViewer.BackColor = Color.White;
            txtXmlViewer.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtXmlViewer.ForeColor = Color.DarkBlue;
            txtXmlViewer.Location = new Point(8, 112);
            txtXmlViewer.Multiline = true;
            txtXmlViewer.Name = "txtXmlViewer";
            txtXmlViewer.ReadOnly = true;
            txtXmlViewer.ScrollBars = ScrollBars.Both;
            txtXmlViewer.Size = new Size(760, 390);
            txtXmlViewer.TabIndex = 1;
            // 
            // tabGenerator
            // 
            tabGenerator.Controls.Add(grpConfig);
            tabGenerator.Controls.Add(btnToggleWatch);
            tabGenerator.Controls.Add(lblWatchStatus);
            tabGenerator.Controls.Add(txtLog);
            tabGenerator.Controls.Add(lblLog);
            tabGenerator.Location = new Point(4, 24);
            tabGenerator.Name = "tabGenerator";
            tabGenerator.Padding = new Padding(3);
            tabGenerator.Size = new Size(776, 510);
            tabGenerator.TabIndex = 1;
            tabGenerator.UseVisualStyleBackColor = true;
            // 
            // grpConfig
            // 
            grpConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpConfig.Controls.Add(txtClientPatchNum);
            grpConfig.Controls.Add(lblClientPatchNum);
            grpConfig.Controls.Add(txtUpdateListVer);
            grpConfig.Controls.Add(lblUpdateListVer);
            grpConfig.Controls.Add(txtPatchVersion);
            grpConfig.Controls.Add(lblPatchVersion);
            grpConfig.Controls.Add(cboFileKey);
            grpConfig.Controls.Add(lblFileKey);
            grpConfig.Controls.Add(btnBrowseUpdate);
            grpConfig.Controls.Add(txtUpdatePath);
            grpConfig.Controls.Add(lblUpdatePath);
            grpConfig.Controls.Add(btnBrowsePangya);
            grpConfig.Controls.Add(txtPangyaPath);
            grpConfig.Controls.Add(lblPangyaPath);
            grpConfig.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpConfig.Location = new Point(8, 6);
            grpConfig.Name = "grpConfig";
            grpConfig.Size = new Size(760, 185);
            grpConfig.TabIndex = 0;
            grpConfig.TabStop = false;
            // 
            // txtClientPatchNum
            // 
            txtClientPatchNum.Font = new Font("Segoe UI", 9F);
            txtClientPatchNum.Location = new Point(570, 149);
            txtClientPatchNum.Name = "txtClientPatchNum";
            txtClientPatchNum.Size = new Size(174, 23);
            txtClientPatchNum.TabIndex = 13;
            // 
            // lblClientPatchNum
            // 
            lblClientPatchNum.AutoSize = true;
            lblClientPatchNum.Font = new Font("Segoe UI", 9F);
            lblClientPatchNum.Location = new Point(570, 131);
            lblClientPatchNum.Name = "lblClientPatchNum";
            lblClientPatchNum.Size = new Size(87, 15);
            lblClientPatchNum.TabIndex = 12;
            // 
            // txtUpdateListVer
            // 
            txtUpdateListVer.Font = new Font("Segoe UI", 9F);
            txtUpdateListVer.Location = new Point(380, 149);
            txtUpdateListVer.Name = "txtUpdateListVer";
            txtUpdateListVer.Size = new Size(174, 23);
            txtUpdateListVer.TabIndex = 11;
            // 
            // lblUpdateListVer
            // 
            lblUpdateListVer.AutoSize = true;
            lblUpdateListVer.Font = new Font("Segoe UI", 9F);
            lblUpdateListVer.Location = new Point(380, 131);
            lblUpdateListVer.Name = "lblUpdateListVer";
            lblUpdateListVer.Size = new Size(107, 15);
            lblUpdateListVer.TabIndex = 10;
            // 
            // txtPatchVersion
            // 
            txtPatchVersion.Font = new Font("Segoe UI", 9F);
            txtPatchVersion.Location = new Point(190, 149);
            txtPatchVersion.Name = "txtPatchVersion";
            txtPatchVersion.Size = new Size(174, 23);
            txtPatchVersion.TabIndex = 9;
            // 
            // lblPatchVersion
            // 
            lblPatchVersion.AutoSize = true;
            lblPatchVersion.Font = new Font("Segoe UI", 9F);
            lblPatchVersion.Location = new Point(190, 131);
            lblPatchVersion.Name = "lblPatchVersion";
            lblPatchVersion.Size = new Size(94, 15);
            lblPatchVersion.TabIndex = 8;
            // 
            // cboFileKey
            // 
            cboFileKey.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFileKey.Font = new Font("Segoe UI", 9F);
            cboFileKey.FormattingEnabled = true;
            cboFileKey.Location = new Point(15, 149);
            cboFileKey.Name = "cboFileKey";
            cboFileKey.Size = new Size(160, 23);
            cboFileKey.TabIndex = 7;
            // 
            // lblFileKey
            // 
            lblFileKey.AutoSize = true;
            lblFileKey.Font = new Font("Segoe UI", 9F);
            lblFileKey.Location = new Point(15, 131);
            lblFileKey.Name = "lblFileKey";
            lblFileKey.Size = new Size(90, 15);
            lblFileKey.TabIndex = 6;
            // 
            // btnBrowseUpdate
            // 
            btnBrowseUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseUpdate.Font = new Font("Segoe UI", 9F);
            btnBrowseUpdate.Location = new Point(659, 95);
            btnBrowseUpdate.Name = "btnBrowseUpdate";
            btnBrowseUpdate.Size = new Size(85, 25);
            btnBrowseUpdate.TabIndex = 5;
            btnBrowseUpdate.UseVisualStyleBackColor = true;
            btnBrowseUpdate.Click += btnBrowseUpdate_Click;
            // 
            // txtUpdatePath
            // 
            txtUpdatePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUpdatePath.Font = new Font("Segoe UI", 9F);
            txtUpdatePath.Location = new Point(15, 96);
            txtUpdatePath.Name = "txtUpdatePath";
            txtUpdatePath.Size = new Size(638, 23);
            txtUpdatePath.TabIndex = 4;
            // 
            // lblUpdatePath
            // 
            lblUpdatePath.AutoSize = true;
            lblUpdatePath.Font = new Font("Segoe UI", 9F);
            lblUpdatePath.Location = new Point(15, 78);
            lblUpdatePath.Name = "lblUpdatePath";
            lblUpdatePath.Size = new Size(165, 15);
            lblUpdatePath.TabIndex = 3;
            // 
            // btnBrowsePangya
            // 
            btnBrowsePangya.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowsePangya.Font = new Font("Segoe UI", 9F);
            btnBrowsePangya.Location = new Point(659, 45);
            btnBrowsePangya.Name = "btnBrowsePangya";
            btnBrowsePangya.Size = new Size(85, 25);
            btnBrowsePangya.TabIndex = 2;
            btnBrowsePangya.UseVisualStyleBackColor = true;
            btnBrowsePangya.Click += btnBrowsePangya_Click;
            // 
            // txtPangyaPath
            // 
            txtPangyaPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPangyaPath.Font = new Font("Segoe UI", 9F);
            txtPangyaPath.Location = new Point(15, 46);
            txtPangyaPath.Name = "txtPangyaPath";
            txtPangyaPath.Size = new Size(638, 23);
            txtPangyaPath.TabIndex = 1;
            // 
            // lblPangyaPath
            // 
            lblPangyaPath.AutoSize = true;
            lblPangyaPath.Font = new Font("Segoe UI", 9F);
            lblPangyaPath.Location = new Point(15, 28);
            lblPangyaPath.Name = "lblPangyaPath";
            lblPangyaPath.Size = new Size(148, 15);
            lblPangyaPath.TabIndex = 0;
            // 
            // btnToggleWatch
            // 
            btnToggleWatch.BackColor = Color.LightGreen;
            btnToggleWatch.FlatStyle = FlatStyle.Flat;
            btnToggleWatch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnToggleWatch.Location = new Point(8, 203);
            btnToggleWatch.Name = "btnToggleWatch";
            btnToggleWatch.Size = new Size(240, 45);
            btnToggleWatch.TabIndex = 1;
            btnToggleWatch.UseVisualStyleBackColor = false;
            btnToggleWatch.Click += btnToggleWatch_Click;
            // 
            // lblWatchStatus
            // 
            lblWatchStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblWatchStatus.BorderStyle = BorderStyle.Fixed3D;
            lblWatchStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblWatchStatus.ForeColor = Color.DimGray;
            lblWatchStatus.Location = new Point(256, 203);
            lblWatchStatus.Name = "lblWatchStatus";
            lblWatchStatus.Size = new Size(512, 45);
            lblWatchStatus.TabIndex = 2;
            lblWatchStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.BackColor = Color.Black;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.Cyan;
            txtLog.Location = new Point(8, 275);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(760, 227);
            txtLog.TabIndex = 4;
            // 
            // lblLog
            // 
            lblLog.AutoSize = true;
            lblLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLog.Location = new Point(8, 257);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(142, 15);
            lblLog.TabIndex = 3;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblLanguage, cboLanguage });
            statusStrip1.Location = new Point(0, 538);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(784, 23);
            statusStrip1.TabIndex = 1;
            // 
            // lblLanguage
            // 
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new Size(47, 18);
            // 
            // cboLanguage
            // 
            cboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLanguage.Name = "cboLanguage";
            cboLanguage.Size = new Size(120, 23);
            cboLanguage.SelectedIndexChanged += cboLanguage_SelectedIndexChanged;
            // 
            // FrmUpdateList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(tabMain);
            Controls.Add(statusStrip1);
            MinimumSize = new Size(800, 600);
            Name = "FrmUpdateList";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Updatelist - Interface";
            tabMain.ResumeLayout(false);
            tabDecrypt.ResumeLayout(false);
            tabDecrypt.PerformLayout();
            pnlCryptoDrop.ResumeLayout(false);
            tabGenerator.ResumeLayout(false);
            tabGenerator.PerformLayout();
            grpConfig.ResumeLayout(false);
            grpConfig.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabDecrypt;
        private System.Windows.Forms.TabPage tabGenerator;
        private System.Windows.Forms.TextBox txtXmlViewer;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Button btnBrowseUpdate;
        private System.Windows.Forms.TextBox txtUpdatePath;
        private System.Windows.Forms.Label lblUpdatePath;
        private System.Windows.Forms.Button btnBrowsePangya;
        private System.Windows.Forms.TextBox txtPangyaPath;
        private System.Windows.Forms.Label lblPangyaPath;
        private System.Windows.Forms.TextBox txtPatchVersion;
        private System.Windows.Forms.Label lblPatchVersion;
        private System.Windows.Forms.TextBox txtUpdateListVer;
        private System.Windows.Forms.Label lblUpdateListVer;
        private System.Windows.Forms.TextBox txtClientPatchNum;
        private System.Windows.Forms.Label lblClientPatchNum;
        private System.Windows.Forms.ComboBox cboFileKey;
        private System.Windows.Forms.Label lblFileKey;
        private System.Windows.Forms.Button btnToggleWatch;
        private System.Windows.Forms.Label lblWatchStatus;
        private System.Windows.Forms.Panel pnlCryptoDrop;
        private System.Windows.Forms.Label lblDropHint;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblLanguage;
        private System.Windows.Forms.ToolStripComboBox cboLanguage;
    }
}
