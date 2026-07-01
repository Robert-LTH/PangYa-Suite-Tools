using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace PangYa_Suite_Tools
{
    public partial class FrmOptions : Form
    {
        private readonly string _currentLanguage;
        private readonly string _exePath;
        private const string ProgramId = "PangYaSuiteTools.PAK";
        private const string Description = "PangYa PAK Archive File";

        public FrmOptions(string currentLanguage)
        {
            _currentLanguage = currentLanguage;
            _exePath = Process.GetCurrentProcess().MainModule.FileName;

            InitializeComponent();
            ApplyLocalization();
            CheckCurrentRegistryState();
        }

        private void ApplyLocalization()
        {
            ComponentResourceManager res = new ComponentResourceManager(typeof(FrmOptions));
            string suffix = (_currentLanguage == "en") ? "_en" : "_br";

            // Títulos e Textos Principais
            this.Text = GetText("Options", "Opções");
            groupRegister.Text = GetText("Register .pak files", "Registrar arquivos .pak");
            btnCancel.Text = GetText("Cancel", "Cancelar");
            btnOK.Text = "OK";

            // Labels e Opções
            chkRegisterFile.Text = GetText("Register SuiteTools to open .pak Files", "Registrar SuiteTools para abrir arquivos .pak");
            chkShellContext.Text = GetText("Add SuiteTools shell context to Windows Explorer", "Adicionar contexto shell do SuiteTools ao Windows Explorer");

            // Validação de privilégio de Administrador
            if (IsUserAnAdmin())
            {
                lblAdminWarning.Visible = false;
                chkRegisterFile.Enabled = true;
                chkShellContext.Enabled = true;
            }
            else
            {
                lblAdminWarning.Text = GetText(
                    "SuiteTools has to be started \"As Administrator\" to be able to modify the shell entries!",
                    "O SuiteTools deve ser iniciado \"Como Administrador\" para modificar as entradas de shell!");
                lblAdminWarning.Visible = true;
                chkRegisterFile.Enabled = false;
                chkShellContext.Enabled = false;
            }
        }

        private void CheckCurrentRegistryState()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.pak"))
                {
                    if (key != null && string.Equals(key.GetValue("")?.ToString(), ProgramId, StringComparison.OrdinalIgnoreCase))
                    {
                        chkRegisterFile.Checked = true;
                    }
                }

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ProgramId}\shell\PangYaSuiteTools.OpenWithPakMaker"))
                {
                    if (key != null)
                    {
                        chkShellContext.Checked = true;
                    }
                }
            }
            catch
            {
                // Ignora falhas de leitura silenciosamente
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!IsUserAnAdmin())
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            try
            {
                // --- REGISTRO DA ASSOCIAÇÃO DIRETA (.pak executar o app) ---
                if (chkRegisterFile.Checked)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
                    {
                        if (key != null)
                        {
                            using (RegistryKey extKey = key.CreateSubKey(".pak"))
                            {
                                extKey.SetValue("", ProgramId);
                            }

                            using (RegistryKey progKey = key.CreateSubKey(ProgramId))
                            {
                                progKey.SetValue("", Description);
                                using (RegistryKey shellKey = progKey.CreateSubKey(@"shell\open\command"))
                                {
                                    shellKey.SetValue("", $"\"{_exePath}\" \"%1\"");
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Remove associação se desmarcado
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.pak", false);
                }

                // --- REGISTRO DO MENU DE CONTEXTO (Botão Direito no Windows Explorer) ---
                if (chkShellContext.Checked)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\*", true))
                    {
                        if (key != null)
                        {
                            // Cria a opção no menu de contexto para qualquer arquivo clicado com o botão direito
                            using (RegistryKey contextKey = key.CreateSubKey($@"shell\{ProgramId}"))
                            {
                                contextKey.SetValue("", GetText("Open with PakMaker", "Abrir com PakMaker"));
                                using (RegistryKey cmdKey = contextKey.CreateSubKey("command"))
                                {
                                    cmdKey.SetValue("", $"\"{_exePath}\" \"%1\"");
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Remove menu de contexto se desmarcado
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\*", true))
                    {
                        key?.DeleteSubKeyTree($@"shell\{ProgramId}", false);
                    }
                }

                // Avisa o Windows Explorer para atualizar os ícones
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetText("Failed to apply changes:", "Falha ao aplicar alterações:")} {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool IsUserAnAdmin()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private string GetText(string en, string br)
        {
            return (_currentLanguage == "br") ? br : en;
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}