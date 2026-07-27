namespace PangYa_Suite_Tools
{
    partial class FrmMenu
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnOpenPakMaker;
        private System.Windows.Forms.Button btnOpenUpdateList;
        private System.Windows.Forms.Button btnOpenIffManager;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnOpenOptions;
        private System.Windows.Forms.Button btnOpenPakDiff;
        private System.Windows.Forms.Button btnOpenLog;
        private System.Windows.Forms.Button btnOpenShop;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnOpenPakMaker = new Button();
            btnOpenUpdateList = new Button();
            btnOpenIffManager = new Button();
            btnOpenPakDiff = new Button();
            btnOpenOptions = new Button();
            btnOpenLog = new Button();
            btnOpenShop = new CenteredImageButton();
            lblTitle = new Label();
            SuspendLayout();
            // 
            // btnOpenPakMaker
            // 
            btnOpenPakMaker.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenPakMaker.Location = new Point(42, 70);
            btnOpenPakMaker.Name = "btnOpenPakMaker";
            btnOpenPakMaker.Size = new Size(300, 50);
            btnOpenPakMaker.TabIndex = 1;
            btnOpenPakMaker.UseVisualStyleBackColor = true;
            btnOpenPakMaker.Click += btnOpenPakMaker_Click;
            // 
            // btnOpenUpdateList
            // 
            btnOpenUpdateList.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenUpdateList.Location = new Point(42, 135);
            btnOpenUpdateList.Name = "btnOpenUpdateList";
            btnOpenUpdateList.Size = new Size(300, 50);
            btnOpenUpdateList.TabIndex = 2;
            btnOpenUpdateList.UseVisualStyleBackColor = true;
            btnOpenUpdateList.Click += btnOpenUpdateList_Click;
            // 
            // btnOpenIffManager
            // 
            btnOpenIffManager.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenIffManager.Location = new Point(42, 200);
            btnOpenIffManager.Name = "btnOpenIffManager";
            btnOpenIffManager.Size = new Size(300, 50);
            btnOpenIffManager.TabIndex = 3;
            btnOpenIffManager.UseVisualStyleBackColor = true;
            btnOpenIffManager.Click += btnOpenIffManager_Click;
            // 
            // btnOpenPakDiff
            // 
            btnOpenPakDiff.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenPakDiff.Location = new Point(42, 265);
            btnOpenPakDiff.Name = "btnOpenPakDiff";
            btnOpenPakDiff.Size = new Size(300, 50);
            btnOpenPakDiff.TabIndex = 4;
            btnOpenPakDiff.Text = "🔍 Comparador PAK (Diff)";
            btnOpenPakDiff.UseVisualStyleBackColor = true;
            btnOpenPakDiff.Click += btnOpenPakDiff_Click;
            // 
            // btnOpenOptions
            // 
            btnOpenOptions.Location = new Point(103, 395);
            btnOpenOptions.Name = "btnOpenOptions";
            btnOpenOptions.Size = new Size(180, 35);
            btnOpenOptions.TabIndex = 6;
            btnOpenOptions.Text = "Options";
            btnOpenOptions.UseVisualStyleBackColor = true;
            btnOpenOptions.Click += btnOpenOptions_Click;
            //
            // btnOpenLog
            //
            btnOpenLog.Location = new Point(103, 440);
            btnOpenLog.Name = "btnOpenLog";
            btnOpenLog.Size = new Size(180, 35);
            btnOpenLog.TabIndex = 7;
            btnOpenLog.UseVisualStyleBackColor = true;
            btnOpenLog.Click += btnOpenLog_Click;
            //
            // btnOpenShop
            //
            btnOpenShop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenShop.Location = new Point(42, 330);
            btnOpenShop.Name = "btnOpenShop";
            btnOpenShop.Size = new Size(300, 50);
            btnOpenShop.TabIndex = 5;
            btnOpenShop.UseVisualStyleBackColor = true;
            btnOpenShop.Click += btnOpenShop_Click;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(12, 19);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(360, 30);
            lblTitle.TabIndex = 0;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FrmMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(402, 520);
            Controls.Add(btnOpenShop);
            Controls.Add(btnOpenLog);
            Controls.Add(btnOpenOptions);
            Controls.Add(btnOpenPakDiff);
            Controls.Add(btnOpenIffManager);
            Controls.Add(btnOpenUpdateList);
            Controls.Add(btnOpenPakMaker);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FrmMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pangya Studio - Menu Principal";
            ResumeLayout(false);
        }
    }
}
