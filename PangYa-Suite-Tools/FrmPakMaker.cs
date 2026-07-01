using PangyaAPI.PAK.Flags;
using PangyaAPI.PAK.Models;
using System.ComponentModel;
using System.Data;

namespace PangYa_Suite_Tools
{
    public partial class FrmPakMaker : Form
    {
        private PakReader? _currentReader;

        // Tag do nó raiz virtual da árvore = "ver todos os arquivos" (modo lista completa)
        private const string RootFolderTag = "";

        // Mapa caminho-da-pasta -> TreeNode, para navegação rápida (duplo clique em uma pasta na lista)
        private readonly Dictionary<string, TreeNode> _folderNodes = new(StringComparer.OrdinalIgnoreCase);

        // Conjunto de entries atualmente "no escopo" (pasta selecionada na árvore), antes do filtro de pesquisa
        private List<PakFileEntry> _scopedEntries = [];
        private bool isInitializingLanguages = true;
        public FrmPakMaker()
        {
            InitializeComponent();
            InitializeLanguageComboBox();
            SetupCustomComponents();
            LoadSetupOptions();
            SetupContextMenu(); // Inicializa o menu de contexto da ListView
            CleanupOldTempDragFolders(); // Remove resíduos de exportações de drag-out de execuções anteriores
        }


        public FrmPakMaker(string initialPakPath) : this()
        {
            this.Shown += (s, e) =>
            {
                if (!string.IsNullOrEmpty(initialPakPath) && File.Exists(initialPakPath))
                {
                    LoadPak(initialPakPath);
                }
            };
        }


        private void InitializeLanguageComboBox()
        {
            cboLanguage.ComboBox.DisplayMember = "Key";
            cboLanguage.ComboBox.ValueMember = "Value";

            // Usando KeyValuePair para garantir tipagem forte e evitar bugs no ToolStrip
            cboLanguage.Items.Add(new KeyValuePair<string, string>("Português (BR)", "br"));
            cboLanguage.Items.Add(new KeyValuePair<string, string>("English (US)", "en"));
            cboLanguage.SelectedIndex = 1;

            isInitializingLanguages = false;

            // Executa a primeira tradução com base na seleção inicial
            ApplyLocalization("en");
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitializingLanguages) return;

            if (cboLanguage.SelectedItem is KeyValuePair<string, string> selectedItem)
            {
                string targetLanguage = selectedItem.Value;
                ApplyLocalization(targetLanguage);
            }
        }

        private void ApplyLocalization(string lang)
        {
            ComponentResourceManager res = new ComponentResourceManager(typeof(FrmPakMaker));
            string suffix = (lang == "en") ? "_en" : "_br";

            // Janela Principal e Label do Combo
            this.Text = res.GetString($"FrmPakMaker{suffix}") ?? this.Text;
            lblLanguage.Text = res.GetString($"lblLanguage{suffix}") ?? lblLanguage.Text;

            // --- ABA 1: LEITOR & MODIFICAÇÕES ---
            tabExtract.Text = res.GetString($"tabExtract{suffix}") ?? tabExtract.Text;
            btnBrowsePak.Text = res.GetString($"btnBrowsePak{suffix}") ?? btnBrowsePak.Text;
            txtPakPath.PlaceholderText = res.GetString($"txtPakPathHint{suffix}") ?? txtPakPath.PlaceholderText;

            // Grupo Cabeçalho
            groupHeader.Text = res.GetString($"groupHeader{suffix}") ?? groupHeader.Text;
            lblAuthor.Text = res.GetString($"lblAuthor{suffix}") ?? lblAuthor.Text;
            lblVersion.Text = res.GetString($"lblVersion{suffix}") ?? lblVersion.Text;
            lblEntries.Text = res.GetString($"lblEntries{suffix}") ?? lblEntries.Text;

            // Pesquisa e Listagem
            lblSearch.Text = res.GetString($"lblSearch{suffix}") ?? lblSearch.Text;
            txtSearch.PlaceholderText = res.GetString($"txtSearchHint{suffix}") ?? txtSearch.PlaceholderText;
            lblCurrentPath.Text = res.GetString($"lblCurrentPath{suffix}") ?? lblCurrentPath.Text;

            // Botões de Ação
            btnExtractSelected.Text = res.GetString($"btnExtractSelected{suffix}") ?? btnExtractSelected.Text;
            btnRemoveSelected.Text = res.GetString($"btnRemoveSelected{suffix}") ?? btnRemoveSelected.Text;
            btnBatchExtract.Text = res.GetString($"btnBatchExtract{suffix}") ?? btnBatchExtract.Text;
            btnUpdatePak.Text = res.GetString($"btnUpdatePak{suffix}") ?? btnUpdatePak.Text;
            btnExtractAll.Text = res.GetString($"btnExtractAll{suffix}") ?? btnExtractAll.Text;

            // Colunas de Exibição
            colName.Text = res.GetString($"colName{suffix}") ?? colName.Text;
            colType.Text = res.GetString($"colType{suffix}") ?? colType.Text;
            colSize.Text = res.GetString($"colSize{suffix}") ?? colSize.Text;
            colCompSize.Text = res.GetString($"colCompSize{suffix}") ?? colCompSize.Text;

            // Painel Inferior XTEA
            lblNewKey.Text = res.GetString($"lblNewKey{suffix}") ?? lblNewKey.Text;
            btnChangeKey.Text = res.GetString($"btnChangeKey{suffix}") ?? btnChangeKey.Text;


            // --- ABA 2: CRIAR NOVO PAK ---
            tabCreate.Text = res.GetString($"tabCreate{suffix}") ?? tabCreate.Text;
            txtSourceFolder.PlaceholderText = res.GetString($"txtSourceFolderHint{suffix}") ?? txtSourceFolder.PlaceholderText;
            btnBrowseFolder.Text = res.GetString($"btnBrowseFolder{suffix}") ?? btnBrowseFolder.Text;

            lblVol.Text = res.GetString($"lblVol{suffix}") ?? lblVol.Text;
            lblComp.Text = res.GetString($"lblComp{suffix}") ?? lblComp.Text;
            lblLevel.Text = res.GetString($"lblLevel{suffix}") ?? lblLevel.Text;
            lblReg.Text = res.GetString($"lblReg{suffix}") ?? lblReg.Text;
            btnCreatePak.Text = res.GetString($"btnCreatePak{suffix}") ?? btnCreatePak.Text;


            // --- COMPONENTES GLOBAIS ---
            lblStatus.Text = res.GetString($"lblStatus{suffix}") ?? lblStatus.Text;

            // Menu de contexto (criado dinamicamente em código, não pelo Designer)
            if (_menuExtractSingle != null)
                _menuExtractSingle.Text = GetText("📁 Extract selected item(s)...", "📁 Extrair selecionado(s)...");
            if (_menuRemoveSingle != null)
                _menuRemoveSingle.Text = GetText("🗑️ Remove selected item(s) from PAK...", "🗑️ Remover selecionado(s) do PAK...");
            if (_menuExtractFolder != null)
                _menuExtractFolder.Text = GetText("📁 Extract this folder...", "📁 Extrair esta pasta...");
            if (_menuRemoveFolder != null)
                _menuRemoveFolder.Text = GetText("🗑️ Remove this folder from PAK...", "🗑️ Remover esta pasta do PAK...");
        }

        private void SetupCustomComponents()
        {
            // Ativa o Drag-and-Drop no formulário principal e nas caixas de texto
            this.AllowDrop = true;
            this.DragEnter += FrmPakMaker_DragEnter;
            this.DragLeave += FrmPakMaker_DragLeave;
            this.DragDrop += FrmPakMaker_DragDrop;
            tvFolders.CheckBoxes = true; 
            lstEntries.MultiSelect = true;
            lstEntries.DoubleClick += LstEntries_DoubleClick;
            tvFolders.AfterSelect += TvFolders_AfterSelect;
            txtSearch.TextChanged += (s, e) => ApplyDisplayFilter();

            // Permite arrastar arquivos para dentro da lista de entries, para injetar/atualizar no PAK já carregado
            lstEntries.AllowDrop = true;
            lstEntries.DragEnter += LstEntries_DragEnter;
            lstEntries.DragDrop += LstEntries_DragDrop;

            tvFolders.KeyDown += TvFolders_KeyDown; // Detecta a tecla Delete
            tvFolders.NodeMouseClick += TvFolders_NodeMouseClick; // Seleciona com botão direito e abre o menu
            SetupFolderContextMenu(); // Inicializa o menu de contexto da árvore de pastas

            // Permite arrastar os itens selecionados (ou uma pasta) PARA FORA do app, extraindo-os direto no Explorer
            lstEntries.ItemDrag += LstEntries_ItemDrag;
            tvFolders.ItemDrag += TvFolders_ItemDrag;

        }

        /// <summary>
        /// Inicializa o menu de contexto específico para a TreeView de pastas.
        /// </summary>
        private void SetupFolderContextMenu()
        {
            ContextMenuStrip folderContextMenu = new ContextMenuStrip();
            _menuExtractFolder = new ToolStripMenuItem(GetText("📁 Extract this folder...", "📁 Extrair esta pasta..."));
            _menuExtractFolder.Click += async (s, e) => await ExtractSelectedFolderAsync();

            _menuRemoveFolder = new ToolStripMenuItem(GetText("🗑️ Remove this folder from PAK...", "🗑️ Remover esta pasta do PAK..."));
            _menuRemoveFolder.Click += async (s, e) => await RemoveFolderFromTreeAsync();

            folderContextMenu.Items.Add(_menuExtractFolder);
            folderContextMenu.Items.Add(_menuRemoveFolder);
            tvFolders.ContextMenuStrip = folderContextMenu;
        }

        // ─── ARRASTAR PARA FORA (DRAG-OUT / EXPORTAÇÃO RÁPIDA) ─────────────────

        /// <summary>
        /// Extrai as entries fornecidas para uma pasta temporária e devolve os caminhos de
        /// nível superior (arquivos soltos na raiz da pasta temp), prontos para iniciar um
        /// DoDragDrop nativo com DataFormats.FileDrop.
        /// </summary>
        private string[]? ExtractEntriesToTempForDrag(List<PakFileEntry> entries, string rootToStrip)
        {
            if (_currentReader == null || entries.Count == 0) return null;

            string tempDir = Path.Combine(Path.GetTempPath(), "PangYaSuiteTools_DragExport_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            foreach (var entry in entries)
            {
                string relativePath = entry.Name.Replace('\\', '/');
                if (!string.IsNullOrEmpty(rootToStrip) && relativePath.StartsWith(rootToStrip, StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(rootToStrip.Length);

                string outPath = Path.Combine(tempDir, relativePath.Replace('/', '\\'));
                string? fileDir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                    Directory.CreateDirectory(fileDir);

                _currentReader.ExtractEntry(entry, outPath);
            }

            // Devolve apenas os itens que ficaram no nível raiz da pasta temp (arquivos e/ou subpastas)
            return Directory.GetFileSystemEntries(tempDir);
        }

        /// <summary>Apaga pastas temporárias de drag-out deixadas por execuções anteriores (best-effort).</summary>
        private static void CleanupOldTempDragFolders()
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(Path.GetTempPath(), "PangYaSuiteTools_DragExport_*"))
                {
                    try { Directory.Delete(dir, true); } catch { /* ainda em uso, ignora */ }
                }
            }
            catch { /* ignora falhas de acesso ao %TEMP% */ }
        }

        /// <summary>Arrastar arquivo(s) selecionado(s) na ListView para fora do app (Explorer, etc.) extrai-os direto no destino.</summary>
        private async void LstEntries_ItemDrag(object? sender, ItemDragEventArgs e)
        {

            if (_currentReader == null || lstEntries.SelectedItems.Count == 0) return;

            // Filtra as entradas válidas que estão selecionadas
            var selectedEntries = lstEntries.SelectedItems
                .Cast<ListViewItem>()
                .Select(i => (PakFileEntry)i.Tag)
                .Where(en => en.Type != PakFileEntryType.Directory)
                .ToList();

            if (selectedEntries.Count == 0) return;

            // Pasta temporária para onde faremos a extração rápida antes de entregar ao Windows Explorer
            string tempSessionDir = Path.Combine(Path.GetTempPath(), "PangYaSuiteTools_DragDrop", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempSessionDir);

            List<string> filesToDrop = new();

            // Extrai rapidamente em background
            lblStatus.Text = GetText("Preparing files for dragging...", "Preparando arquvivos para arrastar...");
            await Task.Run(() =>
            {
                foreach (var entry in selectedEntries)
                {
                    // Salva na pasta temporária mantendo apenas o nome do arquivo
                    string suggestedName = Path.GetFileName(entry.Name.Replace('/', '\\'));
                    string outPath = Path.Combine(tempSessionDir, suggestedName);

                    _currentReader.ExtractEntry(entry, outPath);
                    filesToDrop.Add(outPath);
                }
            });
            lblStatus.Text = GetText("Ready", "Pronto");

            // Executa a operação nativa do Windows de arrastar e soltar arquivos físicos
            var dataObject = new DataObject(DataFormats.FileDrop, filesToDrop.ToArray());
            DoDragDrop(dataObject, DragDropEffects.Copy);
        }

        /// <summary>Arrastar uma pasta da TreeView para fora do app extrai a pasta inteira (com subpastas) direto no destino.</summary>
        private async void TvFolders_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (_currentReader == null || tvFolders.SelectedNode == null) return;

            // Usa o Tag do nó (caminho real da pasta, sem emojis/localização) em vez de
            // fazer parsing do texto exibido — muito mais confiável.
            string folderTag = tvFolders.SelectedNode.Tag as string ?? "";
            string cleanPath = folderTag.Replace('\\', '/'); // "" = raiz (todos os arquivos)

            List<PakFileEntry> entriesToExtract;
            string rootToStrip = "";

            if (string.IsNullOrWhiteSpace(cleanPath))
            {
                entriesToExtract = _currentReader.Entries.Where(en => en.Type != PakFileEntryType.Directory).ToList();
            }
            else
            {
                string prefix = cleanPath.Trim('/') + "/";

                // rootToStrip = tudo até a pasta selecionada (inclusive), preservando só
                // o que vem DEPOIS dela. Ex.: selecionado "data/map" -> rootToStrip = "data/map/"
                rootToStrip = prefix;

                entriesToExtract = _currentReader.Entries
                    .Where(en => en.Type != PakFileEntryType.Directory &&
                                 en.Name.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (entriesToExtract.Count == 0) return;

            string tempSessionDir = Path.Combine(Path.GetTempPath(), "PangYaSuiteTools_DragDrop", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempSessionDir);

            List<string> directoriesToDrop = new();

            // Nome da pasta raiz a ser arrastada (só o último segmento, ex.: "map")
            string selectedFolderName = string.IsNullOrEmpty(cleanPath) ? "" : Path.GetFileName(cleanPath.TrimEnd('/'));

            lblStatus.Text = GetText("Preparing folder structure for dragging", "Preparando estrutura de pasta para arrastar...");
            await Task.Run(() =>
            {
                foreach (var entry in entriesToExtract)
                {
                    string relativePath = entry.Name.Replace('\\', '/');

                    if (!string.IsNullOrEmpty(rootToStrip) && relativePath.StartsWith(rootToStrip, StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = relativePath.Substring(rootToStrip.Length);
                    }

                    // Reanexa o nome da pasta selecionada como raiz da estrutura arrastada
                    // (ex.: "map/arquivos/file.ext" em vez de só "arquivos/file.ext")
                    if (!string.IsNullOrEmpty(selectedFolderName))
                    {
                        relativePath = selectedFolderName + "/" + relativePath;
                    }

                    string localRelativePath = relativePath.Replace('/', '\\');
                    string outPath = Path.Combine(tempSessionDir, localRelativePath);

                    string? fileDir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    _currentReader.ExtractEntry(entry, outPath);
                }

                string firstLevelDir = Path.Combine(tempSessionDir, selectedFolderName);
                if (!string.IsNullOrEmpty(selectedFolderName) && Directory.Exists(firstLevelDir))
                {
                    directoriesToDrop.Add(firstLevelDir);
                }
                else
                {
                    // Raiz "Todos os Arquivos" — pega tudo que foi extraído direto no tempSessionDir
                    directoriesToDrop.AddRange(Directory.GetDirectories(tempSessionDir));
                    directoriesToDrop.AddRange(Directory.GetFiles(tempSessionDir));
                }
            });
            lblStatus.Text = GetText("Ready", "Pronto");

            if (directoriesToDrop.Count > 0)
            {
                var dataObject = new DataObject(DataFormats.FileDrop, directoriesToDrop.ToArray());
                DoDragDrop(dataObject, DragDropEffects.Copy);
            }
        }

        private void SetupContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            _menuExtractSingle = new ToolStripMenuItem(GetText("📁 Extract selected item(s)...", "📁 Extrair selecionado(s)..."));
            _menuExtractSingle.Click += async (s, e) => await ExtractSelectedAsync();

            _menuRemoveSingle = new ToolStripMenuItem(GetText("🗑️ Remove selected item(s) from PAK...", "🗑️ Remover selecionado(s) do PAK..."));
            _menuRemoveSingle.Click += async (s, e) => await RemoveSelectedAsync();

            contextMenu.Items.Add(_menuExtractSingle);
            contextMenu.Items.Add(_menuRemoveSingle);
            lstEntries.ContextMenuStrip = contextMenu; // Vincula o menu à ListView
        }

        private void LoadSetupOptions()
        {
            // Popula os seletores usando os Enums e Listas da sua PangyaAPI
            cboVersion.DataSource = Enum.GetValues(typeof(PakFileEntryVersion));
            cboVersion.SelectedItem = PakFileEntryVersion.V3;

            cboCompressType.DataSource = Enum.GetValues(typeof(PakFileEntryType));
            cboCompressType.SelectedItem = PakFileEntryType.LZ772;

            cboRegion.DataSource = PakKeys.All
                .Select(x => new { Label = x.Label, Keys = x.Keys })
                .ToList();
            cboRegion.DisplayMember = "Label";
            cboRegion.SelectedIndex = 0;

            // Combo de chave/região destino, usado na troca de chave XTEA (aba de extração)
            cboNewRegion.DataSource = PakKeys.All
                .Select(x => new { Label = x.Label, Keys = x.Keys })
                .ToList();
            cboNewRegion.DisplayMember = "Label";
            cboNewRegion.SelectedIndex = 0;
        }

        private void FrmPakMaker_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                txtPakPath.BackColor = Color.LightCyan;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FrmPakMaker_DragLeave(object? sender, EventArgs e)
        {
            txtPakPath.BackColor = SystemColors.Control;
        }

        private async void FrmPakMaker_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                txtPakPath.BackColor = SystemColors.Control;
                int currentTab = tabControl1.SelectedIndex;

                if (currentTab == 0)
                {
                    if (_currentReader == null || string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
                    {
                        string firstPath = files[0];
                        if (File.Exists(firstPath) && firstPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                        {
                            txtPakPath.Text = firstPath;
                            LoadPak(firstPath);
                        }
                        else
                        {
                            MessageBox.Show(
                                GetText("Please drag a valid .pak file extension to open.", "Por favor, arraste um arquivo válido de extensão .pak para abrir."),
                                GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        var itemsToInject = new List<PakInjectItem>();

                        // Usa o Tag do nó selecionado (caminho real da pasta) em vez de
                        // fazer parsing do FullPath exibido — evita falhas com subpastas/idiomas.
                        string virtualTargetFolder = "";
                        if (tvFolders.SelectedNode != null)
                        {
                            string folderTag = tvFolders.SelectedNode.Tag as string ?? "";
                            virtualTargetFolder = folderTag.Replace('\\', '/').Trim('/');
                            if (!string.IsNullOrEmpty(virtualTargetFolder))
                                virtualTargetFolder += "/";
                        }

                        foreach (string path in files)
                        {
                            if (File.Exists(path))
                            {
                                itemsToInject.Add(new PakInjectItem(path, null));
                            }
                            else if (Directory.Exists(path))
                            {
                                string[] allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                                foreach (string file in allFiles)
                                {
                                    string fileRelativeName = Path.GetFileName(path) + "/" +
                                                              Path.GetRelativePath(path, file).Replace('\\', '/');

                                    fileRelativeName = fileRelativeName.Trim('/');
                                    string finalVirtualPath = virtualTargetFolder + fileRelativeName;

                                    itemsToInject.Add(new PakInjectItem(file, finalVirtualPath));
                                }
                            }
                        }

                        if (itemsToInject.Count == 0)
                        {
                            MessageBox.Show(
                                GetText("No valid files or folders were found for injection.", "Nenhum arquivo ou pasta válida foi encontrado para injeção."),
                                GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        await InjectFilesIntoCurrentPakAsync(itemsToInject);
                    }
                }
                else if (currentTab == 1)
                {
                    string path = files[0];
                    if (Directory.Exists(path))
                    {
                        txtSourceFolder.Text = path;
                    }
                    else
                    {
                        MessageBox.Show(
                            GetText("Please drag a valid folder to compile a new PAK.", "Por favor, arraste uma pasta válida para compilar um novo PAK."),
                            GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        // ─── ABA 1: LEITURA E EXTRAÇÃO ─────────────────────────────────────────
        private void btnBrowsePak_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog { Filter = "Pangya PAK Files (*.pak)|*.pak" };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtPakPath.Text = openFileDialog.FileName;
                LoadPak(openFileDialog.FileName);
            }
        }

        private void LoadPak(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            try
            {
                if (string.IsNullOrEmpty(txtPakPath.Text))
                    txtPakPath.Text = path;

                _currentReader?.Dispose();
                _currentReader = new PakReader(path);
                _currentReader.Parse();
                // Atualiza as Labels de informação do Header
                txtUpdateAuthor.Text = _currentReader?.Header.Author;
                lblAuthor.Text = $"{GetText("Author:", "Autor:")} {_currentReader.Header.Author}";
                lblVersion.Text = $"{GetText("Version:", "Versão:")} 0x{_currentReader.Header.Version:X2}";
                lblEntries.Text = $"{GetText("Entries:", "Entradas:")} {_currentReader.Header.NumFileEntry}";

                txtSearch.Text = "";
                BuildFolderTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetText("Error opening PAK file:", "Erro ao abrir o arquivo PAK:")}\n{ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── NAVEGAÇÃO POR PASTAS ───────────────────────────────────────────────

        /// <summary>
        /// Monta a árvore de pastas a partir dos nomes internos das entries, e mantém
        /// um nó raiz virtual "Todos os Arquivos" que funciona como a visão de lista completa.
        /// </summary>
        private void BuildFolderTree()
        {
            tvFolders.BeginUpdate();
            tvFolders.Nodes.Clear();
            _folderNodes.Clear();

            var rootNode = new TreeNode(GetText("🗂 All Files", "🗂 Todos os Arquivos")) { Tag = RootFolderTag };
            tvFolders.Nodes.Add(rootNode);
            _folderNodes[RootFolderTag] = rootNode;

            if (_currentReader != null)
            {
                // Garante que toda pasta (mesmo sem uma entry "Directory" explícita) exista na árvore
                foreach (var entry in _currentReader.Entries)
                {
                    if (entry.Type == PakFileEntryType.Directory) continue;

                    string folder = Path.GetDirectoryName(entry.Name.Replace('/', '\\')) ?? "";
                    EnsureFolderNode(folder);
                }
            }

            tvFolders.EndUpdate();
            tvFolders.SelectedNode = rootNode;
        }

        /// <summary>
        /// Garante que o caminho de pasta (e todos os seus pais) existam como TreeNode,
        /// criando-os recursivamente se necessário. Retorna o TreeNode correspondente.
        /// </summary>
        private TreeNode EnsureFolderNode(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return _folderNodes[RootFolderTag];

            if (_folderNodes.TryGetValue(folderPath, out var existing))
                return existing;

            string parentPath = Path.GetDirectoryName(folderPath) ?? "";
            TreeNode parentNode = EnsureFolderNode(parentPath);

            string folderName = Path.GetFileName(folderPath);
            var node = new TreeNode("📁 " + folderName) { Tag = folderPath };
            parentNode.Nodes.Add(node);
            _folderNodes[folderPath] = node;
            return node;
        }

        /// <summary>
        /// Garante que o clique com o botão direito selecione o nó antes de abrir o menu suspenso.
        /// </summary>
        private void TvFolders_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tvFolders.SelectedNode = e.Node; // Força a seleção do nó clicado
            }
        }

        /// <summary>
        /// Captura o botão "Delete" do teclado para acionar a remoção da pasta selecionada.
        /// </summary>
        private async void TvFolders_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true; // Evita bipes do Windows
                await RemoveFolderFromTreeAsync();
            }
        }

        private void TvFolders_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (_currentReader == null) return;

            string folderTag = e.Node?.Tag as string ?? RootFolderTag;

            _scopedEntries = string.IsNullOrEmpty(folderTag)
                ? [.. _currentReader.Entries.Where(en => en.Type != PakFileEntryType.Directory)]
                : _currentReader.Entries
                    .Where(en => string.Equals(
                        Path.GetDirectoryName(en.Name.Replace('/', '\\')) ?? "",
                        folderTag,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

            lblCurrentPath.Text = string.IsNullOrEmpty(folderTag)
                ? GetText("📂 Path: (all files)", "📂 Caminho: (todos os arquivos)")
                : $"{GetText("📂 Path:", "📂 Caminho:")} {folderTag.Replace('\\', '/')}";

            ApplyDisplayFilter();
        }

        /// <summary>
        /// Executa a lógica de remoção da pasta atualmente selecionada na TreeView.
        /// </summary>
        private async Task RemoveFolderFromTreeAsync()
        {
            if (_currentReader == null || string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
            {
                MessageBox.Show(GetText("Select an active .pak file first.", "Selecione um arquivo .pak ativo primeiro."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (tvFolders.SelectedNode == null) return;

            string folderTag = tvFolders.SelectedNode.Tag as string ?? "";

            // Evita deletar tudo caso esteja no nó raiz virtual sem querer
            if (string.IsNullOrEmpty(folderTag))
            {
                MessageBox.Show(GetText("You cannot delete the root folder. Select a specific folder.", "Você não pode deletar a pasta raiz. Selecione uma pasta específica."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string prefix = folderTag.Replace('\\', '/').Trim('/') + "/";

            // Coleta todos os arquivos que pertencem àquela estrutura de pasta
            var filesInFolder = _currentReader.Entries
                .Where(en => en.Type != PakFileEntryType.Directory &&
                             en.Name.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(en => en.Name)
                .ToList();

            if (filesInFolder.Count == 0)
            {
                MessageBox.Show(GetText("No files found inside this folder to delete.", "Nenhum arquivo encontrado dentro desta pasta para apagar."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Mensagem de confirmação específica para a pasta selecionada
            string folderName = tvFolders.SelectedNode.Text.Replace("📁 ", "");
            string confirmationMessage = GetText(
                $"Are you sure you want to remove the folder '{folderName}' and all of its contents?\nTotal files to delete: {filesInFolder.Count}.",
                $"Tem certeza de que deseja remover a pasta '{folderName}' e todo o seu conteúdo?\nTotal de arquivos a serem apagados: {filesInFolder.Count}.");

            var confirm = MessageBox.Show(
                $"{confirmationMessage}\n\n{GetText("The PAK will be rebuilt and a backup (.bak) will be created automatically.", "O PAK será reconstruído e um backup (.bak) será criado automaticamente.")}",
                GetText("Confirm Removal", "Confirmar remoção"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            string pakPath = txtPakPath.Text;
            var reader = _currentReader;

            lblStatus.Text = GetText("Removing and rebuilding PAK...", "Removendo e reconstruindo PAK...");
            tvFolders.Enabled = false; // Bloqueia a árvore durante o processo

            try
            {
                var options = BuildRebuildOptionsForCurrentPak();

                await Task.Run(() =>
                {
                    PakManager.RemoveFiles(pakPath, reader, filesInFolder, options,
                        log: msg => { },
                        onProgress: (done, total) => ReportProgress(done, total, GetText("Rebuilding PAK", "Reconstruindo PAK")));
                });

                lblStatus.Text = GetText("Removal completed", "Remoção concluída");
                MessageBox.Show(
                    GetText("The folder and its items were removed successfully!", "A pasta e seus itens foram removidos com sucesso!"),
                    GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadPak(pakPath);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetText("Error removing", "Erro ao remover");
                MessageBox.Show($"{GetText("Failure:", "Falha na remoção:")} {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideProgress();
                tvFolders.Enabled = true;
            }
        }

        /// <summary>Aplica o texto de pesquisa por cima do escopo de pasta atual e repopula a ListView.</summary>
        private void ApplyDisplayFilter()
        {
            string term = txtSearch.Text.Trim();

            IEnumerable<PakFileEntry> filtered = string.IsNullOrEmpty(term)
                ? _scopedEntries
                : _scopedEntries.Where(en => en.Name.Contains(term, StringComparison.OrdinalIgnoreCase));

            PopulateList(filtered);
        }

        private List<PakFileEntry> GetEntriesForFolder(string folderPath)
        {
            if (_currentReader == null) return new List<PakFileEntry>();

            // Padroniza as barras para a filtragem interna
            string cleanPath = folderPath.Replace('\\', '/').Trim('/');

            // Se estiver na raiz ("Todos os Arquivos" ou string vazia), retorna TUDO do PAK
            if (string.IsNullOrWhiteSpace(cleanPath) || cleanPath.Equals(GetText("All Files", "Todos os Arquivos"), StringComparison.OrdinalIgnoreCase))
            {
                return _currentReader.Entries
                    .Where(en => en.Type != PakFileEntryType.Directory)
                    .ToList();
            }

            // Se for uma pasta específica, garante o caractere '/' no final (Ex: "data/round20_abbot/")
            string prefix = cleanPath + "/";

            // Retorna apenas as entradas que começam com o caminho daquela pasta
            return [.. _currentReader.Entries
                .Where(en => en.Type != PakFileEntryType.Directory &&
                             en.Name.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))];
        }

        /// <summary>Preenche a ListView com o conjunto de entries fornecido (sem tocar no escopo/pesquisa).</summary>
        private void PopulateList(IEnumerable<PakFileEntry> entries)
        {
            lstEntries.BeginUpdate();
            lstEntries.Items.Clear();
            foreach (var entry in entries)
            {
                // CORREÇÃO: Padroniza as barras e extrai apenas o nome do arquivo puro
                string cleanName = entry.Name.Replace('/', '\\');
                string displayName = Path.GetFileName(cleanName);

                // Caso seja uma pasta pura (se o seu PAK listar diretórios como entradas vazias)
                if (string.IsNullOrEmpty(displayName) && entry.Type == PakFileEntryType.Directory)
                {
                    displayName = Path.GetFileName(cleanName.TrimEnd('\\'));
                }

                var item = new ListViewItem(displayName); // Exibe apenas "data.iff"
                item.SubItems.Add(entry.Type.ToString());
                item.SubItems.Add($"0x{entry.Size:X8}");
                item.SubItems.Add($"0x{entry.CompressSize:X8}");

                item.Tag = entry; // O Tag continua guardando o objeto completo intacto (com o Name original do PAK)
                if (entry.Type == PakFileEntryType.Directory)
                    item.ForeColor = Color.DarkCyan; // Pastas mantêm o tom ciano escuro
                else if (entry.Type == PakFileEntryType.LZ77)
                    item.ForeColor = Color.ForestGreen; // Arquivos compactados em LZ77 (Verde)
                else if (entry.Type == PakFileEntryType.Raw)
                    item.ForeColor = Color.DimGray; // Arquivos sem compressão / Brutos (Cinza discreto)
                else if (entry.Type == PakFileEntryType.LZ772)
                    item.ForeColor = Color.ForestGreen;  // Arquivos compactados do jogo em Verde

                lstEntries.Items.Add(item);
            }

            lstEntries.EndUpdate();
        }

        /// <summary>Duplo clique numa pasta dentro da lista navega para ela na árvore.</summary>
        private void LstEntries_DoubleClick(object? sender, EventArgs e)
        {
            if (lstEntries.SelectedItems.Count == 0) return;
            if (lstEntries.SelectedItems[0].Tag is not PakFileEntry entry) return;
            if (entry.Type != PakFileEntryType.Directory) return;

            string folderPath = entry.Name.Replace('/', '\\');
            if (_folderNodes.TryGetValue(folderPath, out var node))
                tvFolders.SelectedNode = node;
        }

        // ─── PROGRESSO / STATUS ─────────────────────────────────────────────────

        /// <summary>Helper thread-safe para reportar progresso (0-100%) na status bar a partir de uma Task de fundo.</summary>
        private void ReportProgress(int done, int total, string? prefix = null)
        {
            void Apply()
            {
                progressBar1.Visible = true;
                progressBar1.Maximum = 100;
                progressBar1.Value = total > 0 ? Math.Clamp((done * 100) / total, 0, 100) : 0;
                if (prefix != null)
                    lblStatus.Text = $"{prefix} ({done}/{total})";
            }

            if (InvokeRequired) Invoke(Apply);
            else Apply();
        }

        private void HideProgress()
        {
            void Apply() => progressBar1.Visible = false;
            if (InvokeRequired) Invoke(Apply);
            else Apply();
        }

        // ─── EXTRAIR TUDO / SELECIONADO(S) / LOTE ──────────────────────────────

        private async void btnExtractAll_Click(object sender, EventArgs e)
        {
            if (_currentReader == null)
            {
                MessageBox.Show(GetText("Please load a .pak file first.", "Por favor, carregue um arquivo .pak primeiro."), GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string destination = folderDialog.SelectedPath;
                btnExtractAll.Enabled = false;
                lblStatus.Text = GetText("Extracting files...", "Extraindo arquivos...");

                try
                {
                    await Task.Run(() =>
                    {
                        _currentReader.Extract("*", destination, msg => { },
                            (done, total) => ReportProgress(done, total, GetText("Extracting", "Extraindo")));
                    });

                    lblStatus.Text = GetText("Ready", "Pronto");
                    MessageBox.Show(GetText("All files were extracted successfully!", "Todos os arquivos foram extraídos com sucesso!"), GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    lblStatus.Text = GetText("Extraction error", "Erro na extração");
                    MessageBox.Show($"{GetText("Error extracting:", "Erro ao extrair:")} {ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    HideProgress();
                    btnExtractAll.Enabled = true;
                }
            }
        }

        private async void btnExtractSelected_Click(object sender, EventArgs e) => await ExtractSelectedAsync();

        /// <summary>
        /// Extrai apenas os itens selecionados na ListView, usando o caminho rápido
        /// (PakReader.ExtractEntry), que reaproveita o stream já aberto — sem reabrir
        /// e reanalisar o .pak inteiro como acontecia antes.
        /// </summary>
        private async Task ExtractSelectedAsync()
        {
            if (_currentReader == null) return;

            // Se houver itens na lista da direita, prioriza a extração de arquivos individuais
            if (lstEntries.SelectedItems.Count > 0)
            {
                await ExtractOnlySelectedFilesAsync();
            }
            // Se não houver nada na lista, mas houver uma pasta na árvore, extrai a pasta
            else if (tvFolders.SelectedNode != null)
            {
                await ExtractSelectedFolderAsync();
            }
            else
            {
                MessageBox.Show(GetText("Select files from the list or a folder from the treeview to extract.", "Selecione arquivos na lista ou uma pasta na árvore lateral para extrair."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Opção 1: Extrai estritamente a pasta selecionada na TreeView (e suas subpastas)
        /// </summary>
        private async Task ExtractSelectedFolderAsync()
        {
            if (_currentReader == null || tvFolders.SelectedNode == null) return;

            string rawPath = tvFolders.SelectedNode.FullPath;

            // Limpa os emojis e espaços extras
            string cleanPath = rawPath
                .Replace("🗂 ", "").Replace("🗂", "")
                .Replace("📁 ", "").Replace("📁", "")
                .Replace('\\', '/');

            if (cleanPath.StartsWith("Todos os Arquivos/", StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = cleanPath.Substring("Todos os Arquivos/".Length);
            }
            else if (cleanPath.Equals(GetText("All Files", "Todos os Arquivos"), StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = "";
            }

            List<PakFileEntry> entriesToExtract;
            string dialogTitle;
            string rootToStrip = ""; // Guardará o que precisamos remover do caminho do arquivo

            if (string.IsNullOrWhiteSpace(cleanPath))
            {
                entriesToExtract = _currentReader.Entries
                    .Where(en => en.Type != PakFileEntryType.Directory)
                    .ToList();
                dialogTitle = GetText("Entire PAK", "Todo o PAK");
            }
            else
            {
                string prefix = cleanPath.Trim('/') + "/";

                // Guardamos o caminho da pasta pai para remover da estrutura final
                // Se selecionou 'data/round20_abbot/ase', queremos manter apenas o que está de 'ase' para frente
                int lastSlash = cleanPath.LastIndexOf('/');
                if (lastSlash >= 0)
                {
                    rootToStrip = cleanPath.Substring(0, lastSlash + 1); // Ex: "data/round20_abbot/"
                }

                entriesToExtract = _currentReader.Entries
                    .Where(en => en.Type != PakFileEntryType.Directory &&
                                 en.Name.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                dialogTitle = $"{GetText("Folder:", "Pasta:")} {tvFolders.SelectedNode.Text.Replace("📁 ", "")}";
            }

            if (entriesToExtract.Count == 0)
            {
                MessageBox.Show($"{GetText("No file found for path:", "Nenhum arquivo encontrado para o caminho:")} {cleanPath}", GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var folderDialog = new FolderBrowserDialog
            {
                Description = $"{GetText("Select the destination to extract", "Selecione o destino para extrair a")} {dialogTitle}"
            };
            if (folderDialog.ShowDialog() != DialogResult.OK) return;

            // Passamos o 'rootToStrip' para o método que grava os arquivos
            await RunExtractionWithStripAsync(entriesToExtract, folderDialog.SelectedPath, rootToStrip);
        }

        private async Task RunExtractionWithStripAsync(List<PakFileEntry> entries, string destinationDir, string rootToStrip)
        {
            btnExtractSelected.Enabled = false;
            lblStatus.Text = GetText("Extracting selected item(s)...", "Extraindo selecionado(s)...");

            try
            {
                await Task.Run(() =>
                {
                    int total = entries.Count;
                    int done = 0;

                    foreach (var entry in entries)
                    {
                        // Padroniza o nome do arquivo interno
                        string relativePath = entry.Name.Replace('\\', '/');

                        // Se o arquivo começar com a árvore de pastas que queremos cortar, removemos ela
                        if (!string.IsNullOrEmpty(rootToStrip) && relativePath.StartsWith(rootToStrip, StringComparison.OrdinalIgnoreCase))
                        {
                            relativePath = relativePath.Substring(rootToStrip.Length);
                        }

                        // Converte de volta para o padrão de barras do Windows (\)
                        string localRelativePath = relativePath.Replace('/', '\\');
                        string outPath = Path.Combine(destinationDir, localRelativePath);

                        // Garante que se houver subpastas internas a partir dali, elas sejam criadas
                        string? fileDir = Path.GetDirectoryName(outPath);
                        if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }

                        // Extrai o arquivo na sua nova posição simplificada
                        _currentReader!.ExtractEntry(entry, outPath);

                        done++;
                        ReportProgress(done, total, GetText("Extracting selected item(s)", "Extraindo selecionado(s)"));
                    }
                });

                lblStatus.Text = GetText("Ready", "Pronto");
                MessageBox.Show(GetText("Folder extracted respecting the selected level!", "Pasta extraída respeitando o nível selecionado!"), GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetText("Extraction error", "Erro na extração");
                MessageBox.Show($"{GetText("Error extracting:", "Erro ao extrair:")} {ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideProgress();
                btnExtractSelected.Enabled = true;
            }
        }

        /// <summary>
        /// Opção 2: Extrai estritamente os arquivos que estão marcados/selecionados na ListView
        /// </summary>
        private async Task ExtractOnlySelectedFilesAsync()
        {
            if (_currentReader == null || lstEntries.SelectedItems.Count == 0) return;

            var selectedEntries = lstEntries.SelectedItems
                .Cast<ListViewItem>()
                .Select(i => (PakFileEntry)i.Tag)
                .Where(en => en.Type != PakFileEntryType.Directory)
                .ToList();

            if (selectedEntries.Count == 0) return;

            string destinationDir;

            // Se for apenas um arquivo, permite escolher o nome exato do arquivo (SaveFileDialog)
            if (selectedEntries.Count == 1)
            {
                string suggestedName = Path.GetFileName(selectedEntries[0].Name.Replace('/', '\\'));
                using var saveFileDialog = new SaveFileDialog
                {
                    FileName = suggestedName,
                    Title = $"{GetText("Extract", "Extrair")} {suggestedName}"
                };
                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                destinationDir = Path.GetDirectoryName(saveFileDialog.FileName) ?? "./";
                await RunExtractionAsync(selectedEntries, destinationDir, saveFileDialog.FileName);
            }
            // Se forem vários arquivos da lista, pede a pasta de destino
            else
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = GetText("Select the destination folder for the selected files", "Selecione a pasta de destino para os arquivos selecionados")
                };
                if (folderDialog.ShowDialog() != DialogResult.OK) return;

                destinationDir = folderDialog.SelectedPath;
                await RunExtractionAsync(selectedEntries, destinationDir, null);
            }
        }

        private async Task RunExtractionAsync(List<PakFileEntry> entries, string destinationDir, string? exactPathForSingle)
        {
            btnExtractSelected.Enabled = false;
            lblStatus.Text = GetText("Extracting selected item(s)...", "Extraindo selecionado(s)...");

            try
            {
                await Task.Run(() =>
                {
                    int total = entries.Count;
                    int done = 0;

                    foreach (var entry in entries)
                    {
                        string outPath = exactPathForSingle ?? Path.Combine(destinationDir, entry.Name.Replace('/', '\\'));
                        _currentReader!.ExtractEntry(entry, outPath);

                        done++;
                        ReportProgress(done, total, GetText("Extracting selected item(s)", "Extraindo selecionado(s)"));
                    }
                });

                lblStatus.Text = GetText("Ready", "Pronto");
                MessageBox.Show(GetText("File(s) extracted successfully!", "Arquivo(s) extraído(s) com sucesso!"), GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetText("Extraction error", "Erro na extração");
                MessageBox.Show($"{GetText("Error extracting:", "Erro ao extrair:")} {ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideProgress();
                btnExtractSelected.Enabled = true;
            }
        }

        private async void btnBatchExtract_Click(object sender, EventArgs e)
        {
            using var sourceFolderDialog = new FolderBrowserDialog { Description = GetText("Select the folder that CONTAINS the .pak files", "Selecione a pasta que CONTÉM os arquivos .pak") };
            if (sourceFolderDialog.ShowDialog() != DialogResult.OK) return;

            string sourceDir = sourceFolderDialog.SelectedPath;
            string[] pakFiles = Directory.GetFiles(sourceDir, "*.pak", SearchOption.TopDirectoryOnly);

            if (pakFiles.Length == 0)
            {
                MessageBox.Show(GetText("No .pak files were found in the selected folder.", "Nenhum arquivo .pak foi encontrado na pasta selecionada."), GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var destFolderDialog = new FolderBrowserDialog { Description = GetText("Select the DESTINATION folder for extraction", "Selecione a pasta de DESTINO para a extração") };
            if (destFolderDialog.ShowDialog() != DialogResult.OK) return;

            string targetBaseDir = destFolderDialog.SelectedPath;

            // Mesma região/chave do PAK atualmente carregado (se houver), evita ficar perguntando por console.
            uint[]? sharedKeys = _currentReader?.LocationKeys;

            btnBatchExtract.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Maximum = pakFiles.Length;
            progressBar1.Value = 0;

            int paksProcessados = 0;

            foreach (var pakPath in pakFiles)
            {
                string pakName = Path.GetFileNameWithoutExtension(pakPath);
                string specificDestFolder = Path.Combine(targetBaseDir, pakName);

                lblStatus.Text = $"{GetText("Processing", "Processando")} ({paksProcessados + 1}/{pakFiles.Length}): {pakName}.pak...";

                try
                {
                    await Task.Run(() =>
                    {
                        if (!Directory.Exists(specificDestFolder))
                            Directory.CreateDirectory(specificDestFolder);

                        using var batchReader = new PakReader(pakPath);
                        batchReader.Parse(sharedKeys);
                        batchReader.Extract("*", specificDestFolder);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{GetText("Failed to extract", "Falha ao extrair")} {pakName}.pak:\n{ex.Message}", GetText("Batch Error", "Erro no Lote"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                paksProcessados++;
                progressBar1.Value = paksProcessados;
            }

            lblStatus.Text = GetText("Batch extraction completed!", "Extração em lote concluída!");
            progressBar1.Visible = false;
            btnBatchExtract.Enabled = true;

            MessageBox.Show($"{paksProcessados} {GetText("PAK packages extracted successfully to:", "pacotes PAK extraídos com sucesso em:")}\n{targetBaseDir}", GetText("Processing Complete", "Processamento Concluído"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ─── INJETAR / ATUALIZAR ────────────────────────────────────────────────

        private PakRebuildOptions BuildRebuildOptionsForCurrentPak()
        {
            if (_currentReader != null)
            {
                string novoAutor = txtUpdateAuthor.Text?.Trim();
                string autorAtual = _currentReader.Header.Author?.Trim();

                // O campo de texto está vazio ou apenas com espaços
                if (string.IsNullOrEmpty(novoAutor))
                {
                    // Só avisa se o autor atual já NÃO for "PakMaker" (evita avisos redundantes)
                    if (!string.Equals(autorAtual, "PakMaker", StringComparison.OrdinalIgnoreCase))
                    {
                        // Exibe a confirmação para redefinir para o padrão
                        var resultado = MessageBox.Show(
                            GetText(
                                "The Author field is empty. Do you want to reset the author name to 'PakMaker'?",
                                "O campo do Autor está vazio. Deseja redefinir o nome do autor para 'PakMaker'?"),
                            GetText("Confirm Author Reset", "Confirmar redefinição de Autor"),
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        // Se o utilizador confirmar, altera para o padrão
                        if (resultado == DialogResult.Yes)
                        {
                            _currentReader.Header.Author = "PakMaker";
                        }
                    }
                }
                // O utilizador digitou um novo nome
                else
                {
                    // Só atualiza se o texto digitado for DIFERENTE do autor que já lá está
                    if (!string.Equals(novoAutor, autorAtual, StringComparison.OrdinalIgnoreCase))
                    {
                        // Exibe a confirmação para atualizar para o novo nome digitado
                        var resultado = MessageBox.Show(
                            GetText(
                                $"Do you want to change the author name to '{novoAutor}'?",
                                $"Deseja alterar o nome do autor para '{novoAutor}'?"),
                            GetText("Confirm Author Change", "Confirmar alteração de Autor"),
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        // Se o utilizador confirmar, altera para o novo nome digitado
                        if (resultado == DialogResult.Yes)
                        {
                            _currentReader.Header.Author = novoAutor;
                        }
                        // Se o utilizador disser "Não", mantém o 'autorAtual' intacto e não faz nada
                    }
                }
            }

            // Obtém a região selecionada na ComboBox
            var selectedRegion = (dynamic)cboRegion.SelectedItem;

            // Retorna as opções preenchidas corretamente com o estado atual do Header.Author
            return new PakRebuildOptions(
                EntryVersion: (PakFileEntryVersion)cboVersion.SelectedItem,
                EntryType: (PakFileEntryType)cboCompressType.SelectedItem,
                CompressLevel: (byte)numCompressLevel.Value,
                LocationKeys: _currentReader?.LocationKeys ?? (uint[])selectedRegion.Keys,
                Author: _currentReader?.Header.Author
            );
        }


        private async void btnUpdatePak_Click(object sender, EventArgs e)
        {
            if (_currentReader == null || string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
            {
                MessageBox.Show(
                                    GetText("Select an active .pak file first.", "Selecione um arquivo .pak ativo primeiro."),
                                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var openFileDialog = new OpenFileDialog
            {
                Title = GetText("Select the files to update/inject", "Selecione os arquivos para atualizar/injetar"),
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            var items = openFileDialog.FileNames.Select(f => new PakInjectItem(f, null)).ToList();
            await InjectFilesIntoCurrentPakAsync(items);

        }

        /// <summary>
        /// Lógica comum de injeção/atualização do PAK atualmente carregado.
        /// Usada tanto pelo botão "Atualizar PAK" quanto pelo drag-and-drop na lista de entries.
        /// </summary>
        private async Task InjectFilesIntoCurrentPakAsync(List<PakInjectItem> items)
        {
            if (_currentReader == null || string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
            {
                MessageBox.Show(
                    GetText("Select an active .pak file first.", "Selecione um arquivo .pak ativo primeiro."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (items == null || items.Count == 0) return;

            string pakPath = txtPakPath.Text;
            var reader = _currentReader;

            lblStatus.Text = GetText("Merging and rebuilding PAK...", "Mesclando e reconstruindo PAK...");
            btnUpdatePak.Enabled = false;

            try
            {
                var options = BuildRebuildOptionsForCurrentPak();

                await Task.Run(() =>
                {
                    PakManager.InjectFiles(pakPath, reader, items, options,
                        log: msg => { },
                        onProgress: (done, total) => ReportProgress(done, total, GetText("Rebuilding PAK", "Reconstruindo PAK")));
                });

                lblStatus.Text = GetText("PAK updated successfully!", "PAK atualizado com sucesso!");
                MessageBox.Show(
                      $"{items.Count} {GetText("file(s) injected and the PAK was rebuilt!", "arquivo(s) injetado(s) e o PAK foi reconstruído!")}",
                      GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPak(pakPath);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetText("Error injecting", "Erro ao injetar");
                MessageBox.Show($"{GetText("Injection failed:", "Injeção falhou:")} {ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideProgress();
                btnUpdatePak.Enabled = true;
            }
        }

        private void LstEntries_DragEnter(object? sender, DragEventArgs e)
        {
            if (_currentReader != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private async void LstEntries_DragDrop(object? sender, DragEventArgs e)
        {
            if (_currentReader == null || e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] dropped = (string[])e.Data.GetData(DataFormats.FileDrop)!;

            var items = new List<PakInjectItem>();
            foreach (var path in dropped)
            {
                if (File.Exists(path))
                {
                    // Arquivo solto: sem pasta explícita, cai no fallback de busca por nome existente
                    items.Add(new PakInjectItem(path, null));
                }
                else if (Directory.Exists(path))
                {
                    try
                    {
                        string baseFolder = path; // a própria pasta arrastada é a "raiz" da estrutura relativa
                        foreach (var filePath in Directory.GetFiles(baseFolder, "*", SearchOption.AllDirectories))
                        {
                            string relativeToBase = Path.GetRelativePath(baseFolder, filePath);
                            string relFolder = Path.GetDirectoryName(relativeToBase) ?? "";

                            items.Add(new PakInjectItem(filePath, relFolder));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{GetText("Error reading folder", "Erro ao ler a pasta")} '{path}':\n{ex.Message}",
                            GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            if (items.Count == 0)
            {
                MessageBox.Show(GetText("No valid file was found to inject.", "Nenhum arquivo válido encontrado para injetar."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await InjectFilesIntoCurrentPakAsync(items);
        }

        // ─── REMOVER ─────────────────────────────────────────────────────────────

        private async void btnRemoveSelected_Click(object sender, EventArgs e) => await RemoveSelectedAsync();

        private async Task RemoveSelectedAsync()
        {
            if (_currentReader == null || string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
            {
                MessageBox.Show(
                    GetText("Select an active .pak file first.", "Selecione um arquivo .pak ativo primeiro."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HashSet<string> namesToRemove = new(StringComparer.OrdinalIgnoreCase);
            List<string> folderNamesForDialog = new();

            // --- 1. COLETAR ARQUIVOS SELECIONADOS NO LISTVIEW (MULTIPLE SELECTION) ---
            if (lstEntries.SelectedItems.Count > 0)
            {
                var selectedFiles = lstEntries.SelectedItems
                    .Cast<ListViewItem>()
                    .Select(i => (PakFileEntry)i.Tag)
                    .Where(en => en.Type != PakFileEntryType.Directory)
                    .Select(en => en.Name);

                foreach (var file in selectedFiles)
                {
                    namesToRemove.Add(file);
                }
            }

            // --- 2. COLETAR TODAS AS PASTAS CHECADAS NA TREEVIEW (MULTIPLE SELECTION) ---
            // Função local recursiva para varrer a árvore inteira atrás de nós checados
            void ColetarPastasChecadas(TreeNodeCollection nodes)
            {
                foreach (TreeNode node in nodes)
                {
                    if (node.Checked)
                    {
                        string rawTreePath = node.FullPath;
                        string virtualTargetFolder = rawTreePath
                            .Replace("🗂 ", "").Replace("🗂", "")
                            .Replace("📁 ", "").Replace("📁", "")
                            .Replace('\\', '/');

                        if (virtualTargetFolder.StartsWith("Todos os Arquivos/", StringComparison.OrdinalIgnoreCase))
                            virtualTargetFolder = virtualTargetFolder.Substring("Todos os Arquivos/".Length);
                        else if (virtualTargetFolder.StartsWith("All Files/", StringComparison.OrdinalIgnoreCase))
                            virtualTargetFolder = virtualTargetFolder.Substring("All Files/".Length);
                        else if (virtualTargetFolder.Equals(GetText("All Files", "Todos os Arquivos"), StringComparison.OrdinalIgnoreCase))
                            virtualTargetFolder = "";

                        // Ignora se for a raiz total, para evitar deletar o PAK inteiro sem querer
                        if (!string.IsNullOrEmpty(virtualTargetFolder))
                        {
                            string prefix = virtualTargetFolder.Trim('/') + "/";

                            var folderFiles = _currentReader.Entries
                                .Where(en => en.Type != PakFileEntryType.Directory &&
                                             en.Name.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                .Select(en => en.Name);

                            foreach (var file in folderFiles)
                            {
                                namesToRemove.Add(file);
                            }

                            string folderName = node.Text.Replace("📁 ", "").Replace("🗂 ", "");
                            folderNamesForDialog.Add(folderName);
                        }
                    }

                    // Continua buscando nas subpastas deste nó
                    if (node.Nodes.Count > 0)
                    {
                        ColetarPastasChecadas(node.Nodes);
                    }
                }
            }

            // Inicia a varredura a partir da raiz da TreeView
            ColetarPastasChecadas(tvFolders.Nodes);

            // Se nenhum arquivo e nenhuma pasta foram marcados
            if (namesToRemove.Count == 0)
            {
                MessageBox.Show(
                    GetText("Select files from the list or check folder boxes from the treeview to remove.", "Selecione arquivos na lista ou marque as caixas das pastas na árvore lateral para remover."),
                    GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- 3. MENSAGEM DE CONFIRMAÇÃO ---
            string confirmationMessage;
            if (folderNamesForDialog.Count > 0)
            {
                confirmationMessage = GetText(
                    $"You are about to remove the checked folder(s): '{string.Join(", ", folderNamesForDialog)}' and/or selected files.\nTotal files to delete: {namesToRemove.Count}.\n\nDo you want to continue?",
                    $"Você está prestes a remover a(s) pasta(s) marcada(s): '{string.Join(", ", folderNamesForDialog)}' e/ou os arquivos selecionados.\nTotal de arquivos a serem apagados: {namesToRemove.Count}.\n\nDeseja continuar?");
            }
            else
            {
                confirmationMessage = GetText(
                    $"Remove {namesToRemove.Count} selected file(s) from PAK?",
                    $"Remover {namesToRemove.Count} arquivo(s) selecionado(s) do PAK?");
            }

            var confirm = MessageBox.Show(
                $"{confirmationMessage}\n\n{GetText("The PAK will be rebuilt and a backup (.bak) will be created automatically.", "O PAK será reconstruído e um backup (.bak) será criado automaticamente.")}",
                GetText("Confirm Removal", "Confirmar remoção"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            string pakPath = txtPakPath.Text;
            var reader = _currentReader;

            lblStatus.Text = GetText("Removing and rebuilding PAK...", "Removendo e reconstruindo PAK...");
            btnRemoveSelected.Enabled = false;

            try
            {
                var options = BuildRebuildOptionsForCurrentPak();

                await Task.Run(() =>
                {
                    PakManager.RemoveFiles(pakPath, reader, namesToRemove.ToList(), options,
                        log: msg => { },
                        onProgress: (done, total) => ReportProgress(done, total, GetText("Rebuilding PAK", "Reconstruindo PAK")));
                });

                lblStatus.Text = GetText("Removal completed", "Remoção concluída");
                MessageBox.Show(
                    GetText("The selected items were removed successfully and the PAK file was rebuilt!", "Os itens selecionados foram removidos com sucesso e o arquivo PAK foi reconstruído!"),
                    GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadPak(pakPath);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetText("Error removing", "Erro ao remover");
                MessageBox.Show($"{GetText("Failure:", "Falha na remoção:")} {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideProgress();
                btnRemoveSelected.Enabled = true;
            }
        }

        // ─── ABA 2: CRIAÇÃO DE PAK ─────────────────────────────────────────────
        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtSourceFolder.Text = folderDialog.SelectedPath;
            }
        }

        private async void btnCreatePak_Click(object sender, EventArgs e)
        {
            string source = txtSourceFolder.Text;
            if (string.IsNullOrEmpty(source) || !Directory.Exists(source))
            {
                MessageBox.Show(
                          GetText("Select a valid source directory.", "Selecione um diretório de origem válido."),
                          GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveFileDialog = new SaveFileDialog { Filter = "PangYa PAK|*.pak", Title = GetText("Save New PAK", "Salvar Novo PAK") };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var selectedItem = cboRegion.SelectedItem as dynamic;
                    if (selectedItem != null)
                    {
                        btnCreatePak.Enabled = false;
                        lblStatus.Text = GetText("Compiling PAK...", "Compilando PAK...");
                        uint[] selectedKeys = selectedItem.Keys;//name is keys, label is name key 
                        var selectedVersion = (PakFileEntryVersion)cboVersion.SelectedItem;
                        //minha tecnica antiga para criar paks raw
                        if (selectedVersion == PakFileEntryVersion.Raw)
                        {
                            selectedKeys = Array.Empty<uint>(); // Ou mantenha null se o Writer aceitar
                        }

                        //tambem tem a versao raw ou universal key, que nao inserimos chave, pois ela se trata de dados brutos diferentes
                        var writer = new PakWriter
                        {
                            EntryVersion = selectedVersion,
                            EntryType = (PakFileEntryType)cboCompressType.SelectedItem,
                            CompressLevel = (byte)numCompressLevel.Value,
                            // Se não for Raw e selectedKeys vier nulo por falha de seleção, aplica o fallback JP
                            LocationKeys = selectedKeys ?? (selectedVersion == PakFileEntryVersion.Raw ? Array.Empty<uint>() : PakKeys.JP),
                            Author = txtNewAuthorPak.Text // Assinatura do PAK
                        };
                        //inicia a criacao do pak
                        await Task.Run(() => writer.CreateFromDirectory(source, saveFileDialog.FileName));
                        //terminou
                        lblStatus.Text = GetText("Ready", "Pronto");
                        btnCreatePak.Enabled = true;
                        MessageBox.Show(
                                            GetText(".pak file generated successfully!", "Arquivo .pak gerado com sucesso!"),
                                            GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(GetText("Please select a valid region before continuing.", "Por favor, selecione uma região válida antes de continuar."), GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = GetText("Error creating PAK", "Erro ao criar PAK");
                    btnCreatePak.Enabled = true;
                    MessageBox.Show($"{GetText("Compilation error:", "Erro de compilação:")} {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnChangeKey_Click(object sender, EventArgs e)
        {
            if (_currentReader == null || string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
            {
                MessageBox.Show(GetText("Select an active .pak file first.", "Selecione um arquivo .pak ativo primeiro."), GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRegion = cboNewRegion.SelectedItem as dynamic;
            if (selectedRegion == null)
            {
                MessageBox.Show(GetText("Select the target region/key.", "Selecione a região/chave de destino."), GetText("Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            uint[] newKeys = selectedRegion.Keys;

            // Evita reconstrução desnecessária se a chave de destino já for a mesma do PAK carregado
            if (_currentReader.LocationKeys != null && newKeys.SequenceEqual(_currentReader.LocationKeys))
            {
                MessageBox.Show(GetText("The PAK is already using that key.", "O PAK já está usando essa chave."), GetText("Information", "Informação"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"{GetText("Change the PAK key to", "Trocar a chave do PAK para")} \"{selectedRegion.Label}\"?\n{GetText("The PAK will be rebuilt and a backup (.bak) will be created.", "O PAK será reconstruído e um backup (.bak) será criado.")}",
                GetText("Confirm key change", "Confirmar troca de chave"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            string pakPath = txtPakPath.Text;
            var reader = _currentReader;

            // Mantém versão/compressão/autor atuais do PAK, só troca a LocationKeys
            var currentOptions = BuildRebuildOptionsForCurrentPak();
            var newOptions = currentOptions with { LocationKeys = newKeys };

            lblStatus.Text = GetText("Changing key and rebuilding PAK...", "Trocando chave e reconstruindo PAK...");
            btnChangeKey.Enabled = false;

            try
            {
                await Task.Run(() =>
                {
                    PakManager.ChangeEncryptionKey(pakPath, reader, newOptions,
                        log: msg => { },
                        onProgress: (done, total) => ReportProgress(done, total, GetText("Rebuilding PAK", "Reconstruindo PAK")));
                });

                lblStatus.Text = GetText("Key changed successfully!", "Chave trocada com sucesso!");
                MessageBox.Show($"{GetText("The PAK was rebuilt with the key from", "O PAK foi reconstruído com a chave de")} \"{selectedRegion.Label}\"!",
                    GetText("Success", "Sucesso"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadPak(pakPath);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetText("Error changing key", "Erro ao trocar chave");
                MessageBox.Show($"{GetText("Rebuild failed:", "Falha na reconstrução:")} {ex.Message}", GetText("Error", "Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideProgress();
                btnChangeKey.Enabled = true;
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
