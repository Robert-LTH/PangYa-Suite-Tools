using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using PangYa_Suite_Tools.Shop;
using PangyaAPI.IFF;
using PangyaAPI.PAK.Models;
using Xunit;

namespace PangYa_Suite_Tools.Tests;

public sealed class ShopTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "PangYaShopTests", Guid.NewGuid().ToString("N"));

    public ShopTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public async Task UiDocument_LoadEditAndAtomicSave_RoundTripsElementPropertiesAndStates()
    {
        string uiDirectory = Path.Combine(_directory, "ui");
        Directory.CreateDirectory(uiDirectory);
        string path = Path.Combine(uiDirectory, "shop.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <element type="FORM" name="shopmain" size="800 600">
                <item type="BUTTON" name="buy" rect="10 20 110 60">
                  <param name="normal" var="buy_normal.tga"/>
                  <param name="over" var="buy_hover.tga"/>
                  <param name="selected" var="buy_selected.tga"/>
                </item>
              </element>
            </resource>
            """, Encoding.UTF8);

        PangyaUiDocument document = PangyaUiDocument.Load(path);
        Assert.Equal(new Size(800, 600), document.CanvasSize);
        PangyaUiNode button = Assert.Single(document.Nodes, node => node.Type == "BUTTON");
        Assert.Equal(new Rectangle(10, 20, 100, 40), button.Bounds);
        Assert.Equal("buy_hover.tga", button.GetResource(PangyaUiButtonState.Hover));
        Assert.Equal("buy_selected.tga", button.GetResource(PangyaUiButtonState.Selected));

        button.SetName("purchase");
        button.SetBounds(new Rectangle(25, 35, 120, 45));
        button.SetResource("purchase_normal.tga");
        await document.SaveAtomicAsync();

        PangyaUiNode saved = Assert.Single(PangyaUiDocument.Load(path).Nodes, node => node.Type == "BUTTON");
        Assert.Equal("purchase", saved.Name);
        Assert.Equal(new Rectangle(25, 35, 120, 45), saved.Bounds);
        Assert.Equal("purchase_normal.tga", saved.GetResource(PangyaUiButtonState.Normal));
        Assert.Empty(Directory.EnumerateFiles(uiDirectory, "*.tmp"));
    }

    [Fact]
    public void UiDocument_RejectsDocumentTypeDefinitions()
    {
        string path = Path.Combine(_directory, "unsafe.xml");
        File.WriteAllText(path, """
            <!DOCTYPE resource [<!ENTITY payload SYSTEM "file:///does-not-exist">]>
            <resource><element type="FORM" name="unsafe" size="640 480">&payload;</element></resource>
            """, Encoding.UTF8);

        Assert.Throws<System.Xml.XmlException>(() => PangyaUiDocument.Load(path));
    }

    [Fact]
    public void UiDocument_AcceptsStandardAndLegacyMultilineComments()
    {
        string path = Path.Combine(_directory, "comments.xml");
        File.WriteAllText(path, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <!--
                A standards-compliant multiline comment.
              -->
              <!--
                ---------------- UI FORM ----------------
              -->
              <element type="FORM" name="commented" size="640 480"/>
            </resource>
            """, Encoding.UTF8);

        PangyaUiDocument document = PangyaUiDocument.Load(path);

        PangyaUiNode form = Assert.Single(document.Nodes);
        Assert.Equal("commented", form.Name);
    }

    [Fact]
    public void UiDocument_AcceptsLegacyAttributesWithoutWhitespaceSeparator()
    {
        string path = Path.Combine(_directory, "adjacent-attributes.xml");
        File.WriteAllText(path, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <item type="STATIC" name="tooltip" caption="Legacy tooltip :"pos="326 261" size="120 20"/>
            </resource>
            """, Encoding.UTF8);

        PangyaUiDocument document = PangyaUiDocument.Load(path);

        PangyaUiNode item = Assert.Single(document.Nodes);
        Assert.Equal("Legacy tooltip :", item.Element.GetAttribute("caption"));
        Assert.Equal(new Rectangle(326, 261, 120, 20), item.Bounds);
    }

    [Fact]
    public async Task UiDocument_ConditionalElementsAreFilteredAndDirectivesArePreserved()
    {
        string path = Path.Combine(_directory, "conditional.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <element type="FORM" name="form" size="200 120">
                <item type="LABEL" name="normal" rect="0 0 20 20"/>
                <!-- #ifdef _JAPAN_ -->
                <item type="LABEL" name="marker" rect="20 0 40 20"/>
                <!-- #ifdef _DETAIL_ -->
                <item type="LABEL" name="nested" rect="40 0 60 20"/>
                <!-- #endif -->
                <!-- #endif -->
                <!-- #ifdef _JAPAN_
                <item type="LABEL" name="preview" rect="60 0 80 20"/>
                #endif -->
                <!-- #ifdef _JAPAN_
                <item type="LABEL" name="broken" rect="80 0 100 20">
                #endif -->
              </element>
            </resource>
            """, Encoding.UTF8);

        PangyaUiDocument document = PangyaUiDocument.Load(path);
        PangyaUiNode form = Assert.Single(document.Nodes, node => node.IsForm);
        var japan = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_JAPAN_" };
        var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_JAPAN_", "_DETAIL_" };

        Assert.Equal(["_DETAIL_", "_JAPAN_"],
            document.ConditionalSymbols.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        Assert.Equal(["form", "normal"],
            document.GetNodesForForm(form).Select(node => node.Name));
        Assert.Equal(["form", "normal", "marker", "preview"],
            document.GetNodesForForm(form, japan).Select(node => node.Name));
        Assert.Equal(["form", "normal", "marker", "nested", "preview"],
            document.GetNodesForForm(form, all).Select(node => node.Name));

        PangyaUiNode marker = Assert.Single(document.Nodes, node => node.Name == "marker");
        PangyaUiNode preview = Assert.Single(document.Nodes, node => node.Name == "preview");
        Assert.True(marker.IsEditable);
        Assert.True(preview.IsPreviewOnly);
        Assert.Throws<InvalidOperationException>(() => preview.SetName("changed_preview"));
        Assert.Contains(document.Warnings, warning =>
            warning.Contains("malformed #ifdef fragment", StringComparison.OrdinalIgnoreCase));

        marker.SetName("changed_marker");
        await document.SaveAtomicAsync();
        string savedXml = await File.ReadAllTextAsync(path, Encoding.UTF8);
        Assert.Contains("#ifdef _JAPAN_", savedXml, StringComparison.Ordinal);
        Assert.Contains("#endif", savedXml, StringComparison.Ordinal);

        PangyaUiDocument reloaded = PangyaUiDocument.Load(path);
        Assert.Contains(reloaded.GetNodesForForm(
                Assert.Single(reloaded.Nodes, node => node.IsForm), japan),
            node => node.Name == "changed_marker");
        Assert.Contains(reloaded.Nodes, node => node.Name == "preview" && node.IsPreviewOnly);

        RunSta(() =>
        {
            ConstructorInfo constructor = typeof(FrmPangyaUiEditor).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic, binder: null,
                [typeof(string), typeof(IReadOnlyList<string>), typeof(ShopAssetResolver)],
                modifiers: null)!;
            using var form = (FrmPangyaUiEditor)constructor.Invoke(
                [_directory, new[] { path }, new ShopAssetResolver(_directory)]);
            typeof(FrmPangyaUiEditor).GetMethod("ConfigureIfdefControls",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [document]);
            ToolStrip toolbar = Assert.Single(form.Controls.OfType<ToolStrip>(),
                control => control.Name == "uiEditorToolbar");
            CheckBox[] checkBoxes = toolbar.Items.OfType<ToolStripControlHost>()
                .Select(host => host.Control)
                .OfType<CheckBox>()
                .OrderBy(checkBox => checkBox.Text, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Assert.Equal(["_DETAIL_", "_JAPAN_"], checkBoxes.Select(checkBox => checkBox.Text));
            Assert.All(checkBoxes, checkBox => Assert.False(checkBox.Checked));
        });
    }

    [Fact]
    public async Task UiDocument_RecoversOnlyCaseInsensitiveClosingTagMatches()
    {
        string path = Path.Combine(_directory, "tag-casing.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <!-- Preserve <item></ITEM> inside comments. -->
              <![CDATA[Preserve <item></ITEM> inside CDATA.]]>
              <item type="LABEL" name="case" caption="&lt;/ITEM&gt;" rect="0 0 20 20">
                <param name="normal" var="case_image"></PARAM>
              </ITEM>
            </resource>
            """, Encoding.UTF8);

        PangyaUiDocument document = PangyaUiDocument.Load(path);
        PangyaUiNode item = Assert.Single(document.Nodes);
        Assert.Equal("</ITEM>", item.Element.GetAttribute("caption"));
        await document.SaveAtomicAsync();

        string savedXml = await File.ReadAllTextAsync(path, Encoding.UTF8);
        Assert.Contains("Preserve <item></ITEM> inside comments.", savedXml, StringComparison.Ordinal);
        Assert.Contains("Preserve <item></ITEM> inside CDATA.", savedXml, StringComparison.Ordinal);
        Assert.Contains("</param>", savedXml, StringComparison.Ordinal);
        Assert.Contains("</item>", savedXml, StringComparison.Ordinal);

        string invalidPath = Path.Combine(_directory, "different-tags.xml");
        await File.WriteAllTextAsync(invalidPath,
            "<resource><item type=\"LABEL\" name=\"bad\"></element></resource>", Encoding.UTF8);
        Assert.Throws<XmlException>(() => PangyaUiDocument.Load(invalidPath));
    }

    [Fact]
    public void UiDocument_FindsXmlFilesOnlyUnderTheUiDirectory()
    {
        string uiDirectory = Path.Combine(_directory, "ui");
        Directory.CreateDirectory(Path.Combine(uiDirectory, "sub"));
        File.WriteAllText(Path.Combine(uiDirectory, "shop.xml"), "<resource/>");
        File.WriteAllText(Path.Combine(uiDirectory, "sub", "login.xml"), "<resource/>");
        File.WriteAllText(Path.Combine(_directory, "outside.xml"), "<resource/>");

        IReadOnlyList<string> files = PangyaUiDocument.FindUiFiles(_directory);

        Assert.Equal(2, files.Count);
        Assert.All(files, file => Assert.StartsWith(uiDirectory, file, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UiDocument_GetNodesForForm_ExcludesOtherForms()
    {
        string path = Path.Combine(_directory, "forms.xml");
        File.WriteAllText(path, """
            <resource>
              <element type="FORM" name="first" size="640 480">
                <item type="BUTTON" name="first_button" rect="0 0 20 20"/>
              </element>
              <element type="FORM" name="second" size="320 240">
                <item type="BUTTON" name="second_button" rect="0 0 20 20"/>
              </element>
            </resource>
            """);

        PangyaUiDocument document = PangyaUiDocument.Load(path);
        PangyaUiNode secondForm = Assert.Single(document.Nodes,
            node => node.IsForm && node.Name == "second");

        IReadOnlyList<PangyaUiNode> visible = document.GetNodesForForm(secondForm);

        Assert.Equal(["second", "second_button"], visible.Select(node => node.Name));
        Assert.All(visible, node => Assert.Same(secondForm, node.FindContainingForm()));
    }

    [Fact]
    public void ImageResources_ExposeAndLoadAllStandardRasterExtensions()
    {
        string filter = FileDialogFactory.BuildImageResourceFilter();
        foreach (string extension in new[] { ".tga", ".png", ".jpg", ".jpeg", ".gif", ".tif", ".tiff", ".ico" })
        {
            Assert.Contains(extension, IffPreviewImageLoader.SupportedExtensions, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("*" + extension, filter, StringComparison.OrdinalIgnoreCase);
        }

        string gifPath = Path.Combine(_directory, "preview.gif");
        using (var bitmap = new Bitmap(8, 8)) bitmap.Save(gifPath, System.Drawing.Imaging.ImageFormat.Gif);
        using Image? loaded = IffPreviewImageLoader.Load(gifPath);
        Assert.NotNull(loaded);
        Assert.Equal(new Size(8, 8), loaded.Size);
    }

    [Fact]
    public void UiCanvas_FillsViewportAndUsesPaddedImageBounds()
    {
        string imageDirectory = Path.Combine(_directory, "ui", "buttons");
        Directory.CreateDirectory(imageDirectory);
        using (var bitmap = new Bitmap(8, 6))
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Black);
            bitmap.Save(Path.Combine(imageDirectory, "intrinsic.png"));
        }

        string xmlPath = Path.Combine(_directory, "ui", "canvas.xml");
        File.WriteAllText(xmlPath, """
            <resource>
              <element type="FORM" name="form" size="100 80">
                <item type="IMAGE" name="explicit" rect="10 12 30 22">
                  <param name="normal" var="intrinsic.png"/>
                </item>
                <item type="IMAGE" name="intrinsic" pos="30 40" size="0 0">
                  <param name="normal" var="intrinsic.png"/>
                </item>
              </element>
            </resource>
            """);
        PangyaUiDocument document = PangyaUiDocument.Load(xmlPath);
        var assets = new ShopAssetResolver(_directory);

        RunSta(() =>
        {
            using var canvas = new PangyaUiCanvas(assets);
            PangyaUiNode form = Assert.Single(document.Nodes, node => node.IsForm);
            PangyaUiNode explicitNode = Assert.Single(document.Nodes, node => node.Name == "explicit");
            PangyaUiNode intrinsicNode = Assert.Single(document.Nodes, node => node.Name == "intrinsic");
            canvas.LoadDocument(document);
            canvas.SelectedForm = form;
            canvas.ViewportSize = new Size(200, 150);

            Assert.Equal(new Size(200, 150), canvas.Size);
            Assert.Equal(Point.Empty, canvas.LogicalPoint(
                new Point(PangyaUiCanvas.FormPadding, PangyaUiCanvas.FormPadding)));
            Assert.Equal(new Rectangle(10, 12, 8, 6), canvas.GetRenderedBounds(explicitNode));
            Assert.Equal(new Rectangle(30, 40, 8, 6), canvas.GetRenderedBounds(intrinsicNode));

            canvas.Zoom = 2f;
            Assert.Equal(new Size(264, 224), canvas.Size);
            canvas.Zoom = 1f;
            canvas.SelectedNode = intrinsicNode;
            using var rendered = new Bitmap(canvas.Width, canvas.Height);
            canvas.DrawToBitmap(rendered, canvas.ClientRectangle);
            Color outline = rendered.GetPixel(
                PangyaUiCanvas.FormPadding + intrinsicNode.Bounds.X,
                PangyaUiCanvas.FormPadding + intrinsicNode.Bounds.Y);
            Assert.Equal(Color.Orange.ToArgb(), outline.ToArgb());
        });
    }

    [Fact]
    public void UiDimensions_ParseSafelyAndUseRightBottomCoordinates()
    {
        Assert.Equal(new Point(-10, 25), PangyaUiDimensionHelper.ParsePoint("-10 25"));
        Assert.Equal(new Size(640, 480), PangyaUiDimensionHelper.ParseSize("640, 480"));
        Assert.Equal(new Rectangle(10, 20, 40, 60),
            PangyaUiDimensionHelper.ParseRectangle("10 20 50 80"));
        Assert.Null(PangyaUiDimensionHelper.ParsePoint("10"));
        Assert.Null(PangyaUiDimensionHelper.ParseSize("10 invalid"));
        Assert.Null(PangyaUiDimensionHelper.ParseRectangle("10 20 5 15"));
        Assert.Null(PangyaUiDimensionHelper.ParseRectangle(
            "999999999999999999999 0 10 10"));
    }

    [Fact]
    public void UiCanvas_UsesIntrinsicAreaSizeUnlessStretchIsEnabled()
    {
        string imageDirectory = Path.Combine(_directory, "ui", "images");
        Directory.CreateDirectory(imageDirectory);
        SaveSolidImage(Path.Combine(imageDirectory, "red.png"), Color.Red, 4, 3);

        string xmlPath = Path.Combine(_directory, "ui", "stretch.xml");
        File.WriteAllText(xmlPath, """
            <resource>
              <element type="FORM" name="form" size="80 40">
                <item type="GROUPBOX" name="container" rect="1 1 20 18"/>
                <item type="AREA" name="natural" rect="5 5 15 15">
                  <param name="bgimg" var="red.png"/>
                </item>
                <item type="AREA" name="stretched" rect="25 5 35 15">
                  <param name="bgimg" var="red.png"/>
                  <param name="stretch" var="1"/>
                </item>
                <item type="AREA" name="hidden" rect="45 5 55 15">
                  <param name="bgimg" var="red.png"/>
                  <param name="visible" var="0"/>
                </item>
              </element>
            </resource>
            """);
        PangyaUiDocument document = PangyaUiDocument.Load(xmlPath);
        var assets = new ShopAssetResolver(_directory);

        RunSta(() =>
        {
            using var canvas = new PangyaUiCanvas(assets);
            PangyaUiNode form = Assert.Single(document.Nodes, node => node.IsForm);
            PangyaUiNode group = Assert.Single(document.Nodes, node => node.Name == "container");
            PangyaUiNode natural = Assert.Single(document.Nodes, node => node.Name == "natural");
            PangyaUiNode stretched = Assert.Single(document.Nodes, node => node.Name == "stretched");
            PangyaUiNode hidden = Assert.Single(document.Nodes, node => node.Name == "hidden");
            canvas.LoadDocument(document);
            canvas.SelectedForm = form;

            Assert.Equal(new Rectangle(5, 5, 4, 3), canvas.GetRenderedBounds(natural));
            Assert.Equal(new Rectangle(25, 5, 10, 10), canvas.GetRenderedBounds(stretched));
            Assert.Equal(Rectangle.Empty, canvas.GetRenderedBounds(group));
            Assert.Equal(Rectangle.Empty, canvas.GetRenderedBounds(hidden));

            using var rendered = new Bitmap(canvas.Width, canvas.Height);
            canvas.DrawToBitmap(rendered, canvas.ClientRectangle);
            int padding = PangyaUiCanvas.FormPadding;
            Assert.Equal(Color.Red.ToArgb(), rendered.GetPixel(padding + 7, padding + 6).ToArgb());
            Assert.NotEqual(Color.Red.ToArgb(), rendered.GetPixel(padding + 12, padding + 6).ToArgb());
            Assert.Equal(Color.Red.ToArgb(), rendered.GetPixel(padding + 33, padding + 13).ToArgb());
            Assert.NotEqual(Color.Red.ToArgb(), rendered.GetPixel(padding + 47, padding + 6).ToArgb());

            canvas.CanvasMouseDown(canvas, new MouseEventArgs(MouseButtons.Left, 1,
                padding + 12, padding + 6, 0));
            Assert.Null(canvas.SelectedNode);
            canvas.CanvasMouseDown(canvas, new MouseEventArgs(MouseButtons.Left, 1,
                padding + 7, padding + 6, 0));
            Assert.Same(natural, canvas.SelectedNode);

            canvas.ShowDebugBounds = true;
            Assert.Equal(group.Bounds, canvas.GetRenderedBounds(group));
            using var debugRendered = new Bitmap(canvas.Width, canvas.Height);
            canvas.DrawToBitmap(debugRendered, canvas.ClientRectangle);
            Assert.NotEqual(rendered.GetPixel(padding + 1, padding + 1).ToArgb(),
                debugRendered.GetPixel(padding + 1, padding + 1).ToArgb());
        });
    }

    [Fact]
    public void UiCanvas_SelectedElementRendersAndHitTestsInFront()
    {
        string imageDirectory = Path.Combine(_directory, "ui", "buttons");
        Directory.CreateDirectory(imageDirectory);
        SaveSolidImage(Path.Combine(imageDirectory, "red.png"), Color.Red, 20, 20);
        SaveSolidImage(Path.Combine(imageDirectory, "blue.png"), Color.Blue, 20, 20);
        string xmlPath = Path.Combine(_directory, "ui", "front.xml");
        File.WriteAllText(xmlPath, """
            <resource>
              <element type="FORM" name="form" size="80 60">
                <item type="AREA" name="red" rect="10 10 30 30">
                  <param name="bgimg" var="red.png"/>
                </item>
                <item type="AREA" name="blue" rect="10 10 30 30">
                  <param name="bgimg" var="blue.png"/>
                </item>
              </element>
            </resource>
            """);
        PangyaUiDocument document = PangyaUiDocument.Load(xmlPath);
        var assets = new ShopAssetResolver(_directory);

        RunSta(() =>
        {
            using var canvas = new PangyaUiCanvas(assets);
            PangyaUiNode form = Assert.Single(document.Nodes, node => node.IsForm);
            PangyaUiNode red = Assert.Single(document.Nodes, node => node.Name == "red");
            canvas.LoadDocument(document);
            canvas.SelectedForm = form;
            canvas.SelectedNode = red;
            canvas.ViewportSize = new Size(120, 100);

            Assert.Same(red, canvas.RenderOrderedNodes()[^1]);
            using var rendered = new Bitmap(canvas.Width, canvas.Height);
            canvas.DrawToBitmap(rendered, canvas.ClientRectangle);
            Assert.Equal(Color.Red.ToArgb(), rendered.GetPixel(
                PangyaUiCanvas.FormPadding + 20,
                PangyaUiCanvas.FormPadding + 20).ToArgb());

            canvas.CanvasMouseDown(canvas, new MouseEventArgs(MouseButtons.Left, 1,
                PangyaUiCanvas.FormPadding + 20, PangyaUiCanvas.FormPadding + 20, 0));
            Assert.Same(red, canvas.SelectedNode);
        });
    }

    [Fact]
    public void UiCanvas_ResolvesCrossFileMacroAndFrameResources()
    {
        string uiDirectory = Path.Combine(_directory, "ui");
        string imageDirectory = Path.Combine(uiDirectory, "images");
        Directory.CreateDirectory(imageDirectory);
        SaveSolidImage(Path.Combine(imageDirectory, "macro_blue.png"), Color.Blue, 8, 8);
        for (int index = 0; index < 9; index++)
            SaveSolidImage(Path.Combine(imageDirectory, $"frame{index:00}.png"), Color.Red, 2, 2);

        string definitionsPath = Path.Combine(uiDirectory, "definitions.xml");
        File.WriteAllText(definitionsPath, """
            <resource>
              <element type="FRAME" name="SHARED_FRAME">
                <bfrm filename="frame"/>
              </element>
              <element type="MACROITEM" name="SHARED_MACRO">
                <item type="AREA" name="macro_part" rect="0 0 8 8">
                  <param name="bgimg" var="macro_blue.png"/>
                </item>
              </element>
            </resource>
            """);
        string viewPath = Path.Combine(uiDirectory, "view.xml");
        File.WriteAllText(viewPath, """
            <resource>
              <element type="FORM" name="form" size="40 30" resource="SHARED_FRAME">
                <item type="MACROITEM" name="macro_instance" pos="10 12" resource="SHARED_MACRO"/>
              </element>
            </resource>
            """);
        PangyaUiDocument document = PangyaUiDocument.Load(viewPath);
        PangyaUiResourceCatalog catalog = PangyaUiResourceCatalog.Load(
            [definitionsPath, viewPath]);
        var assets = new ShopAssetResolver(_directory);

        RunSta(() =>
        {
            using var canvas = new PangyaUiCanvas(assets, catalog);
            PangyaUiNode form = Assert.Single(document.Nodes, node => node.IsForm);
            PangyaUiNode macro = Assert.Single(document.Nodes, node => node.Name == "macro_instance");
            canvas.LoadDocument(document);
            canvas.SelectedForm = form;
            canvas.SelectedNode = form;
            canvas.ViewportSize = new Size(100, 80);

            Assert.NotNull(catalog.TryResolve("SHARED_FRAME", "FORM",
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)));
            Assert.NotNull(catalog.TryResolve("SHARED_MACRO", "MACROITEM",
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)));
            Assert.Equal(new Rectangle(10, 12, 8, 8), canvas.GetRenderedBounds(macro));

            using var rendered = new Bitmap(canvas.Width, canvas.Height);
            canvas.DrawToBitmap(rendered, canvas.ClientRectangle);
            Assert.Equal(Color.Red.ToArgb(), rendered.GetPixel(
                PangyaUiCanvas.FormPadding + 2,
                PangyaUiCanvas.FormPadding + 2).ToArgb());
            Assert.Equal(Color.Blue.ToArgb(), rendered.GetPixel(
                PangyaUiCanvas.FormPadding + 12,
                PangyaUiCanvas.FormPadding + 14).ToArgb());
        });
    }

    [Fact]
    public void UiCanvas_DraggingDifferentElementUpdatesItsPropertyCoordinates()
    {
        string uiDirectory = Path.Combine(_directory, "ui");
        Directory.CreateDirectory(uiDirectory);
        string xmlPath = Path.Combine(uiDirectory, "drag.xml");
        File.WriteAllText(xmlPath, """
            <resource>
              <element type="FORM" name="form" size="100 80">
                <item type="LABEL" name="first" rect="5 5 15 15"/>
                <item type="LABEL" name="second" rect="40 30 50 40"/>
              </element>
            </resource>
            """);
        PangyaUiDocument document = PangyaUiDocument.Load(xmlPath);
        var assets = new ShopAssetResolver(_directory);

        RunSta(() =>
        {
            using var canvas = new PangyaUiCanvas(assets);
            using var properties = new PangyaUiPropertyPanel(assets);
            PangyaUiNode form = Assert.Single(document.Nodes, node => node.IsForm);
            PangyaUiNode first = Assert.Single(document.Nodes, node => node.Name == "first");
            PangyaUiNode second = Assert.Single(document.Nodes, node => node.Name == "second");
            canvas.LoadDocument(document);
            canvas.SelectedForm = form;
            canvas.SelectedNode = first;
            canvas.ShowDebugBounds = true;
            properties.SelectedNode = first;
            canvas.SelectionChanged += (_, node) =>
            {
                canvas.SelectedNode = node;
                properties.SelectedNode = node;
            };
            canvas.ElementChanged += (_, e) => properties.SelectedNode = e.Node;

            var start = new Point(PangyaUiCanvas.FormPadding + 41, PangyaUiCanvas.FormPadding + 31);
            canvas.CanvasMouseDown(canvas, new MouseEventArgs(MouseButtons.Left, 1, start.X, start.Y, 0));
            canvas.CanvasMouseMove(canvas,
                new MouseEventArgs(MouseButtons.Left, 0, start.X + 5, start.Y + 7, 0));

            Assert.Same(second, canvas.SelectedNode);
            Assert.Same(second, properties.SelectedNode);
            Assert.Equal(new Point(45, 37), second.Bounds.Location);
            Assert.Equal(new Point(45, 37), properties.DisplayedLocation);

            canvas.CanvasMouseDown(canvas, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

            Assert.Same(form, canvas.SelectedForm);
            Assert.Same(second, canvas.SelectedNode);
            Assert.Same(second, properties.SelectedNode);
        });
    }

    [Fact]
    public void UiResourcePicker_UsesCurrentDirectoryAndStoresOnlyFilename()
    {
        string imageDirectory = Path.Combine(_directory, "ui", "buttons");
        Directory.CreateDirectory(imageDirectory);
        string currentPath = Path.Combine(imageDirectory, "current.tga");
        string replacementPath = Path.Combine(imageDirectory, "replacement.png");
        File.WriteAllBytes(currentPath, [1]);
        File.WriteAllBytes(replacementPath, [2]);
        string xmlPath = Path.Combine(_directory, "ui", "resource.xml");
        File.WriteAllText(xmlPath, """
            <resource>
              <element type="FORM" name="form" size="100 80">
                <item type="IMAGE" name="image" rect="0 0 10 10">
                  <param name="normal" var="current.tga"/>
                </item>
              </element>
            </resource>
            """);
        PangyaUiNode image = Assert.Single(PangyaUiDocument.Load(xmlPath).Nodes,
            node => node.Name == "image");
        var assets = new ShopAssetResolver(_directory);
        string outsidePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
        File.WriteAllBytes(outsidePath, [3]);

        try
        {
            RunSta(() =>
            {
                using var properties = new PangyaUiPropertyPanel(assets) { SelectedNode = image };

                Assert.Equal(imageDirectory, properties.GetResourceInitialDirectory());
                Assert.True(properties.TrySetResourceFromPath(replacementPath));
                Assert.Equal("replacement.png", image.GetResource(PangyaUiButtonState.Normal));
                Assert.False(properties.TrySetResourceFromPath(outsidePath));
                Assert.Equal("replacement.png", image.GetResource(PangyaUiButtonState.Normal));
            });
        }
        finally
        {
            try { File.Delete(outsidePath); } catch { }
        }
    }

    [Fact]
    public void LayoutParser_ExpandsMacrosAndPreservesDuplicateNamesAndCoordinates()
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, """
            <?xml version="1.0" encoding="utf-8"?><resource><element type="FORM" name="shopmain" size="800 600">
            <item type="AREA" name="same" pos="10 20"><param name="bgimg" var="one"/></item>
            <item type="AREA" name="same" rect="30 40 80 90"/><item type="MACROITEM" resource="macro"/>
            </element></resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """
            <?xml version="1.0" encoding="utf-8"?><resource><element type="MACROITEM" name="macro">
            <item type="BUTTON" name="from_macro" rect="1 2 11 12"/></element></resource>
            """, Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        Assert.Equal(new Size(800, 600), layout.Size);
        Assert.Equal(3, layout.Elements.Count);
        Assert.Equal(2, layout.Elements.Count(element => element.Name == "same"));
        Assert.Equal(new Rectangle(10, 20, 0, 0), layout.Elements[0].Bounds);
        Assert.Equal(new Rectangle(30, 40, 50, 50), layout.Elements[1].Bounds);
        Assert.Equal("from_macro", layout.Elements[2].Name);
    }

    [Fact]
    public void LayoutParser_FindsShopMainFormCaseInsensitivelyWhenNested()
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <group>
                <ELEMENT type="form" name="ShopMain" size="1024 768">
                  <ITEM type="AREA" name="nested_area" rect="5 6 25 36">
                    <PARAM name="bgimg" var="panel"/>
                  </ITEM>
                </ELEMENT>
              </group>
            </resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """<?xml version="1.0" encoding="utf-8"?><resource/>""", Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        Assert.Equal(new Size(1024, 768), layout.Size);
        ShopLayoutElement element = Assert.Single(layout.Elements);
        Assert.Equal("nested_area", element.Name);
        Assert.Equal(new Rectangle(5, 6, 20, 30), element.Bounds);
        Assert.Equal("panel", element.Parameters["bgimg"]);
    }

    [Fact]
    public void LayoutParser_AcceptsFormTagWithIdAndUppercaseAttributes()
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <FORM ID="shopmain" SIZE="640 480">
                <ITEM TYPE="AREA" NAME="area_from_form_tag" RECT="1 2 21 32" />
              </FORM>
            </resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """<?xml version="1.0" encoding="utf-8"?><resource/>""", Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        Assert.Equal(new Size(640, 480), layout.Size);
        ShopLayoutElement element = Assert.Single(layout.Elements);
        Assert.Equal("AREA", element.Type);
        Assert.Equal("area_from_form_tag", element.Name);
        Assert.Equal(new Rectangle(1, 2, 20, 30), element.Bounds);
    }

    [Theory]
    [InlineData("""<FORM ID="shopmain" WIDTH="640" HEIGHT="480" />""", 640, 480)]
    [InlineData("""<FORM ID="shopmain" W="800" H="600" />""", 800, 600)]
    [InlineData("""<FORM ID="shopmain" RECT="10,20,1034,788" />""", 1024, 768)]
    [InlineData("""<FORM ID="shopmain" SIZE="width=1280 height=720" />""", 1280, 720)]
    public void LayoutParser_AcceptsAlternateShopMainSizeFormats(string formXml, int width, int height)
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, $"""
            <?xml version="1.0" encoding="utf-8"?>
            <resource>{formXml}</resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """<?xml version="1.0" encoding="utf-8"?><resource/>""", Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        Assert.Equal(new Size(width, height), layout.Size);
        Assert.Empty(layout.Elements);
    }

    [Theory]
    [InlineData("""<base size="960 540" />""", 960, 540)]
    [InlineData("""<item type="BASE" rect="0 0 1024 768" />""", 1024, 768)]
    [InlineData("""<element name="base">1280 720</element>""", 1280, 720)]
    public void LayoutParser_UsesInlineBaseElementWhenShopMainHasNoSize(string baseXml, int width, int height)
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, $"""
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <FORM ID="shopmain">
                {baseXml}
              </FORM>
            </resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """<?xml version="1.0" encoding="utf-8"?><resource/>""", Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        Assert.Equal(new Size(width, height), layout.Size);
    }

    [Fact]
    public void LayoutParser_SkipsMissingMacrosInsteadOfFailingWholeShop()
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <FORM ID="shopmain" SIZE="640 480">
                <item type="MACROITEM" resource="under_tab_m" />
                <item type="AREA" name="still_loaded" rect="1 2 21 32" />
              </FORM>
            </resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """<?xml version="1.0" encoding="utf-8"?><resource/>""", Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        ShopLayoutElement element = Assert.Single(layout.Elements);
        Assert.Equal("still_loaded", element.Name);
    }

    [Fact]
    public void LayoutParser_ExpandsInlineShopMacroDefinitions()
    {
        string shop = Path.Combine(_directory, "shop.xml");
        string predefined = Path.Combine(_directory, "predefined.xml");
        File.WriteAllText(shop, """
            <?xml version="1.0" encoding="utf-8"?>
            <resource>
              <FORM ID="shopmain" SIZE="640 480">
                <item type="MACROITEM" resource="under_tab_m" />
              </FORM>
              <under_tab_m>
                <ITEM TYPE="BUTTON" NAME="inline_macro_button" RECT="10 20 30 45" />
              </under_tab_m>
            </resource>
            """, Encoding.UTF8);
        File.WriteAllText(predefined, """<?xml version="1.0" encoding="utf-8"?><resource/>""", Encoding.UTF8);

        ShopLayout layout = ShopLayoutParser.Load(shop, predefined);

        ShopLayoutElement element = Assert.Single(layout.Elements);
        Assert.Equal("inline_macro_button", element.Name);
        Assert.Equal(new Rectangle(10, 20, 20, 25), element.Bounds);
    }

    [Fact]
    public void TgaDecoder_DecodesBottomOriginBgraAndAlpha()
    {
        string path = Path.Combine(_directory, "test.tga");
        byte[] bytes = new byte[18 + 8];
        bytes[2] = 2;
        bytes[12] = 1;
        bytes[14] = 2;
        bytes[16] = 32;
        bytes[17] = 8;
        bytes[18] = 30; bytes[19] = 20; bytes[20] = 10; bytes[21] = 40;
        bytes[22] = 70; bytes[23] = 60; bytes[24] = 50; bytes[25] = 80;
        File.WriteAllBytes(path, bytes);

        using Bitmap bitmap = TgaDecoder.Load(path);

        Assert.Equal(Color.FromArgb(80, 50, 60, 70), bitmap.GetPixel(0, 0));
        Assert.Equal(Color.FromArgb(40, 10, 20, 30), bitmap.GetPixel(0, 1));
    }

    [Fact]
    public void TgaDecoder_RejectsTruncatedPixels()
    {
        string path = Path.Combine(_directory, "bad.tga");
        byte[] bytes = new byte[18];
        bytes[2] = 2; bytes[12] = 1; bytes[14] = 1; bytes[16] = 32;
        File.WriteAllBytes(path, bytes);
        Assert.Throws<InvalidDataException>(() => TgaDecoder.Load(path));
    }

    [Fact]
    public void AssetResolver_PrefersShopAssetsAndRejectsMissingResources()
    {
        string preferred = Path.Combine(_directory, "ui", "shop_myroom", "button.tga");
        string other = Path.Combine(_directory, "ui", "other", "button.tga");
        Directory.CreateDirectory(Path.GetDirectoryName(preferred)!);
        Directory.CreateDirectory(Path.GetDirectoryName(other)!);
        File.WriteAllBytes(preferred, [1]);
        File.WriteAllBytes(other, [2]);
        var resolver = new ShopAssetResolver(_directory);
        Assert.Equal(preferred, resolver.Resolve("button"));
        Assert.Equal(preferred, resolver.Resolve("button.tga"));
        Assert.Throws<FileNotFoundException>(() => resolver.Resolve("missing"));
    }

    [Fact]
    public void Session_ComputesBothCurrenciesAndChecksFundsAtomically()
    {
        var pang = new ShopCatalogItem("Item", 1, "Pang item", "a", 100, 80, 20, false, "a");
        var cash = new ShopCatalogItem("Item", 2, "Cash item", "b", 20_000, 0, 0, true, "b");
        var session = new ShopSession();
        session.Add(pang);
        session.Add(cash);
        Assert.Equal((80UL, 20_000UL), session.Totals(false));
        Assert.False(session.TryCheckout(false));
        Assert.Equal(2, session.Cart.Count);
        Assert.Equal(1_000_000UL, session.Pang);
        Assert.Equal(10_000UL, session.Cookies);
        session.Clear();
        session.Add(pang);
        Assert.True(session.TryCheckout(true));
        Assert.Equal(999_980UL, session.Pang);
        Assert.Empty(session.Cart);
    }

    [Theory]
    [InlineData(0x00, 0x02, "mark_new")]
    [InlineData(0x00, 0x20, "mark_hot")]
    [InlineData(0x00, 0x40, "mark_surprise_sale")]
    [InlineData(0x08, 0x00, "mark_surprise_sale")]
    [InlineData(0x00, 0x00, null)]
    public void BannerSelection_MapsShopDisplayFlags(byte shopFlags, byte moneyFlags, string? expected)
    {
        var item = new ShopCatalogItem("Item", 1, "Test", "icon", 1, 0, 0, false, "icon.tga",
            shopFlags: shopFlags, moneyFlags: moneyFlags);
        Assert.Equal(expected, ShopCanvas.GetBannerResource(item));
    }

    [Fact]
    public async Task CatalogEditor_PersistsPricesAndIconToLooseIff()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(949);
        string path = Path.Combine(_directory, "Item.iff");
        var header = new IffHeader(1, 0, 11, [0, 0, 0]);
        IffSchema schema = IffSchemaRegistry.Resolve("Item.iff", header, 196)!;
        IffRecord record = IffRecord.CreateBlank(0, 196, schema);
        record.SetValue("Enabled", true, encoding);
        record.SetValue("ItemId", 123u, encoding);
        record.SetValue("Name", "Test", encoding);
        record.SetValue("Icon", "old_icon", encoding);
        await using (var output = File.Create(path))
            await IffWriter.WriteAsync(output, header, One(record));
        var item = new ShopCatalogItem("Item", 123, "Test", "old_icon", 1, 2, 3, false, "old.tga", "Item.iff", 0);

        DateTime start = new(2026, 1, 2, 3, 4, 5);
        DateTime end = new(2027, 6, 7, 8, 9, 10);
        await ShopCatalogEditor.SaveAsync(path, item, "new_icon", 100, 80, 25, 0xA5, 0xD2, 3, 7, start, end);

        await using IffContainer container = await IffContainer.OpenAsync(path);
        await using Stream stream = await container.Entries.Single().OpenAsync(default);
        await using IffReader reader = IffReader.Open(stream, "Item.iff", new(LeaveOpen: true, SchemaRegion: "TH"));
        IffRecord saved = await Single(reader.ReadRecordsAsync());
        Assert.Equal(100u, saved.GetValue("Price", encoding));
        Assert.Equal(80u, saved.GetValue("DiscountPrice", encoding));
        Assert.Equal(25u, saved.GetValue("UsedPrice", encoding));
        Assert.Equal("new_icon", saved.GetValue("Icon", encoding));
        Assert.Equal((byte)0xA5, saved.GetValue("ShopFlags", encoding));
        Assert.Equal((byte)0xD2, saved.GetValue("MoneyFlags", encoding));
        Assert.Equal((byte)3, saved.GetValue("TimeFlag", encoding));
        Assert.Equal((byte)7, saved.GetValue("Time", encoding));
        Assert.Equal(start, saved.GetValue("StartDate", encoding));
        Assert.Equal(end, saved.GetValue("EndDate", encoding));
    }

    [Fact]
    public async Task IffReferenceResolver_ResolvesLooseReferencedItemsAndMissingIcons()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(949);
        Directory.CreateDirectory(Path.Combine(_directory, "ui", "shop_myroom"));
        using (var bitmap = new Bitmap(8, 8))
        {
            bitmap.SetPixel(0, 0, Color.Red);
            bitmap.Save(Path.Combine(_directory, "ui", "shop_myroom", "item_icon.png"));
        }

        string itemPath = Path.Combine(_directory, "Item.iff");
        var header = new IffHeader(1, 0, 11, [0, 0, 0]);
        IffSchema itemSchema = IffSchemaRegistry.Resolve("Item.iff", header, 196)!;
        IffRecord itemWithIcon = IffRecord.CreateBlank(0, 196, itemSchema);
        itemWithIcon.SetValue("ItemId", 123u, encoding);
        itemWithIcon.SetValue("Name", "Resolved Item", encoding);
        itemWithIcon.SetValue("Icon", "item_icon", encoding);
        IffRecord itemMissingIcon = IffRecord.CreateBlank(1, 196, itemSchema);
        itemMissingIcon.SetValue("ItemId", 456u, encoding);
        itemMissingIcon.SetValue("Name", "No Icon Item", encoding);
        itemMissingIcon.SetValue("Icon", "missing_icon", encoding);
        await using (var output = File.Create(itemPath))
            await IffWriter.WriteAsync(output, header, Many(itemWithIcon, itemMissingIcon));

        IffSchema setSchema = new("SetItem", 32,
        [
            new IffField("ItemCount", 0, 4, IffFieldType.UInt32),
            new IffField("Item1", 4, 4, IffFieldType.ItemIdReference,
                Reference: new IffFieldReference("Item.iff")),
            new IffField("Item2", 8, 4, IffFieldType.ItemIdReference,
                Reference: new IffFieldReference("Item.iff")),
            new IffField("Item3", 12, 4, IffFieldType.ItemIdReference,
                Reference: new IffFieldReference("Item.iff")),
            new IffField("Item1Count", 16, 2, IffFieldType.UInt16),
            new IffField("Item2Count", 18, 2, IffFieldType.UInt16),
            new IffField("Item3Count", 20, 2, IffFieldType.UInt16)
        ]);
        IffRecord setRecord = IffRecord.CreateBlank(0, 32, setSchema);
        setRecord.SetValue("ItemCount", 3u, encoding);
        setRecord.SetValue("Item1", 123u, encoding);
        setRecord.SetValue("Item2", 456u, encoding);
        setRecord.SetValue("Item3", 999u, encoding);
        setRecord.SetValue("Item1Count", (ushort)2, encoding);
        setRecord.SetValue("Item2Count", (ushort)4, encoding);
        setRecord.SetValue("Item3Count", (ushort)6, encoding);

        var document = new IffDocumentInfo("SetItem.iff", "TH", 32, setSchema, header);
        IIffReferenceResolver resolver = (await IffReferenceResolver.CreateAsync(
            document, null, _directory, Path.Combine(_directory, "SetItem.iff"), "TH", encoding,
            new EmbeddedIffSchemaProvider(), CancellationToken.None))!;

        IffReferenceCatalogItem[] catalog = resolver.GetCatalog(setSchema.Fields[1]).ToArray();
        Assert.Equal(2, catalog.Length);
        Assert.Contains(catalog, item => item.Key == 123u && item.Name == "Resolved Item" && item.IconPath is not null);

        IffReferenceDisplay resolved = resolver.Resolve(setSchema.Fields[1], setRecord.GetValue("Item1", encoding));
        IffReferenceDisplay missingIcon = resolver.Resolve(setSchema.Fields[2], setRecord.GetValue("Item2", encoding));
        IffReferenceDisplay missingRecord = resolver.Resolve(setSchema.Fields[3], setRecord.GetValue("Item3", encoding));

        Assert.Equal("Resolved Item", resolved.Name);
        Assert.NotNull(resolved.IconPath);
        Assert.Equal("No Icon Item", missingIcon.Name);
        Assert.True(missingIcon.MissingIcon);
        Assert.True(missingRecord.MissingRecord);
    }

    [Fact]
    public async Task IffReferenceResolver_UsesSelectedDataRootForLooseReferencedIffs()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(949);
        string dataRoot = Path.Combine(_directory, "selected-data");
        Directory.CreateDirectory(dataRoot);
        Directory.CreateDirectory(Path.Combine(dataRoot, "custom_icons"));
        using (var bitmap = new Bitmap(8, 8))
        {
            bitmap.SetPixel(0, 0, Color.Blue);
            bitmap.Save(Path.Combine(dataRoot, "custom_icons", "root_icon.png"));
        }

        var header = new IffHeader(1, 0, 11, [0, 0, 0]);
        IffSchema itemSchema = IffSchemaRegistry.Resolve("Item.iff", header, 196)!;
        IffRecord item = IffRecord.CreateBlank(0, 196, itemSchema);
        item.SetValue("ItemId", 321u, encoding);
        item.SetValue("Name", "Data Root Item", encoding);
        item.SetValue("Icon", "root_icon", encoding);
        await using (var output = File.Create(Path.Combine(dataRoot, "Item.iff")))
            await IffWriter.WriteAsync(output, header, One(item));

        IffSchema setSchema = new("SetItem", 8,
        [
            new IffField("Item1", 0, 4, IffFieldType.ItemIdReference,
                Reference: new IffFieldReference("Item.iff"))
        ]);
        var document = new IffDocumentInfo("SetItem.iff", "TH", 8, setSchema, header);
        var schemaProvider = new StaticIffSchemaProvider(new IffSchema("Item", 196,
        [
            new IffField("ItemId", 4, 4, IffFieldType.UInt32),
            new IffField("Name", 8, 40, IffFieldType.FixedString, Encoding: Encoding.Latin1),
            new IffField("Icon", 49, 40, IffFieldType.Icon, Encoding: Encoding.Latin1, IconPath: "custom_icons")
        ]));

        IIffReferenceResolver resolver = (await IffReferenceResolver.CreateAsync(
            document, null, null, Path.Combine(_directory, "missing-source"), "TH", encoding,
            schemaProvider, CancellationToken.None, dataRoot))!;

        IffReferenceDisplay resolved = resolver.Resolve(setSchema.Fields[0], 321u);

        Assert.Equal(dataRoot, resolver.DataRoot);
        Assert.Equal("Data Root Item", resolved.Name);
        Assert.Equal(Path.Combine(dataRoot, "custom_icons", "root_icon.png"), resolved.IconPath);
    }

    [Fact]
    public async Task IffReferenceResolver_UsesPakExtractionSidecarAsDataRoot()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(949);
        string dataRoot = Path.Combine(_directory, "client");
        string dataDirectory = Path.Combine(dataRoot, "data");
        string iconDirectory = Path.Combine(dataRoot, "custom_icons");
        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(iconDirectory);
        string pangyaPath = Path.Combine(dataDirectory, "pangya_th.iff");
        File.WriteAllBytes(pangyaPath, [1, 2, 3, 4]);
        PakExtractionSidecar.WriteForEntry(new PakFileEntry { Name = @"data\pangya_th.iff" }, pangyaPath);
        using (var bitmap = new Bitmap(8, 8))
        {
            bitmap.SetPixel(0, 0, Color.Green);
            bitmap.Save(Path.Combine(iconDirectory, "sidecar_icon.png"));
        }

        var header = new IffHeader(1, 0, 11, [0, 0, 0]);
        IffSchema itemSchema = IffSchemaRegistry.Resolve("Item.iff", header, 196)!;
        IffRecord item = IffRecord.CreateBlank(0, 196, itemSchema);
        item.SetValue("ItemId", 654u, encoding);
        item.SetValue("Name", "Sidecar Item", encoding);
        item.SetValue("Icon", "sidecar_icon", encoding);
        await using (var output = File.Create(Path.Combine(dataRoot, "Item.iff")))
            await IffWriter.WriteAsync(output, header, One(item));

        IffSchema setSchema = new("SetItem", 4,
        [
            new IffField("Item1", 0, 4, IffFieldType.ItemIdReference,
                Reference: new IffFieldReference("Item.iff"))
        ]);
        var document = new IffDocumentInfo("SetItem.iff", "TH", 4, setSchema, header);
        var schemaProvider = new StaticIffSchemaProvider(new IffSchema("Item", 196,
        [
            new IffField("ItemId", 4, 4, IffFieldType.UInt32),
            new IffField("Name", 8, 40, IffFieldType.FixedString, Encoding: Encoding.Latin1),
            new IffField("Icon", 49, 40, IffFieldType.Icon, Encoding: Encoding.Latin1, IconPath: "custom_icons")
        ]));

        IIffReferenceResolver resolver = (await IffReferenceResolver.CreateAsync(
            document, null, dataDirectory, pangyaPath, "TH", encoding, schemaProvider, CancellationToken.None))!;
        IffReferenceDisplay resolved = resolver.Resolve(setSchema.Fields[0], 654u);

        Assert.Equal(dataRoot, resolver.DataRoot);
        Assert.Equal("Sidecar Item", resolved.Name);
        Assert.Equal(Path.Combine(iconDirectory, "sidecar_icon.png"), resolved.IconPath);
    }

    [Fact]
    public async Task IffReferenceResolver_AllowsMissingOptionalDisplayAndIconFields()
    {
        Encoding encoding = Encoding.ASCII;
        var header = new IffHeader(1, 0, 11, [0, 0, 0]);
        IffSchema itemSchema = new("Item", 4,
            [new IffField("ItemId", 0, 4, IffFieldType.UInt32)]);
        IffRecord item = IffRecord.CreateBlank(0, 4, itemSchema);
        item.SetValue("ItemId", 42u, encoding);
        await using (var output = File.Create(Path.Combine(_directory, "Item.iff")))
            await IffWriter.WriteAsync(output, header, One(item));

        IffSchema setSchema = new("SetItem", 4,
        [
            new IffField("Item1", 0, 4, IffFieldType.ItemIdReference,
                Reference: new IffFieldReference("Item.iff", DisplayField: "MissingName", IconField: "MissingIcon"))
        ]);
        var document = new IffDocumentInfo("SetItem.iff", "TH", 4, setSchema, header);

        IIffReferenceResolver resolver = (await IffReferenceResolver.CreateAsync(
            document, null, _directory, Path.Combine(_directory, "SetItem.iff"), "TH", encoding,
            new StaticIffSchemaProvider(itemSchema), CancellationToken.None))!;

        IffReferenceCatalogItem catalogItem = Assert.Single(resolver.GetCatalog(setSchema.Fields[0]));
        IffReferenceDisplay display = resolver.Resolve(setSchema.Fields[0], 42u);

        Assert.Equal(42u, catalogItem.Key);
        Assert.Equal("42", catalogItem.Name);
        Assert.Equal("42", display.Name);
        Assert.False(display.MissingRecord);
        Assert.False(display.MissingIcon);
    }

    [Fact]
    public void IffReferenceResolver_BuildsItemIdTableRowWithCharacterName()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(949);
        var header = new IffHeader(1, 0, 11, [0, 0, 0]);
        IffSchema schema = IffSchemaRegistry.Resolve("Item.iff", header, 196)!;
        IffRecord record = IffRecord.CreateBlank(0, 196, schema);
        record.SetValue("ItemId", 0u, encoding);
        record.SetValue("IFF Type", 0x2Au, encoding);
        record.SetValue("Character Serial", 7u, encoding);
        record.SetValue("Position", 3u, encoding);
        record.SetValue("Group", 2u, encoding);
        record.SetValue("Type", 1u, encoding);
        record.SetValue("Serial", 99u, encoding);
        record.SetValue("Name", "Item Name", encoding);
        record.SetValue("Icon", "item_icon", encoding);

        IffItemIdTableRow row = IffReferenceResolver.TryCreateItemIdRow(schema, record, encoding,
            "Item.iff", "Item Name", "item_icon", "icon.png",
            new Dictionary<uint, string> { [7] = "Nuri" })!;

        Assert.Equal("Item.iff", row.SourceFile);
        Assert.Equal(0x2Au, row.IffType);
        Assert.Equal(7u, row.CharacterSerial);
        Assert.Equal("Nuri", row.CharacterName);
        Assert.Equal(1u, row.Type);
        Assert.Equal(99u, row.Serial);
        Assert.Contains("Nuri", row.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void IffReferenceResolver_ItemIdTableRowIsOptionalWhenFieldsAreMissing()
    {
        var schema = new IffSchema("NoItemId", 8,
            [new IffField("Name", 0, 8, IffFieldType.FixedString)]);
        IffRecord record = IffRecord.CreateBlank(0, 8, schema);

        Assert.Null(IffReferenceResolver.TryCreateItemIdRow(schema, record, Encoding.ASCII,
            "NoItemId.iff", "Name", string.Empty, null));
    }

    private static async IAsyncEnumerable<IffRecord> One(IffRecord record)
    {
        yield return record;
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<IffRecord> Many(params IffRecord[] records)
    {
        foreach (IffRecord record in records) yield return record;
        await Task.CompletedTask;
    }

    private static async Task<IffRecord> Single(IAsyncEnumerable<IffRecord> records)
    {
        await foreach (IffRecord record in records) return record;
        throw new InvalidOperationException("No record was returned.");
    }

    private static void SaveSolidImage(string path, Color color, int width, int height)
    {
        using var bitmap = new Bitmap(width, height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);
        bitmap.Save(path);
    }

    private sealed class StaticIffSchemaProvider(IffSchema schema) : IIffSchemaProvider
    {
        public IffSchemaResolution Resolve(string fileName, string region, int recordSize) =>
            fileName.Equals("Item.iff", StringComparison.OrdinalIgnoreCase)
                ? new IffSchemaResolution(schema)
                : new IffSchemaResolution(null);
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    public void Dispose()
    {
        try { Directory.Delete(_directory, recursive: true); } catch { }
    }
}
