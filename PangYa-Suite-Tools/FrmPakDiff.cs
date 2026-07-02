using PangyaAPI.PAK.Flags;
using PangyaAPI.PAK.Models;
namespace PangYa_Suite_Tools
{
    public partial class FrmPakDiff : Form
    {
        private string _currentLanguage;
        private List<FileDiffEntry> _comparisonResults = new();

        public FrmPakDiff(string currentLanguage)
        {
            _currentLanguage = currentLanguage;
            InitializeComponent();
            // Aqui você pode inicializar ComboBoxes de modo (Diferentes / Iguais), etc.
        }

        /// <summary>
        /// Realiza a comparação em lote (Multi-PAKs) comparando arquivos de mesma estrutura relativa.
        /// </summary>
        /// <param name="compareMode">"diff" para extrair diferenças, "equal" para extrair o que for igual</param>
        private async Task PerformMultiPakComparisonAsync(string compareMode)
        {
            string sourceDir = txtSourceClient.Text; // Cliente Base / Alvo de onde queremos extrair
            string compareDir = txtCompareClient.Text; // Nosso Cliente para comparação

            if (!Directory.Exists(sourceDir) || !Directory.Exists(compareDir))
            {
                MessageBox.Show(GetText("Select valid directories for both clients.", "Selecione diretórios válidos para ambos os clientes."),
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnCompare.Enabled = false;
            lstDiffFiles.Items.Clear();
            _comparisonResults.Clear();

            try
            {
                await Task.Run(() =>
                {
                    // 1. Localiza todos os arquivos .pak no Cliente de Origem (Source)
                    var sourcePaks = Directory.GetFiles(sourceDir, "*.pak", SearchOption.AllDirectories);

                    foreach (var sourcePakPath in sourcePaks)
                    {
                        // Obtém o caminho relativo do PAK (ex: "data\projectG.pak")
                        string relativePakPath = Path.GetRelativePath(sourceDir, sourcePakPath);

                        // Procura o correspondente exato no outro cliente
                        string matchingComparePakPath = Path.Combine(compareDir, relativePakPath);

                        // Dicionário para mapear os arquivos internos do nosso cliente correspondente
                        var compareEntriesMap = new Dictionary<string, PakFileEntry>(StringComparer.OrdinalIgnoreCase);

                        if (File.Exists(matchingComparePakPath))
                        {
                            try
                            {
                                using (var compReader = new PakReader(matchingComparePakPath))
                                {
                                    foreach (var entry in compReader.Entries)
                                    {
                                        compareEntriesMap[entry.Name] = entry;
                                    }
                                }
                            }
                            catch { /* Ignora se o PAK do segundo cliente estiver corrompido */ }
                        }

                        // Lendo o PAK do cliente de Origem (de onde queremos retirar as coisas)
                        try
                        {
                            using (var srcReader = new PakReader(sourcePakPath))
                            {
                                foreach (var srcEntry in srcReader.Entries)
                                {
                                    if (srcEntry.Type == PakFileEntryType.Directory) continue; // Ignora registros de pastas vazias

                                    bool existsInCompare = compareEntriesMap.TryGetValue(srcEntry.Name, out var compEntry);

                                    if (compareMode == "diff")
                                    {
                                        // MODO DIFERENÇA:
                                        // Não existe no meu cliente OU existe mas o tamanho/hash mudou (foi atualizado)
                                        if (!existsInCompare || (srcEntry.Size != compEntry.Size || srcEntry.CompressSize != compEntry.CompressSize))
                                        {
                                            _comparisonResults.Add(new FileDiffEntry
                                            {
                                                FileName = srcEntry.Name,
                                                SourcePakPath = sourcePakPath,
                                                PakEntry = srcEntry,
                                                Reason = !existsInCompare ? "New File" : "Modified"
                                            });
                                        }
                                    }
                                    else if (compareMode == "equal")
                                    {
                                        // MODO IGUAL:
                                        // Existe em ambos e possui exatamente o mesmo tamanho
                                        if (existsInCompare && (srcEntry.Size == compEntry.Size || srcEntry.CompressSize == compEntry.CompressSize))
                                        {
                                            _comparisonResults.Add(new FileDiffEntry
                                            {
                                                FileName = srcEntry.Name,
                                                SourcePakPath = sourcePakPath,
                                                PakEntry = srcEntry,
                                                Reason = "Identical"
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        catch { /* Ignora falhas de leitura no PAK de origem */ }
                    }
                });

                // 2. Alimenta a interface gráfica (ListView) com os resultados encontrados
                lstDiffFiles.BeginUpdate();
                foreach (var item in _comparisonResults)
                {
                    var lvi = new ListViewItem(item.FileName);
                    lvi.SubItems.Add(Path.GetFileName(item.SourcePakPath)); // Nome do PAK de onde ele veio
                    lvi.SubItems.Add(item.Reason == "New File" ? GetText("New", "Novo") :
                                     item.Reason == "Modified" ? GetText("Modified", "Modificado") : GetText("Identical", "Idêntico"));
                    lvi.Tag = item;
                    lstDiffFiles.Items.Add(lvi);
                }
                lstDiffFiles.EndUpdate();

                MessageBox.Show($"{GetText("Comparison finished! Found", "Comparação concluída! Encontrados")} {_comparisonResults.Count} {GetText("files.", "arquivos.")}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetText("Error during comparison:", "Erro durante a comparação:")} {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCompare.Enabled = true;
            }
        }

        private void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = GetText("Select target client folder (extract source):", "Selecione a pasta do cliente alvo (origem da extração):");
                if (fbd.ShowDialog() == DialogResult.OK) txtSourceClient.Text = fbd.SelectedPath;
            }
        }

        private void BtnBrowseCompare_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = GetText("Select your base client folder (comparison):", "Selecione a pasta do seu cliente base (comparação):");
                if (fbd.ShowDialog() == DialogResult.OK) txtCompareClient.Text = fbd.SelectedPath;
            }
        }

        private async void BtnCompare_Click(object sender, EventArgs e)
        {
            string mode = rbDifferences.Checked ? "diff" : "equal";
            await PerformMultiPakComparisonAsync(mode);
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            lstDiffFiles.BeginUpdate();
            foreach (ListViewItem item in lstDiffFiles.Items)
            {
                item.Checked = chkSelectAll.Checked;
            }
            lstDiffFiles.EndUpdate();
        }

        /// <summary>
        /// Extrai em lote apenas os itens que o usuário marcou na ListView.
        /// </summary>
        private async void BtnExtractSelected_Click(object sender, EventArgs e)
        {
            if (lstDiffFiles.CheckedItems.Count == 0)
            {
                MessageBox.Show(GetText("Please check at least one file to extract.", "Por favor, marque pelo menos um arquivo para extrair."),
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = GetText("Select output folder:", "Selecione a pasta de saída para a extração:");
                if (fbd.ShowDialog() != DialogResult.OK) return;

                string outputDir = fbd.SelectedPath;
                btnExtractSelected.Enabled = false;

                // Agrupa por arquivo PAK físico para abrir o arquivo apenas uma vez por lote (Otimização Máxima)
                var checkedItems = lstDiffFiles.CheckedItems.Cast<ListViewItem>()
                    .Select(lvi => (FileDiffEntry)lvi.Tag)
                    .GroupBy(entry => entry.SourcePakPath);

                try
                {
                    await Task.Run(() =>
                    {
                        foreach (var pakGroup in checkedItems)
                        {
                            string currentPakPath = pakGroup.Key;
                            var entriesToExtract = pakGroup.Select(g => g.PakEntry).ToList();

                            using (var reader = new PakReader(currentPakPath))
                            {
                                // Chama o extrator da sua PakManager passando a lista filtrada do grupo
                                PakManager.ExtractFiles(currentPakPath, reader, entriesToExtract, outputDir,
    log: msg => { },
    onProgress: (done, total) =>
    {
        Invoke(new Action(() => {
            prgBar.Maximum = total;
            prgBar.Value = done;
        }));
    });
                            }
                        }
                    });

                    MessageBox.Show(GetText("Extraction of differences completed successfully!", "Extração das diferenças concluída com sucesso!"),
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{GetText("Extraction failed:", "Falha na extração:")} {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnExtractSelected.Enabled = true;
                }
            }
        }

        private string GetText(string en, string br) => (_currentLanguage == "br") ? br : en;

        // Estrutura para controle dos itens mapeados
        public class FileDiffEntry
        {
            public string FileName { get; set; } = "";
            public string SourcePakPath { get; set; } = "";
            public PakFileEntry PakEntry { get; set; } = null!;
            public string Reason { get; set; } = ""; // "New File", "Modified", "Identical"
        }
    }
}