using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;

namespace PangYa_Suite_Tools
{
    public partial class FrmMenu : Form
    {
        private bool isInitializingLanguages = true;
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

                // Verifica se o arquivo existe e se é uma extensão .pak
                if (File.Exists(filePath) && filePath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                {
                    // Mudamos para o evento 'Shown', que garante uma alternância visual muito mais limpa e sem bugs
                    this.Shown += (s, e) =>
                    {
                        // Oculta o menu principal
                        this.Hide();

                        // Cria a instância do PakMaker passando o arquivo
                        FrmPakMaker pakMaker = new(filePath);

                        // Quando o PakMaker fechar, fecha o programa inteiro de forma limpa
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
            cboLanguage.SelectedIndex = 1;

            isInitializingLanguages = false;
            ApplyLocalization("en");
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitializingLanguages) return;

            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                ApplyLocalization(selectedItem.Value);
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
            // Vai demorar muito para mim fazer-lo, pois o codigo precisa ser bem organizado
            // Eu poderia fazer-lo 1 dia, mas eu tenho outras tarefas.
            // A base sera bem fraca no inicio, mas depois que toma forma, fica algo gigantesco.
            var iffManager = new FrmIFFManager();
            this.Hide();
            iffManager.ShowDialog();
            this.Show();
        }

        private void btnOpenOptions_Click(object sender, EventArgs e)
        {
            // Obtém o idioma selecionado em tempo real no menu principal ('br' ou 'en')
            string idiomaAtual = "en";
            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                idiomaAtual = selectedItem.Value;
            }

            // Instancia e abre o formulário de opções como diálogo modal
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

        
    }
}