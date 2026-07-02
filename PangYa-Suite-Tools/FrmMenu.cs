using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;

namespace PangYa_Suite_Tools
{
    public partial class FrmMenu : Form
    {
        private bool isInitializingLanguages = true;

        // Caminho no registro para salvar as configurações da Suite
        private const string RegistryKeyPath = @"Software\PangYaSuiteTools";
        private const string LanguageValueName = "Language";

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public FrmMenu()
        {
            InitializeComponent();
            InitializeLanguageComboBox();
        }

        public FrmMenu(string[] args) : this()
        {
            if (args != null && args.Length > 0)
            {
                string filePath = args[0];

                if (File.Exists(filePath) && filePath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                {
                    this.Shown += (s, e) =>
                    {
                        this.Hide();
                        FrmPakMaker pakMaker = new(filePath);
                        pakMaker.FormClosed += (sender, formArgs) => this.Close();
                        pakMaker.Show();
                    };
                }
            }
        }

        private void InitializeLanguageComboBox()
        {
            cboLanguage.ComboBox.DisplayMember = "Key";
            cboLanguage.ComboBox.ValueMember = "Value";

            cboLanguage.Items.Add(new KeyValuePair<string, string>("Português (BR)", "br"));
            cboLanguage.Items.Add(new KeyValuePair<string, string>("English (US)", "en"));

            // 1. Recupera o idioma salvo no Registro do Windows. O padrão é "en" se não existir.
            string savedLanguage = "en";
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        savedLanguage = key.GetValue(LanguageValueName, "en") as string ?? "en";
                    }
                }
            }
            catch { /* Ignora falhas de leitura silenciosamente */ }

            // 2. Define o índice correto no ComboBox baseado no que foi recuperado
            cboLanguage.SelectedIndex = (savedLanguage == "br") ? 0 : 1;

            isInitializingLanguages = false;
            ApplyLocalization(savedLanguage);
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitializingLanguages) return;

            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                string selectedLang = selectedItem.Value;

                // Aplica o idioma na UI
                ApplyLocalization(selectedLang);

                // 3. Salva a nova preferência de idioma de forma persistente no Registro
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                    {
                        key.SetValue(LanguageValueName, selectedLang);
                    }
                }
                catch { /* Ignora falhas de escrita (ex: permissões restritas em sandbox) */ }
            }
        }

        private void ApplyLocalization(string lang)
        {
            ComponentResourceManager res = new ComponentResourceManager(typeof(FrmMenu));
            string suffix = (lang == "en") ? "_en" : "_br";

            this.Text = res.GetString($"FrmMenu{suffix}") ?? this.Text;
            lblTitle.Text = res.GetString($"lblTitle{suffix}") ?? lblTitle.Text;
            btnOpenPakMaker.Text = res.GetString($"btnOpenPakMaker{suffix}") ?? btnOpenPakMaker.Text;
            btnOpenUpdateList.Text = res.GetString($"btnOpenUpdateList{suffix}") ?? btnOpenUpdateList.Text;
            btnOpenIffManager.Text = res.GetString($"btnOpenIffManager{suffix}") ?? btnOpenIffManager.Text;
            lblLanguage.Text = res.GetString($"lblLanguage{suffix}") ?? lblLanguage.Text;

            // Adicionado suporte à tradução do novo botão de Diff se aplicável
            if (res.GetString($"btnOpenPakDiff{suffix}") != null)
            {
                // btnOpenPakDiff.Text = res.GetString($"btnOpenPakDiff{suffix}");
            }
        }

        private void btnOpenPakMaker_Click(object sender, EventArgs e)
        {
            var pakMaker = new FrmPakMaker();
            this.Hide();
            pakMaker.ShowDialog();
            this.Show();
        }

        private void btnOpenUpdateList_Click(object sender, EventArgs e)
        {
            var updateList = new FrmUpdateList();
            this.Hide();
            updateList.ShowDialog();
            this.Show();
        }

        private void btnOpenIffManager_Click(object sender, EventArgs e)
        {
            var iffManager = new FrmIFFManager();
            this.Hide();
            iffManager.ShowDialog();
            this.Show();
        }

        private void btnOpenOptions_Click(object sender, EventArgs e)
        {
            string idiomaAtual = "en";
            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                idiomaAtual = selectedItem.Value;
            }

            using (var frmOptions = new FrmOptions(idiomaAtual))
            {
                frmOptions.ShowDialog();
            }
        }

        private string GetText(string en, string br)
        {
            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                string _currentLanguage = selectedItem.Value;
                return (_currentLanguage == "br") ? br : en;
            }
            return "";
        }

        //obter a linguagem atual do comboBox
        private string GetLanguage()
        {
            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                return selectedItem.Value;
            }
            return "_en";
        }
    }
}