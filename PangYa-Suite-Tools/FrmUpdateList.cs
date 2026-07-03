using PangYa_Suite_Tools.Localization;

using System.ComponentModel;
using System.Text; 
using PangyaAPI.UpdateList.Flags; 
using PangyaAPI.UpdateList.Models;
namespace PangYa_Suite_Tools
{
    public partial class FrmUpdateList : Form
    {
        private readonly Dictionary<string, FileStateApp> _fileCache = new(StringComparer.OrdinalIgnoreCase); 
        private UpdateMaker? _updateMaker;
        private List<UpdateEntry> _updateEntries = new();

        private FileSystemWatcher? _watcher;
        private readonly Lock _generatorLock = new();
        private bool _isMonitoring = false;
        private bool isInitializingLanguages = true;

        public FrmUpdateList()
        {
            InitializeComponent();
            InitializeLanguageComboBox();
            SetupComponents();
            LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
            Disposed += (_, _) => LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
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
            Text = Strings.Update_Title;
            tabDecrypt.Text = Strings.Update_DecryptTab;
            tabGenerator.Text = Strings.Update_GeneratorTab;
            grpConfig.Text = Strings.Update_Config;
            lblPangyaPath.Text = Strings.Update_Source;
            lblUpdatePath.Text = Strings.Update_Destination;
            lblFileKey.Text = Strings.Update_Key;
            lblPatchVersion.Text = Strings.Update_PatchVersion;
            lblUpdateListVer.Text = Strings.Update_ListVersion;
            lblClientPatchNum.Text = Strings.Update_PatchNumber;
            btnBrowsePangya.Text = Strings.Pak_Browse;
            btnBrowseUpdate.Text = Strings.Pak_Browse;
            lblLog.Text = Strings.Update_Log;
            lblLanguage.Text = Strings.Common_Language;

            // Estados dinâmicos: só atualiza se não houver monitoramento/drop em andamento, para não confundir o usuário no meio de uma operação
            if (!_isMonitoring)
            {
                btnToggleWatch.Text = Strings.UpdateList_StartMonitoring;
                lblWatchStatus.Text = Strings.UpdateList_INACTIVE;
            }
            else
            {
                btnToggleWatch.Text = Strings.UpdateList_StopMonitoring;
                lblWatchStatus.Text = Strings.UpdateList_ACTIVELYMONITORING;
            }

            if (string.IsNullOrEmpty(txtXmlViewer.Text))
            {
                lblDropHint.Text = Strings.UpdateList_DragAndDropAnEncryptedUpdatelist;
            }
        }
private void SetupComponents()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Ativa o suporte de Drag-and-Drop visual na Aba 1
            pnlCryptoDrop.AllowDrop = true;
            pnlCryptoDrop.DragEnter += PnlCryptoDrop_DragEnter;
            pnlCryptoDrop.DragDrop += pnlCryptoDrop_DragDrop;

            // Alinha as chaves predefinidas do ComboBox de Região
            cboFileKey.Items.Clear();
            cboFileKey.Items.AddRange(new string[] { "JP", "TH", "US", "KR", "ID", "EU" });
            cboFileKey.SelectedIndex = 0; // "JP" como padrão estável

            // Valores de inicialização padrão sugeridos para os novos inputs da Aba 2
            txtPatchVersion.Text = "JP.R7.983.00";
            txtUpdateListVer.Text = DateTime.Now.ToString("yyyyMMdd01");
            txtClientPatchNum.Text = "1";

            Log(Strings.UpdateList_InterfaceInitializedInMultiTabMode);
        }

        #region ABA 1: VISUALIZADOR / DECRYPT DE XML

        private void PnlCryptoDrop_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void pnlCryptoDrop_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length == 0) return;

            string targetFile = files[0];

            txtXmlViewer.Clear();
            lblDropHint.Text = $"{Strings.UpdateList_Processing} {Path.GetFileName(targetFile)}...";

            string selectedKeyName = string.Empty;
            this.Invoke(() => selectedKeyName = cboFileKey.SelectedItem!.ToString()!);

            await Task.Run(() =>
            {
                try
                {
                    var operacao = UpdateKeyDetector.IsFileCrypt(targetFile);

                    if (operacao == OperacaoEnum.Decrypt)
                    {
                        this.Invoke(() => Log($"🔒 {Strings.UpdateList_ProtectedFileDetectedTestingKey} [{selectedKeyName}]..."));

                        uint[] selectedKey = GetKeysByName(selectedKeyName);
                        var reader = new UpdateReader(selectedKey);

                        try
                        {
                            var (header, entries) = reader.ReadUpdateList(targetFile);

                            if (entries != null && entries.Count > 0)
                            {
                                byte[] rawDoc = reader.XteaDecrypt(targetFile);
                                int num = Array.IndexOf(rawDoc, (byte)0);
                                string xmlText = Encoding.GetEncoding("euc-kr").GetString(rawDoc, 0, num == -1 ? rawDoc.Length : num);

                                // Embeleza o XML antes de mandar para a tela
                                string formattedXml = FormatXml(xmlText);

                                this.Invoke(() => {
                                    txtXmlViewer.Text = formattedXml;
                                    lblDropHint.Text = Strings.UpdateList_DragAndDropAnEncryptedUpdatelist;
                                    Log($"✅ [{Strings.UpdateList_SUCCESS}] {Strings.UpdateList_DecryptedWithKey} {selectedKeyName}!");
                                });
                                return;
                            }
                        }
                        catch
                        {
                            this.Invoke(() => Log($"⚠️ {Strings.UpdateList_FailedWithKey} [{selectedKeyName}]. {Strings.UpdateList_StartingAutomaticBruteForceScanner}"));
                        }

                        var result = UpdateKeyDetector.DetectAndSetKey(targetFile, out uint[]? autoDetectedKey, out byte[]? decryptedData, out string document);

                        if (result == UpdateResult.Sucess && decryptedData != null)
                        {
                            int num = Array.IndexOf(decryptedData, (byte)0);
                            string xmlText = Encoding.GetEncoding("euc-kr").GetString(decryptedData, 0, num == -1 ? decryptedData.Length : num);

                            string formattedXml = FormatXml(xmlText);

                            this.Invoke(() => {
                                txtXmlViewer.Text = formattedXml;
                                lblDropHint.Text = Strings.UpdateList_DragAndDropAnEncryptedUpdatelist;
                                Log($"✅ [{Strings.UpdateList_BRUTEFORCESUCCESS}] {Strings.UpdateList_KeyIdentifiedSuccessfully}");
                            });
                        }
                        else
                        {
                            this.Invoke(() => {
                                lblDropHint.Text = Strings.UpdateList_ErrorNoKeyDecodedTheStructure;
                                Log($"❌ [{Strings.UpdateList_TOTALFAILURE}] {Strings.UpdateList_NoKeyFromTheKnownDatabase}");
                            });
                        }
                    }
                    else if (operacao == OperacaoEnum.Encrypt)
                    {
                        string xmlText = File.ReadAllText(targetFile, Encoding.GetEncoding("euc-kr"));
                        string formattedXml = FormatXml(xmlText);

                        this.Invoke(() => {
                            txtXmlViewer.Text = formattedXml;
                            lblDropHint.Text = Strings.UpdateList_DragAndDropAnEncryptedUpdatelist;
                            Log($"📋 {Strings.UpdateList_TheDroppedFileIsAlreadyIn}");
                        });
                    }
                    else
                    {
                        this.Invoke(() => {
                            lblDropHint.Text = Strings.UpdateList_InvalidOrCorruptedFile;
                            Log(Strings.UpdateList_InvalidOrCorruptedFile);
                        });
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(() => {
                        lblDropHint.Text = Strings.UpdateList_CriticalFailureWhileParsingFile;
                        Log($"❌ [{Strings.UpdateList_PARSEERROR}] {ex.Message}");
                    });
                }
            });
        }
        #endregion

        #region ABA 2: GERADOR & MONITOR DE UPDATELIST

        private void btnBrowsePangya_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = Strings.UpdateList_SelectTheRootPangyaFolderWhere };
            if (fbd.ShowDialog() == DialogResult.OK) txtPangyaPath.Text = fbd.SelectedPath;
        }

        private void btnBrowseUpdate_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = Strings.UpdateList_SelectTheDestinationWebServerFolderFor };
            if (fbd.ShowDialog() == DialogResult.OK) txtUpdatePath.Text = fbd.SelectedPath;
        }

        private async void btnToggleWatch_Click(object sender, EventArgs e)
        {
            if (_isMonitoring) StopMonitoring();
            else await StartMonitoringAsync();
        }

        private async Task StartMonitoringAsync()
        {
            string pangyaPath = txtPangyaPath.Text;
            string destPath = txtUpdatePath.Text;

            if (!Directory.Exists(pangyaPath) || !Directory.Exists(destPath))
            {
                MessageBox.Show(Strings.UpdateList_CheckWhetherTheSourceAndWebServer, Strings.UpdateList_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnToggleWatch.Enabled = false;
            lblWatchStatus.Text = Strings.UpdateList_Initializing;

            try
            {
                string selectedKeyName = cboFileKey.SelectedItem!.ToString()!;
                string patchVersion = txtPatchVersion.Text;
                string updateVersion = txtUpdateListVer.Text;
                string patchNum = txtClientPatchNum.Text;

                uint[] regionKeys = GetKeysByName(selectedKeyName);

                await Task.Run(() =>
                {
                    _updateMaker = new UpdateMaker();
                    this.Invoke(() => Log(Strings.UpdateList_ScanningDirectoryTreeAndGeneratingInitial));

                    string finalOutputPath = Path.Combine(destPath, "updatelist");

                    // Injeção de todos os novos parâmetros capturados direto dos inputs dinâmicos do formulário
                    _updateMaker.GenerateFromDirectory(pangyaPath, finalOutputPath, regionKeys, patchVersion, updateVersion, patchNum);
                });

                _watcher = new FileSystemWatcher(pangyaPath)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.EnableRaisingEvents = true;

                _isMonitoring = true;
                btnToggleWatch.Text = Strings.UpdateList_StopMonitoring;
                btnToggleWatch.BackColor = Color.Tomato;
                lblWatchStatus.Text = Strings.UpdateList_ACTIVELYMONITORING;
                lblWatchStatus.ForeColor = Color.Green;
                Log($"[{Strings.UpdateList_SERVICE}] {Strings.UpdateList_FileSystemWatcherActiveOnFolder} {pangyaPath}");
            }
            catch (Exception ex)
            {
                Log($"[{Strings.UpdateList_INITERROR}] {ex.Message}");
                StopMonitoring();
            }
            finally
            {
                btnToggleWatch.Enabled = true;
            }
        }

        private void StopMonitoring()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            _isMonitoring = false;
            btnToggleWatch.Text = Strings.UpdateList_StartMonitoring;
            btnToggleWatch.BackColor = Color.LightGreen;
            lblWatchStatus.Text = Strings.UpdateList_INACTIVE;
            lblWatchStatus.ForeColor = Color.DimGray;
            Log(Strings.UpdateList_BackgroundMonitoringHasBeenStopped);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string ext = Path.GetExtension(e.FullPath).ToLower();
            if (ext != ".pak" && ext != ".exe" && ext != ".dll") return;

            lock (_generatorLock)
            {
                Thread.Sleep(1000); // Buffer de segurança física do Windows para liberação do lock do arquivo

                if (!File.Exists(e.FullPath)) return;

                var info = new FileInfo(e.FullPath);
                var currentState = new FileStateApp { Length = info.Length, LastWriteTime = info.LastWriteTime };

                if (_fileCache.TryGetValue(e.FullPath, out var last) && last.Length == currentState.Length) return;
                _fileCache[e.FullPath] = currentState;

                this.Invoke(() => Log($"[{Strings.UpdateList_DETECTED}] {Strings.UpdateList_FileModification} {e.Name}"));

                string pangyaPath = string.Empty;
                string destPath = string.Empty;
                string selectedKeyName = string.Empty;
                string patchVersion = string.Empty;
                string updateVersion = string.Empty;
                string patchNum = string.Empty;

                this.Invoke(() => {
                    pangyaPath = txtPangyaPath.Text;
                    destPath = txtUpdatePath.Text;
                    selectedKeyName = cboFileKey.SelectedItem!.ToString()!;
                    patchVersion = txtPatchVersion.Text;
                    updateVersion = txtUpdateListVer.Text;
                    patchNum = txtClientPatchNum.Text;
                });

                try
                {
                    string destFile = Path.Combine(destPath, e.Name!);
                    string destFileDir = Path.GetDirectoryName(destFile)!;

                    if (!Directory.Exists(destFileDir)) Directory.CreateDirectory(destFileDir);
                    File.Copy(e.FullPath, destFile, true);

                    uint[] regionKeys = GetKeysByName(selectedKeyName);
                    string finalOutputPath = Path.Combine(destPath, "updatelist");

                    // Gera o patch mantendo a paridade de versões modificadas em tempo real
                    _updateMaker?.GenerateFromDirectory(pangyaPath, finalOutputPath, regionKeys, patchVersion, updateVersion, patchNum);

                    this.Invoke(() => Log($"✨ [{Strings.UpdateList_COMPILED}] {Strings.UpdateList_UpdatelistSignedSuccessfullyTrigger} {e.Name}"));
                }
                catch (Exception ex)
                {
                    this.Invoke(() => Log($"[{Strings.UpdateList_IOERROR}] {Strings.UpdateList_CouldNotManageTheFile} {e.Name}: {ex.Message}"));
                }
            }
        }

        #endregion

        #region MÉTODOS AUXILIARES

        private uint[] GetKeysByName(string name)
        {
            return name.ToUpper() switch
            {
                "TH" => UpdateKeys.TH,
                "JP" => UpdateKeys.JP,
                "US" => UpdateKeys.GB,
                "GB" => UpdateKeys.GB,
                "KR" => UpdateKeys.KR,
                "ID" => UpdateKeys.ID,
                "EU" => UpdateKeys.EU,
                _ => UpdateKeys.JP
            };
        }

        private void Log(string text)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private static string FormatXml(string rawXml)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawXml)) return string.Empty;

                // Trata possíveis quebras incorretas ou espaços no início/fim antes do parse
                rawXml = rawXml.Trim();

                var doc = System.Xml.Linq.XDocument.Parse(rawXml);
                var settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ", // 4 Espaços para manter legível
                    NewLineOnAttributes = false, // Atributos na mesma linha
                    OmitXmlDeclaration = false,
                    NewLineHandling = System.Xml.NewLineHandling.Replace, // Força o tratamento de novas linhas
                    NewLineChars = "\r\n" // Garante o padrão do Windows (CRLF) exigido pelo TextBox
                };

                using var stringWriter = new StringWriter();
                using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, settings))
                {
                    doc.Save(xmlWriter);
                }

                string result = stringWriter.ToString();

                // Garantia extra: se o XmlWriter ainda deixar passar algum '\n' isolado, 
                // normaliza tudo para o padrão que o TextBox do WinForms aceita
                return result.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            }
            catch
            {
                // Fallback de segurança: Caso falhe, tenta pelo menos normalizar as quebras brutas
                return rawXml.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            }
        }
        #endregion
    }

    public class FileStateApp
    {
        public long Length { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}
