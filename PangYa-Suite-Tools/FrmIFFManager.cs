using System.ComponentModel;

namespace PangYa_Suite_Tools
{
    public partial class FrmIFFManager : Form
    {
        private Form? _activeEditorForm = null;
        private bool isInitializingLanguages = true;

        public FrmIFFManager()
        {
            InitializeComponent();
            InitializeLanguageComboBox();
        }

        public FrmIFFManager(string idiomaAtual)
        {
            InitializeComponent();
            cboLanguage.ComboBox.DisplayMember = "Key";
            cboLanguage.ComboBox.ValueMember = "Value";

            cboLanguage.Items.Add(new KeyValuePair<string, string>("Português (BR)", "br"));
            cboLanguage.Items.Add(new KeyValuePair<string, string>("English (US)", "en"));
            cboLanguage.SelectedIndex = idiomaAtual == "en" ? 1 : 0;

            isInitializingLanguages = false;
            ApplyLocalization(idiomaAtual);
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
            ComponentResourceManager res = new ComponentResourceManager(typeof(FrmIFFManager));
            string suffix = (lang == "en") ? "_en" : "_br";

            this.Text = res.GetString($"FrmIFFManager{suffix}") ?? this.Text;
            lblIffDir.Text = res.GetString($"lblIffDir{suffix}") ?? lblIffDir.Text;
            btnBrowseIffDir.Text = res.GetString($"btnBrowseIffDir{suffix}") ?? btnBrowseIffDir.Text;
            grpIffFiles.Text = res.GetString($"grpIffFiles{suffix}") ?? grpIffFiles.Text;
            lblLanguage.Text = res.GetString($"lblLanguage{suffix}") ?? lblLanguage.Text;

            // Apenas atualiza o texto de status/aviso padrão se nenhum diretório foi carregado ainda,
            // para não sobrescrever um estado dinâmico (ex: editor aberto) ao trocar o idioma.
            if (string.IsNullOrEmpty(txtIffDirectory.Text))
            {
                lblStatus.Text = GetText("Ready. Select the IFF files directory.", "Pronto. Selecione o diretório dos arquivos IFF.");
                lblNoFileSelected.Text = GetText("Select an .iff file from the list to load the editing table.", "Selecione um arquivo .iff na lista ao lado para carregar a tabela de edição.");
            }
        }

        private string GetText(string en, string br)
        {
            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
                return (selectedItem.Value == "br") ? br : en;
            return en;
        }

        private void btnBrowseIffDir_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = GetText("Select the extracted folder containing the .iff files (e.g.: Character.iff, Item.iff)", "Selecione a pasta extraída contendo os arquivos .iff (ex: Character.iff, Item.iff)")
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtIffDirectory.Text = fbd.SelectedPath;
                LoadIffFiles(fbd.SelectedPath);
            }
        }

        private void LoadIffFiles(string directoryPath)
        {
            lstIffFiles.Items.Clear();
            CloseActiveEditor();

            try
            {
                // Busca por todos os arquivos .iff na pasta selecionada
                string[] files = Directory.GetFiles(directoryPath, "*.iff", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    lstIffFiles.Items.Add(Path.GetFileName(file));
                }

                lblStatus.Text = $"{GetText("Scan complete.", "Varredura concluída.")} {lstIffFiles.Items.Count} {GetText("'.iff' file(s) found.", "arquivo(s) .iff encontrados.")}";

                if (lstIffFiles.Items.Count == 0)
                {
                    MessageBox.Show(GetText("No file with the '.iff' extension was found in this folder.", "Nenhum arquivo com a extensão '.iff' foi localizado nesta pasta."), GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetText("Error listing directory:", "Erro ao listar diretório:")} {ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstIffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstIffFiles.SelectedItem == null) return;

            string selectedFileName = lstIffFiles.SelectedItem.ToString()!;
            string fullPath = Path.Combine(txtIffDirectory.Text, selectedFileName);

            LoadSpecificIffEditor(selectedFileName, fullPath);
        }

        private void LoadSpecificIffEditor(string filename, string fullPath)
        {
            CloseActiveEditor();
            lblNoFileSelected.Visible = false;

            Form? targetForm = null;

            // Roteamento inteligente baseado no nome do arquivo IFF
            switch (filename.ToLower())
            {
                case "character.iff":
                    // targetForm = new FrmIffCharacter(fullPath); 
                    break;

                case "item.iff":
                    // targetForm = new FrmIffItem(fullPath);
                    break;

                // Adicione os novos cases conforme for criando as views de cada struct
                default:
                    break;
            }

            if (targetForm != null)
            {
                _activeEditorForm = targetForm;

                // Configura o Form para se comportar como um controle comum de painel (Injeção de View)
                targetForm.TopLevel = false;
                targetForm.FormBorderStyle = FormBorderStyle.None;
                targetForm.Dock = DockStyle.Fill;

                pnlEditorContainer.Controls.Add(targetForm);
                pnlEditorContainer.Tag = targetForm;
                targetForm.Show();

                lblStatus.Text = $"{GetText("Editing structure of:", "Editando estrutura de:")} {filename}";
            }
            else
            {
                lblNoFileSelected.Text = $"{GetText("⚠️ The editor layout/structure for the file", "⚠️ A estrutura/layout de editor para o arquivo")} '{filename}'\n{GetText("has not yet been implemented or mapped.", "ainda não foi implementada ou mapeada.")}";
                lblNoFileSelected.Visible = true;
                lblStatus.Text = $"{GetText("Editor not available for:", "Editor não disponível para:")} {filename}";
            }
        }

        private void CloseActiveEditor()
        {
            if (_activeEditorForm != null)
            {
                _activeEditorForm.Close();
                _activeEditorForm.Dispose();
                _activeEditorForm = null;
            }
            lblNoFileSelected.Text = GetText("Select an .iff file from the list to load the editing table.", "Selecione um arquivo .iff na lista ao lado para carregar a tabela de edição.");
            lblNoFileSelected.Visible = true;
        }
    }
}
