using PangYa_Suite_Tools.Localization;
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
            LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
            Disposed += (_, _) => LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
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

            cboLanguage.Items.Add(new KeyValuePair<string, string>(Strings.Common_PortugueseBrazil, LocalizationManager.PortugueseBrazil));
            cboLanguage.Items.Add(new KeyValuePair<string, string>(Strings.Common_EnglishUS, LocalizationManager.English));
            cboLanguage.SelectedIndex = LocalizationManager.CurrentCulture.Name == LocalizationManager.PortugueseBrazil ? 0 : 1;

            isInitializingLanguages = false;
            ApplyLocalization();
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitializingLanguages) return;

            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                LocalizationManager.SetCulture(selectedItem.Value);
            }
        }

        private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
        {
            isInitializingLanguages = true;
            cboLanguage.SelectedIndex = LocalizationManager.CurrentCulture.Name == LocalizationManager.PortugueseBrazil ? 0 : 1;
            isInitializingLanguages = false;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            Text = Strings.Menu_Title;
            lblTitle.Text = Strings.Menu_Title;
            btnOpenPakMaker.Text = Strings.Menu_PakManager;
            btnOpenUpdateList.Text = Strings.Menu_UpdateList;
            btnOpenIffManager.Text = Strings.Menu_IffManager;
            btnOpenOptions.Text = Strings.Menu_Options;
            lblLanguage.Text = Strings.Common_Language;
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
            using (var frmOptions = new FrmOptions())
            {
                frmOptions.ShowDialog();
            }
        }

        
    }
}
