using PangyaAPI.PAK.Flags;
using PangyaAPI.PAK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PangYa_Suite_Tools
{
    public partial class FrmPakMaker : Form
    {
        private PakReader? _currentReader;

        public FrmPakMaker()
        {
            InitializeComponent();
            SetupCustomComponents();
            LoadSetupOptions();
            SetupContextMenu(); // Inicializa o menu de contexto da ListView
        }

        private void SetupCustomComponents()
        {
            // Ativa o Drag-and-Drop no formulário principal e nas caixas de texto
            this.AllowDrop = true;
            this.DragEnter += FrmPakMaker_DragEnter;
            this.DragLeave += FrmPakMaker_DragLeave;
            this.DragDrop += FrmPakMaker_DragDrop;
        }

        private void SetupContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuExtractSingle = new ToolStripMenuItem("📁 Extrair apenas este arquivo...");

            menuExtractSingle.Click += async (s, e) =>
            {
                if (lstEntries.SelectedItems.Count == 0 || _currentReader == null) return;

                // Recupera a instância da Entry guardada no Tag do item selecionado
                var selectedEntry = (PakFileEntry)lstEntries.SelectedItems[0].Tag;
                string internalName = selectedEntry.Name;

                // Proteção preventiva local:
                if (internalName.Contains('\0'))
                {
                    internalName = internalName.Split('\0')[0];
                }
                internalName = internalName.Replace('/', '\\').Trim();
                using var saveFileDialog = new SaveFileDialog
                {
                    FileName = Path.GetFileName(internalName),
                    Title = $"Extrair {internalName}"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    btnExtractAll.Enabled = false;
                    lblStatus.Text = $"Extraindo {internalName}...";

                    try
                    {
                        await Task.Run(() =>
                        {
                            string destinationDir = Path.GetDirectoryName(saveFileDialog.FileName) ?? "./";

                            // Instancia um leitor rápido para a extração direta
                            using var singleReader = new PakReader(txtPakPath.Text);
                            singleReader.Parse(_currentReader.LocationKeys);
                            singleReader.Extract(internalName, destinationDir);

                            // Ajusta o nome do arquivo se o usuário renomeou no SaveFileDialog
                            string extractedDefaultPath = Path.Combine(destinationDir, internalName);
                            if (extractedDefaultPath != saveFileDialog.FileName && File.Exists(extractedDefaultPath))
                            {
                                if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);
                                File.Move(extractedDefaultPath, saveFileDialog.FileName);
                            }
                        });

                        lblStatus.Text = "Pronto";
                        MessageBox.Show("Arquivo extraído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Erro na extração";
                        MessageBox.Show($"Erro ao extrair: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnExtractAll.Enabled = true;
                    }
                }
            };

            contextMenu.Items.Add(menuExtractSingle);
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

        private void FrmPakMaker_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string path = files[0];
                txtPakPath.BackColor = SystemColors.Control;

                // Se for um arquivo .pak, carrega no leitor
                if (File.Exists(path) && path.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                {
                    txtPakPath.Text = path;
                    tabControl1.SelectedIndex = 0; // Foca na aba de extração
                    LoadPak(path);
                }
                // Se for uma pasta, joga para a aba de criação
                else if (Directory.Exists(path))
                {
                    txtSourceFolder.Text = path;
                    tabControl1.SelectedIndex = 1; // Foca na aba de criação
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
                _currentReader?.Dispose();
                _currentReader = new PakReader(path);
                _currentReader.Parse();

                // Atualiza as Labels de informação do Header
                lblAuthor.Text = $"Autor: {_currentReader.Header.Author}";
                lblVersion.Text = $"Versão: 0x{_currentReader.Header.Version:X2}";
                lblEntries.Text = $"Entradas: {_currentReader.Header.NumFileEntry}";

                // Limpa e popula a ListView de arquivos internos
                lstEntries.Items.Clear();
                lstEntries.BeginUpdate();

                foreach (var entry in _currentReader.Entries)
                {
                    var item = new ListViewItem(entry.Name);
                    item.SubItems.Add(entry.Type.ToString());
                    item.SubItems.Add($"0x{entry.Size:X8}");
                    item.SubItems.Add($"0x{entry.CompressSize:X8}");

                    item.Tag = entry;

                    // Diferencia visualmente diretórios de arquivos
                    if (entry.Type == PakFileEntryType.Directory)
                    {
                        item.ForeColor = Color.DarkCyan;
                    }

                    lstEntries.Items.Add(item);
                }

                lstEntries.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o arquivo PAK:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnExtractAll_Click(object sender, EventArgs e)
        {
            if (_currentReader == null)
            {
                MessageBox.Show("Por favor, carregue um arquivo .pak primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string destination = folderDialog.SelectedPath;
                btnExtractAll.Enabled = false;
                lblStatus.Text = "Extraindo arquivos...";

                await Task.Run(() =>
                {
                    _currentReader.Extract("*", destination, msg => { });
                });

                lblStatus.Text = "Pronto";
                btnExtractAll.Enabled = true;
                MessageBox.Show("Todos os arquivos foram extraídos com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnUpdatePak_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPakPath.Text) || !File.Exists(txtPakPath.Text))
            {
                MessageBox.Show("Selecione um arquivo .pak ativo primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var openFileDialog = new OpenFileDialog
            {
                Title = "Selecione os arquivos para atualizar/injetar",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            string pakPath = txtPakPath.Text;
            string[] filesToInject = openFileDialog.FileNames;

            lblStatus.Text = "Mesclando e reconstruindo PAK...";
            btnUpdatePak.Enabled = false;

            try
            {
                await Task.Run(() =>
                {
                    // 1. Criamos uma pasta temporária para armazenar a extração rápida
                    string tempDir = Path.Combine(Path.GetTempPath(), "PakTemp_" + Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDir);

                    // 2. Extrai o PAK completo atual para a pasta temporária
                    uint[]? currentKeys = _currentReader?.LocationKeys;
                    using (var tempReader = new PakReader(pakPath))
                    {
                        tempReader.Parse(currentKeys);
                        tempReader.Extract("*", tempDir);
                    }

                    // 3. Copia/Substitui os novos arquivos por cima da pasta temporária
                    foreach (var newFile in filesToInject)
                    {
                        string fileName = Path.GetFileName(newFile);
                        string destPath = Path.Combine(tempDir, fileName);
                        File.Copy(newFile, destPath, true);
                    }

                    // 4. Cria o backup de segurança do PAK atual
                    string backupPak = pakPath + ".bak";
                    if (File.Exists(backupPak)) File.Delete(backupPak);
                    File.Move(pakPath, backupPak);

                    // 5. Instancia o construtor padrão (PakWriter não possui Dispose, portanto sem 'using')
                    var selectedRegion = (dynamic)cboRegion.SelectedItem;
                    var writer = new PakWriter
                    {
                        EntryVersion = (PakFileEntryVersion)cboVersion.SelectedItem,
                        EntryType = (PakFileEntryType)cboCompressType.SelectedItem,
                        CompressLevel = (byte)numCompressLevel.Value,
                        LocationKeys = selectedRegion.Keys,
                        Author = _currentReader?.Header.Author ?? "PangYaSuiteTools"
                    };

                    // Recompila a estrutura atualizada para o local original do PAK
                    writer.CreateFromDirectory(tempDir, pakPath);

                    // 6. Limpa os rastros temporários
                    Directory.Delete(tempDir, true);
                });

                lblStatus.Text = "PAK atualizado com sucesso!";
                MessageBox.Show("O arquivo PAK foi reconstruído e atualizado!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Recarrega os novos dados na interface
                LoadPak(pakPath);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro ao atualizar";
                MessageBox.Show($"Falha na reconstrução: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpdatePak.Enabled = true;
            }
        }

        private async void btnBatchExtract_Click(object sender, EventArgs e)
        {
            using var sourceFolderDialog = new FolderBrowserDialog { Description = "Selecione a pasta que CONTÉM os arquivos .pak" };
            if (sourceFolderDialog.ShowDialog() != DialogResult.OK) return;

            string sourceDir = sourceFolderDialog.SelectedPath;
            string[] pakFiles = Directory.GetFiles(sourceDir, "*.pak", SearchOption.TopDirectoryOnly);

            if (pakFiles.Length == 0)
            {
                MessageBox.Show("Nenhum arquivo .pak foi encontrado na pasta selecionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var destFolderDialog = new FolderBrowserDialog { Description = "Selecione a pasta de DESTINO para a extração" };
            if (destFolderDialog.ShowDialog() != DialogResult.OK) return;

            string targetBaseDir = destFolderDialog.SelectedPath;

            btnBatchExtract.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Maximum = pakFiles.Length;
            progressBar1.Value = 0;

            int paksProcessados = 0;

            foreach (var pakPath in pakFiles)
            {
                string pakName = Path.GetFileNameWithoutExtension(pakPath);
                string specificDestFolder = Path.Combine(targetBaseDir, pakName);

                lblStatus.Text = $"Processando ({paksProcessados + 1}/{pakFiles.Length}): {pakName}.pak...";

                try
                {
                    await Task.Run(() =>
                    {
                        if (!Directory.Exists(specificDestFolder))
                            Directory.CreateDirectory(specificDestFolder);

                        using var batchReader = new PakReader(pakPath);
                        batchReader.Parse();
                        batchReader.Extract("*", specificDestFolder);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falha ao extrair {pakName}.pak:\n{ex.Message}", "Erro no Lote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                paksProcessados++;
                progressBar1.Value = paksProcessados;
            }

            lblStatus.Text = "Extração em lote concluída!";
            progressBar1.Visible = false;
            btnBatchExtract.Enabled = true;

            MessageBox.Show($"{paksProcessados} pacotes PAK extraídos com sucesso em:\n{targetBaseDir}", "Processamento Concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Selecione um diretório de origem válido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveFileDialog = new SaveFileDialog { Filter = "Pangya PAK Files (*.pak)|*.pak", FileName = "ProjectGxxx.pak" };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var selectedItem = cboRegion.SelectedItem as dynamic;
                    if (selectedItem != null)
                    {
                        btnCreatePak.Enabled = false;
                        lblStatus.Text = "Compilando PAK...";
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
                            Author = "PakToolWinForms" // Assinatura do PAK
                        };
                        //inicia a criacao do pak
                        await Task.Run(() => writer.CreateFromDirectory(source, saveFileDialog.FileName));
                        //terminou
                        lblStatus.Text = "Pronto";
                        btnCreatePak.Enabled = true;
                        MessageBox.Show("Arquivo .pak gerado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Por favor, selecione uma região válida antes de continuar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    } 
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Erro";
                    btnCreatePak.Enabled = true;
                    MessageBox.Show($"Erro ao criar o pacote:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}