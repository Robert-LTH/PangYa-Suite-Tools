using PangYa_Suite_Tools.Localization;
using PangYa_Suite_Tools.Logging;
using System.Diagnostics;
using PangYa_Suite_Tools.Shop;

namespace PangYa_Suite_Tools
{
    public partial class FrmMenu : Form
    {
        private FrmLog? _logWindow;
        public FrmMenu()
        {
            InitializeComponent();
            ConfigureUiEditorButtonIcon();
            ApplyLocalization();
            LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
            Disposed += (_, _) =>
            {
                LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
                btnOpenShop.Image?.Dispose();
            };
        }

        private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
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
            btnOpenPakDiff.Text = Strings.Menu_PakDiff;
            btnOpenLog.Text = Strings.Menu_Log;
            btnOpenShop.Text = Strings.Menu_Shop;
            btnOpenFontViewer.Text = Strings.Menu_FontViewer;
        }

        private void ConfigureUiEditorButtonIcon()
        {
            btnOpenShop.Image = CreateUiEditorButtonIcon();
        }

        private static Bitmap CreateUiEditorButtonIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using var pen = new Pen(Color.Black, 1.5f);
            using var fill = new SolidBrush(Color.FromArgb(42, Color.Black));
            graphics.FillRectangle(fill, 2, 2, 10, 11);
            graphics.DrawRectangle(pen, 2, 2, 10, 11);
            graphics.DrawRectangle(pen, 4, 4, 6, 4);
            graphics.DrawLine(pen, 8, 14, 14, 8);
            graphics.DrawLine(pen, 11, 7, 15, 11);

            return bitmap;
        }

        private void btnOpenPakMaker_Click(object sender, EventArgs e)
        {
            OpenToolWindow(new FrmPakMaker(LocalizationManager.CurrentCulture.Name, ""), hideMenu: true);
        }

        private void btnOpenUpdateList_Click(object sender, EventArgs e)
        {
            OpenToolWindow(new FrmUpdateList(LocalizationManager.CurrentCulture.Name), hideMenu: true);
        }

        private void btnOpenIffManager_Click(object sender, EventArgs e)
        {
            OpenToolWindow(new FrmIFFManager(LocalizationManager.CurrentCulture.Name), hideMenu: true);
        }

        private void btnOpenOptions_Click(object sender, EventArgs e)
        {
            // Obtém o idioma selecionado em tempo real no menu principal ('br' ou 'en')
            OpenToolWindow(new FrmOptions(), hideMenu: false);
        }

        private void btnOpenPakDiff_Click(object sender, EventArgs e)
        {
            OpenToolWindow(new FrmPakDiff(LocalizationManager.CurrentCulture.Name), hideMenu: true);
        }

        private void OpenToolWindow(Form tool, bool hideMenu)
        {
            if (hideMenu) Hide();
            tool.FormClosed += (_, _) =>
            {
                tool.Dispose();
                if (hideMenu && !IsDisposed) Show();
            };
            tool.Show();
        }

        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            if (_logWindow is null || _logWindow.IsDisposed)
            {
                _logWindow = new FrmLog();
                _logWindow.FormClosed += (_, _) => _logWindow = null;
                _logWindow.Show();
                return;
            }

            if (_logWindow.WindowState == FormWindowState.Minimized)
            {
                _logWindow.WindowState = FormWindowState.Normal;
            }

            _logWindow.Activate();
        }

        private async void btnOpenShop_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog { Description = Strings.UiEditor_SelectDataFolder };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            btnOpenShop.Enabled = false;
            try
            {
                FrmPangyaUiEditor editor = await FrmPangyaUiEditor.CreateAsync(dialog.SelectedPath);
                OpenToolWindow(editor, hideMenu: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
            {
                AppLogger.Instance.Log("UI Editor",
                    $"Could not open the PangYa UI editor: {ex.GetType().Name}: {ex.Message}",
                    AppLogLevel.Error);
                MessageBox.Show(this, string.Format(LocalizationManager.CurrentCulture, Strings.UiEditor_LoadFailed, ex.Message),
                    Strings.Common_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { btnOpenShop.Enabled = true; }
        }

        private void btnOpenFontViewer_Click(object? sender, EventArgs e)
        {
            OpenToolWindow(new FrmWftViewer(), hideMenu: true);
        }

        private sealed class CenteredImageButton : Button
        {
            private bool _isMouseDown;

            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                _isMouseDown = true;
                base.OnMouseDown(mevent);
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                _isMouseDown = false;
                base.OnMouseUp(mevent);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                _isMouseDown = false;
                base.OnMouseLeave(e);
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                ButtonState state = !Enabled
                    ? ButtonState.Inactive
                    : _isMouseDown
                        ? ButtonState.Pushed
                        : ButtonState.Normal;
                ControlPaint.DrawButton(pevent.Graphics, ClientRectangle, state);

                Size imageSize = Image?.Size ?? Size.Empty;
                Size textSize = TextRenderer.MeasureText(
                    pevent.Graphics,
                    Text,
                    Font,
                    Size.Empty,
                    TextFormatFlags.NoPadding);
                int gap = imageSize.IsEmpty || string.IsNullOrEmpty(Text) ? 0 : 6;
                int totalWidth = imageSize.Width + gap + textSize.Width;
                int centerOffset = _isMouseDown ? 1 : 0;
                int x = (ClientSize.Width - totalWidth) / 2 + centerOffset;

                if (Image != null)
                {
                    int imageY = (ClientSize.Height - imageSize.Height) / 2 + centerOffset;
                    pevent.Graphics.DrawImage(Image, x, imageY, imageSize.Width, imageSize.Height);
                    x += imageSize.Width + gap;
                }

                Color textColor = Enabled ? ForeColor : SystemColors.GrayText;
                var textBounds = new Rectangle(
                    x,
                    (ClientSize.Height - textSize.Height) / 2 + centerOffset,
                    textSize.Width,
                    textSize.Height);
                TextRenderer.DrawText(
                    pevent.Graphics,
                    Text,
                    Font,
                    textBounds,
                    textColor,
                    TextFormatFlags.NoPadding);

                if (Focused && ShowFocusCues)
                {
                    Rectangle focusBounds = ClientRectangle;
                    focusBounds.Inflate(-4, -4);
                    ControlPaint.DrawFocusRectangle(pevent.Graphics, focusBounds);
                }
            }
        }
    }
}
