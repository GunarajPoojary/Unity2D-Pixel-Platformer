// ScriptToTextExporter.cs  –  Unity UI Toolkit Editor Window
// Place inside any Editor/ folder. Open: Tools > Script to Text Exporter.

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ScriptToTextExporter : EditorWindow
{
    // ══════════════════════════════════════════════════════════════════════════
    //  DATA MODEL
    // ══════════════════════════════════════════════════════════════════════════

    private class Node
    {
        public string     name;
        public string     path;           // e.g. "Assets/Scripts/Foo.cs"
        public bool       isFolder;
        public List<Node> children  = new List<Node>();
        public bool       isExpanded = true;
        public bool       isSelected = false;
        public long       sizeBytes  = 0;  // 0 for folders
        public int        lineCount  = 0;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PERSISTENCE KEYS  (EditorPrefs so settings survive domain reloads)
    // ══════════════════════════════════════════════════════════════════════════

    const string PREF_ROOT        = "STE_rootFolder";
    const string PREF_SEPARATE    = "STE_exportSeparate";
    const string PREF_DEST        = "STE_exportDest";
    const string PREF_INCL_PATH   = "STE_includeFilePath";
    const string PREF_INCL_LINES  = "STE_includeLineCount";
    const string PREF_SHOW_SIZES  = "STE_showSizes";
    const string PREF_SORT_ALPHA  = "STE_sortAlpha";

    // ══════════════════════════════════════════════════════════════════════════
    //  STATE
    // ══════════════════════════════════════════════════════════════════════════

    private string     rootFolder           = "Assets";
    private Node       rootNode;
    private List<Node> selectedNodes        = new List<Node>();
    private bool       exportAsSeparateFiles = true;
    private string     exportDestFolder     = "";    // empty = same as Assets root
    private bool       includeFilePath      = true;
    private bool       includeLineCount     = false;
    private bool       showFileSizes        = false;
    private string     searchQuery          = "";
    private bool       sortAlpha            = true;
    private int        _treeRowIndex;

    // Clipboard feedback
    private double     _clipboardFeedbackUntil;
    private Label      _clipboardFeedbackLabel;

    // Preview
    private string     _previewPath   = null;
    private string     _previewContent = null;

    // ── UI handles ────────────────────────────────────────────────────────────
    private VisualElement treeContainer;
    private Label         selCountLabel;
    private VisualElement bottomPanel;
    private VisualElement chipStrip;
    private Toggle        exportToggle;
    private Button        exportBtn;
    private Button        pingBtn;
    private TextField     rootField;
    private TextField     searchField;
    private Label         totalSizeLabel;
    private VisualElement previewPanel;
    private Label         previewHeader;
    private TextField     previewTextArea;
    private VisualElement optionsPanel;

    // ══════════════════════════════════════════════════════════════════════════
    //  THEME
    // ══════════════════════════════════════════════════════════════════════════

    // ── Cartoony Matte Theme ──────────────────────────────────────────────────
    // Warm, flat pastels with chunky ink-line borders and generous rounding.
    // Dark variant uses deep indigo-tinted slates; light uses creamy off-whites.

    static bool  Dark => EditorGUIUtility.isProSkin;

    // Backgrounds — warm-tinted neutrals, never pure grey
    static Color Bg0        => Dark ? RGB(30, 28, 38)   : RGB(245, 242, 234); // canvas
    static Color Bg1        => Dark ? RGB(40, 37, 52)   : RGB(235, 231, 220); // toolbar / bottom
    static Color Bg2        => Dark ? RGB(50, 47, 64)   : RGB(224, 219, 206); // folder / options
    static Color Bg3        => Dark ? RGB(36, 33, 46)   : RGB(252, 249, 240); // preview bg

    // Tree rows — alternating warm tones
    static Color RowEven    => Dark ? RGB(46, 43, 60)   : RGB(250, 247, 238);
    static Color RowOdd     => Dark ? RGB(42, 39, 55)   : RGB(240, 236, 226);
    static Color RowSel     => Dark ? RGB(80, 60, 160)  : RGB(115, 85, 220);  // soft purple
    static Color RowSelHov  => Dark ? RGB(92, 72, 175)  : RGB(130, 100, 235);
    static Color RowHov     => Dark ? RGB(60, 57, 78)   : RGB(230, 225, 210);
    static Color RowSearch  => Dark ? RGB(80, 65, 20)   : RGB(255, 244, 185); // warm amber tint

    // Text
    static Color TxtMain    => Dark ? RGB(230, 225, 245) : RGB(38, 32, 52);   // indigo-tinted
    static Color TxtDim     => Dark ? RGB(140, 132, 165) : RGB(148, 138, 168);
    static Color TxtSel     => RGB(255, 255, 255);
    static Color TxtFolder  => Dark ? RGB(160, 215, 255) : RGB(60, 90, 200);  // cornflower
    static Color TxtMeta    => Dark ? RGB(120, 200, 140) : RGB(70, 150, 80);  // mint green
    static Color TxtSearch  => Dark ? RGB(255, 210, 70)  : RGB(170, 105, 0);  // amber

    // Controls
    static Color Accent     => Dark ? RGB(115, 85, 220)  : RGB(105, 72, 210); // rich violet
    static Color Danger     => Dark ? RGB(220, 75, 75)   : RGB(210, 55, 55);  // tomato red
    static Color BtnNeutral => Dark ? RGB(68, 64, 88)    : RGB(205, 198, 182);
    static Color BtnGreen   => Dark ? RGB(60, 160, 90)   : RGB(50, 150, 75);  // leaf green
    static Color BorderLine => Dark ? RGB(18, 16, 26)    : RGB(140, 130, 110);// warm ink
    static Color SearchBg   => Dark ? RGB(56, 52, 72)    : RGB(248, 244, 234);

    // Ink border used on cards/rows — slightly softer than BorderLine
    static Color InkBorder  => Dark ? RGB(22, 20, 32)    : RGB(160, 148, 125);

    static Color RGB(int r, int g, int b) => new Color(r/255f, g/255f, b/255f);

    // ══════════════════════════════════════════════════════════════════════════
    //  LAYOUT CONSTANTS
    // ══════════════════════════════════════════════════════════════════════════

    const int PAD      = 14;
    const int PAD_V    = 10;
    const int GAP      = 7;
    const int ROW_H    = 30;
    const int FOLDER_H = 33;
    const int BTN_H    = 26;
    const int ICON_W   = 22;
    const int INDENT   = 20;

    // ══════════════════════════════════════════════════════════════════════════
    //  MENU
    // ══════════════════════════════════════════════════════════════════════════

    [MenuItem("Tools/Script to Text Exporter")]
    public static void ShowWindow()
    {
        var w = GetWindow<ScriptToTextExporter>("Script Exporter");
        w.minSize = new Vector2(520, 600);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════════

    private void OnEnable()
    {
        // Restore persisted settings
        rootFolder            = EditorPrefs.GetString(PREF_ROOT,       "Assets");
        exportAsSeparateFiles = EditorPrefs.GetBool  (PREF_SEPARATE,   true);
        exportDestFolder      = EditorPrefs.GetString(PREF_DEST,       "");
        includeFilePath       = EditorPrefs.GetBool  (PREF_INCL_PATH,  true);
        includeLineCount      = EditorPrefs.GetBool  (PREF_INCL_LINES, false);
        showFileSizes         = EditorPrefs.GetBool  (PREF_SHOW_SIZES, false);
        sortAlpha             = EditorPrefs.GetBool  (PREF_SORT_ALPHA, true);
    }

    private void OnDisable()
    {
        // Persist settings
        EditorPrefs.SetString(PREF_ROOT,       rootFolder);
        EditorPrefs.SetBool  (PREF_SEPARATE,   exportAsSeparateFiles);
        EditorPrefs.SetString(PREF_DEST,       exportDestFolder);
        EditorPrefs.SetBool  (PREF_INCL_PATH,  includeFilePath);
        EditorPrefs.SetBool  (PREF_INCL_LINES, includeLineCount);
        EditorPrefs.SetBool  (PREF_SHOW_SIZES, showFileSizes);
        EditorPrefs.SetBool  (PREF_SORT_ALPHA, sortAlpha);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CREATE GUI
    // ══════════════════════════════════════════════════════════════════════════

    public void CreateGUI()
    {
        BuildTree();

        var root = rootVisualElement;
        Inline(root, s =>
        {
            s.flexDirection   = FlexDirection.Column;
            s.flexGrow        = 1;
            s.flexShrink      = 1;
            s.backgroundColor = Bg0;
            s.overflow        = Overflow.Hidden;
        });

        // F5 to refresh tree
        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.F5) DoRefresh();
        }, TrickleDown.TrickleDown);

        root.Add(BuildToolbar());
        root.Add(MakeSeparator());

        // Tree scroll — takes all remaining vertical space
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        Inline(scroll, s =>
        {
            s.flexGrow        = 1;
            s.flexShrink      = 1;
            s.backgroundColor = Bg0;
        });

        treeContainer = new VisualElement();
        Inline(treeContainer, s => { s.paddingTop = 6; s.paddingBottom = 6; });
        scroll.Add(treeContainer);
        root.Add(scroll);

        // Preview panel (hidden by default)
        root.Add(MakeSeparator());
        root.Add(BuildPreviewPanel());

        // Options panel (hidden by default, toggled by ⚙ button)
        root.Add(BuildOptionsPanel());

        // Bottom export panel
        root.Add(MakeSeparator());
        root.Add(BuildBottomPanel());

        RefreshTree();
        RefreshBottomPanel();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ZONE 1 – TOOLBAR
    // ══════════════════════════════════════════════════════════════════════════

    private VisualElement BuildToolbar()
    {
        var bar = new VisualElement();
        Inline(bar, s =>
        {
            s.backgroundColor = Bg1;
            s.paddingTop      = PAD_V;
            s.paddingBottom   = PAD_V;
            s.paddingLeft     = PAD;
            s.paddingRight    = PAD;
            s.flexShrink      = 0;
        });

        // ── Header title row ──────────────────────────────────────────────────
        var rowTitle = Row();
        Inline(rowTitle, s => s.marginBottom = PAD_V);

        var titleLbl = new Label("Script to Text Exporter");
        Inline(titleLbl, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.color = TxtMain; s.fontSize = 14;
            s.unityFontStyleAndWeight = FontStyle.Bold;
            s.unityTextAlign = TextAnchor.MiddleLeft;
        });

        // ⚙ settings toggle
        var settingsBtn = Btn("⚙  Options", ToggleOptions, BtnNeutral, TxtMain, -1, BTN_H);
        Inline(settingsBtn, s => { s.paddingLeft = 10; s.paddingRight = 10; s.flexShrink = 0; });

        rowTitle.Add(titleLbl); rowTitle.Add(settingsBtn);
        bar.Add(rowTitle);

        // ── Row A: Root folder path ───────────────────────────────────────────
        var rowA = Row(); Inline(rowA, s => s.marginBottom = GAP);

        var lbl = new Label("Root Folder");
        Inline(lbl, s =>
        {
            s.width = 82; s.flexShrink = 0;
            s.color = TxtMain; s.fontSize = 12;
            s.unityFontStyleAndWeight = FontStyle.Bold;
            s.unityTextAlign = TextAnchor.MiddleLeft;
        });

        rootField = new TextField { value = rootFolder };
        Inline(rootField, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.height = BTN_H; s.marginLeft = GAP; s.marginRight = GAP;
        });
        StyleTextField(rootField);
        rootField.RegisterValueChangedCallback(e => rootFolder = e.newValue);

        var browseBtn  = Btn("Browse",    BrowseFolder, BtnNeutral, TxtMain, 64,  BTN_H);
        var refreshBtn = Btn("⟳  Refresh", DoRefresh,   BtnNeutral, TxtMain, 84,  BTN_H);

        rowA.Add(lbl); rowA.Add(rootField);
        rowA.Add(browseBtn);
        Inline(browseBtn, s => s.marginRight = GAP);
        rowA.Add(refreshBtn);
        bar.Add(rowA);

        // ── Row B: Search bar ─────────────────────────────────────────────────
        var rowB = Row(); Inline(rowB, s => s.marginBottom = GAP);

        var searchIcon = new Label("🔍");
        Inline(searchIcon, s =>
        {
            s.width = ICON_W; s.flexShrink = 0;
            s.fontSize = 12; s.unityTextAlign = TextAnchor.MiddleCenter;
            s.marginRight = 4;
        });

        searchField = new TextField { value = "" };
        searchField.Q<TextElement>()?.schedule.Execute(() =>
        {
            var el = searchField.Q<TextElement>();
            if (el != null) el.text = "";
        });
        Inline(searchField, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.height = BTN_H; s.marginRight = GAP;
        });
        StyleTextField(searchField);

        // Placeholder text via scheduled check
        searchField.schedule.Execute(() =>
        {
            var inner = searchField.Q<VisualElement>("unity-text-input");
            if (inner != null)
            {
                Inline(inner, s =>
                {
                    s.color = TxtMain;
                    s.backgroundColor = SearchBg;
                });
            }
        });

        searchField.RegisterValueChangedCallback(e =>
        {
            searchQuery = e.newValue.Trim();
            RefreshTree();
        });

        var clearSearchBtn = Btn("✕", () =>
        {
            searchQuery = "";
            searchField.SetValueWithoutNotify("");
            RefreshTree();
        }, BtnNeutral, TxtDim, BTN_H, BTN_H);

        rowB.Add(searchIcon); rowB.Add(searchField); rowB.Add(clearSearchBtn);
        bar.Add(rowB);

        // ── Row C: Select All / Deselect All / total size / count ────────────
        var rowC = Row();

        var selAllBtn = Btn("✔  Select All",   SelectAll,   Accent,  TxtSel, 110, BTN_H);
        var deselBtn  = Btn("✖  Deselect All", DeselectAll, Danger,  TxtSel, 116, BTN_H);
        Inline(deselBtn, s => s.marginLeft = GAP);

        totalSizeLabel = new Label("");
        Inline(totalSizeLabel, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.color = TxtDim; s.fontSize = 11;
            s.unityTextAlign = TextAnchor.MiddleRight;
        });

        selCountLabel = new Label("0 scripts selected");
        Inline(selCountLabel, s =>
        {
            s.color = TxtDim; s.fontSize = 11;
            s.marginLeft = 10; s.flexShrink = 0;
            s.unityTextAlign = TextAnchor.MiddleRight;
        });

        rowC.Add(selAllBtn); rowC.Add(deselBtn);
        rowC.Add(totalSizeLabel); rowC.Add(selCountLabel);
        bar.Add(rowC);

        return bar;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  OPTIONS PANEL  (collapsible)
    // ══════════════════════════════════════════════════════════════════════════

    private VisualElement BuildOptionsPanel()
    {
        optionsPanel = new VisualElement();
        optionsPanel.name = "options-panel";
        Inline(optionsPanel, s =>
        {
            s.backgroundColor = Bg2;
            s.paddingTop      = PAD_V;
            s.paddingBottom   = PAD_V;
            s.paddingLeft     = PAD;
            s.paddingRight    = PAD;
            s.flexShrink      = 0;
            s.display         = DisplayStyle.None;
        });

        var hdr = new Label("⚙  Export Options");
        Inline(hdr, s =>
        {
            s.color = TxtMain; s.fontSize = 12;
            s.unityFontStyleAndWeight = FontStyle.Bold;
            s.marginBottom = 8;
        });
        optionsPanel.Add(hdr);

        // ── Export destination ────────────────────────────────────────────────
        var destRow = Row(); Inline(destRow, s => s.marginBottom = GAP);

        var destLbl = new Label("Export To");
        Inline(destLbl, s =>
        {
            s.width = 82; s.flexShrink = 0;
            s.color = TxtMain; s.fontSize = 12;
            s.unityTextAlign = TextAnchor.MiddleLeft;
        });

        var destField = new TextField { value = exportDestFolder };
        Inline(destField, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.height = BTN_H; s.marginLeft = GAP; s.marginRight = GAP;
        });
        StyleTextField(destField);
        destField.RegisterValueChangedCallback(e => exportDestFolder = e.newValue);

        var destBrowseBtn = Btn("Browse", () =>
        {
            string f = EditorUtility.OpenFolderPanel("Select Export Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(f))
            {
                exportDestFolder = f;
                destField.SetValueWithoutNotify(f);
            }
        }, BtnNeutral, TxtMain, 64, BTN_H);

        var destClearBtn = Btn("Use Assets/", () =>
        {
            exportDestFolder = "";
            destField.SetValueWithoutNotify("");
        }, BtnNeutral, TxtDim, 80, BTN_H);

        destRow.Add(destLbl); destRow.Add(destField);
        destRow.Add(destBrowseBtn); destRow.Add(destClearBtn);
        optionsPanel.Add(destRow);

        // ── Toggle options row ────────────────────────────────────────────────
        var togglesRow = Row(); Inline(togglesRow, s => s.marginTop = 6);

        var pathToggle  = MakeOptionToggle("Include file path header",  includeFilePath,
            v => includeFilePath = v);
        var linesToggle = MakeOptionToggle("Include line count header",  includeLineCount,
            v => includeLineCount = v);
        var sizesToggle = MakeOptionToggle("Show file sizes in tree",    showFileSizes,
            v => { showFileSizes = v; RefreshTree(); });
        var sortToggle  = MakeOptionToggle("Sort files A → Z",           sortAlpha,
            v => { sortAlpha = v; BuildTree(); RefreshTree(); RefreshBottomPanel(); });

        Inline(linesToggle, s => s.marginLeft = 20);
        Inline(sizesToggle, s => s.marginLeft = 20);
        Inline(sortToggle,  s => s.marginLeft = 20);

        togglesRow.Add(pathToggle);
        togglesRow.Add(linesToggle);
        togglesRow.Add(sizesToggle);
        togglesRow.Add(sortToggle);
        optionsPanel.Add(togglesRow);

        return optionsPanel;
    }

    private Toggle MakeOptionToggle(string label, bool initialValue, System.Action<bool> onChange)
    {
        var t = new Toggle(label) { value = initialValue };
        Inline(t, s => { s.flexShrink = 0; });
        StyleToggleLabel(t);
        t.RegisterValueChangedCallback(e => onChange(e.newValue));
        return t;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PREVIEW PANEL
    // ══════════════════════════════════════════════════════════════════════════

    private VisualElement BuildPreviewPanel()
    {
        previewPanel = new VisualElement();
        previewPanel.name = "preview-panel";
        Inline(previewPanel, s =>
        {
            s.backgroundColor = Bg3;
            s.flexShrink      = 0;
            s.display         = DisplayStyle.None;
            s.maxHeight       = 280;
        });

        // Header bar
        var phdr = Row();
        Inline(phdr, s =>
        {
            s.backgroundColor = Bg2;
            s.paddingLeft = PAD; s.paddingRight = PAD;
            s.paddingTop = 5; s.paddingBottom = 5;
            s.flexShrink = 0;
        });

        previewHeader = new Label("Preview");
        Inline(previewHeader, s =>
        {
            s.flexGrow = 1; s.color = TxtMain;
            s.fontSize = 11; s.unityFontStyleAndWeight = FontStyle.Bold;
        });

        var copyBtn = Btn("📋 Copy", CopyPreviewToClipboard, BtnNeutral, TxtMain, 72, 20);
        Inline(copyBtn, s => s.marginRight = GAP);

        var closePreviewBtn = Btn("✕ Close", ClosePreview, Danger, TxtSel, 68, 20);

        // Clipboard feedback label
        _clipboardFeedbackLabel = new Label("Copied!");
        Inline(_clipboardFeedbackLabel, s =>
        {
            s.color = TxtSel; s.backgroundColor = BtnGreen;
            s.fontSize = 10;
            s.paddingLeft = 8; s.paddingRight = 8;
            s.paddingTop = 2; s.paddingBottom = 2;
            s.borderTopLeftRadius = 4; s.borderTopRightRadius = 4;
            s.borderBottomLeftRadius = 4; s.borderBottomRightRadius = 4;
            s.marginRight = GAP;
            s.display = DisplayStyle.None;
        });

        phdr.Add(previewHeader);
        phdr.Add(_clipboardFeedbackLabel);
        phdr.Add(copyBtn);
        phdr.Add(closePreviewBtn);
        previewPanel.Add(phdr);

        // Scrollable text content
        var previewScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        Inline(previewScroll, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.backgroundColor = Bg3;
        });

        previewTextArea = new TextField { multiline = true, isReadOnly = true };
        Inline(previewTextArea, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.fontSize = 11;
            s.paddingLeft = PAD; s.paddingRight = PAD;
            s.paddingTop = 8; s.paddingBottom = 8;
            s.whiteSpace = WhiteSpace.Normal;
        });
        previewTextArea.schedule.Execute(() =>
        {
            var inner = previewTextArea.Q<VisualElement>("unity-text-input");
            if (inner != null)
                Inline(inner, s =>
                {
                    s.color = TxtMain; s.backgroundColor = Bg3;
                    s.fontSize = 11;
                    s.unityFontStyleAndWeight = FontStyle.Normal;
                });
        });

        previewScroll.Add(previewTextArea);
        previewPanel.Add(previewScroll);

        // Schedule clipboard feedback hide
        previewPanel.schedule.Execute(TickClipboardFeedback).Every(100);

        return previewPanel;
    }

    private void TickClipboardFeedback()
    {
        if (_clipboardFeedbackLabel == null) return;
        bool show = EditorApplication.timeSinceStartup < _clipboardFeedbackUntil;
        _clipboardFeedbackLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ZONE 3 – BOTTOM EXPORT PANEL
    // ══════════════════════════════════════════════════════════════════════════

    private VisualElement BuildBottomPanel()
    {
        bottomPanel = new VisualElement();
        Inline(bottomPanel, s =>
        {
            s.backgroundColor = Bg1;
            s.paddingTop      = PAD_V;
            s.paddingBottom   = PAD_V;
            s.paddingLeft     = PAD;
            s.paddingRight    = PAD;
            s.flexShrink      = 0;
            s.minHeight       = 96;
        });

        // Empty state
        var empty = new Label("No scripts selected — click items in the tree above, or press ✔ Select All.");
        empty.name = "empty";
        Inline(empty, s =>
        {
            s.color = TxtDim; s.fontSize = 11;
            s.unityTextAlign = TextAnchor.MiddleCenter;
            s.paddingTop = 18; s.paddingBottom = 18;
            s.flexGrow = 1;
            s.whiteSpace = WhiteSpace.Normal;
        });
        bottomPanel.Add(empty);

        // Selected header row  [label] [total size badge]
        var selHdrRow = Row();
        selHdrRow.name = "sel-hdr";
        Inline(selHdrRow, s => s.marginBottom = 5);

        var selHdrLbl = new Label("Selected Scripts");
        Inline(selHdrLbl, s =>
        {
            s.flexGrow = 1; s.color = TxtMain;
            s.fontSize = 12;
            s.unityFontStyleAndWeight = FontStyle.Bold;
        });
        selHdrRow.Add(selHdrLbl);
        bottomPanel.Add(selHdrRow);

        // Chip strip
        chipStrip = new VisualElement();
        chipStrip.name = "chips";
        Inline(chipStrip, s =>
        {
            s.flexDirection = FlexDirection.Row;
            s.flexWrap      = Wrap.Wrap;
            s.marginBottom  = 8;
        });
        bottomPanel.Add(chipStrip);

        // Export toggle
        exportToggle = new Toggle("Export as separate files") { value = exportAsSeparateFiles };
        exportToggle.name = "exp-toggle";
        Inline(exportToggle, s => s.marginBottom = 8);
        StyleToggleLabel(exportToggle);
        exportToggle.RegisterValueChangedCallback(e =>
        {
            exportAsSeparateFiles = e.newValue;
            UpdateExportBtnText();
        });
        bottomPanel.Add(exportToggle);

        // Action row: [Ping] [Copy All] [Export]
        var actionRow = Row();
        actionRow.name = "actions";

        pingBtn = Btn("📍  Ping", PingAll, BtnNeutral, TxtMain, -1, 30);
        Inline(pingBtn, s => { s.paddingLeft = 12; s.paddingRight = 12; s.marginRight = GAP; s.flexShrink = 0; });

        var copyAllBtn = Btn("📋  Copy All", CopyAllToClipboard, BtnNeutral, TxtMain, -1, 30);
        Inline(copyAllBtn, s => { s.paddingLeft = 12; s.paddingRight = 12; s.marginRight = GAP; s.flexShrink = 0; });

        exportBtn = Btn("📄  Export to Separate Files", ExportSelectedScripts, Accent, TxtSel, -1, 30);
        Inline(exportBtn, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.unityFontStyleAndWeight = FontStyle.Bold;
        });

        actionRow.Add(pingBtn); actionRow.Add(copyAllBtn); actionRow.Add(exportBtn);
        bottomPanel.Add(actionRow);

        return bottomPanel;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  TREE RENDER
    // ══════════════════════════════════════════════════════════════════════════

    private void RefreshTree()
    {
        treeContainer.Clear();
        _treeRowIndex = 0;

        if (rootNode == null || rootNode.children.Count == 0)
        {
            var msg = new Label("No .cs scripts found in the selected folder.");
            Inline(msg, s =>
            {
                s.color = TxtDim; s.unityTextAlign = TextAnchor.MiddleCenter;
                s.paddingTop = 28; s.fontSize = 12;
            });
            treeContainer.Add(msg);
            return;
        }

        bool hasSearch = !string.IsNullOrEmpty(searchQuery);

        foreach (var child in rootNode.children)
            DrawNode(treeContainer, child, 0, hasSearch);
    }

    private bool DrawNode(VisualElement parent, Node node, int depth, bool hasSearch)
    {
        if (node.isFolder)
            return DrawFolder(parent, node, depth, hasSearch);
        else
            return DrawScript(parent, node, depth, hasSearch);
    }

    // Returns true if node or any descendant is visible (used to hide empty folders during search)
    private bool DrawFolder(VisualElement parent, Node node, int depth, bool hasSearch)
    {
        // When searching, skip folders with no matching descendants
        bool anyVisible = false;
        if (hasSearch)
        {
            anyVisible = NodeMatchesSearch(node);
            if (!anyVisible) return false;
        }

        var header = Row();
        Inline(header, s =>
        {
            s.height = FOLDER_H;
            s.backgroundColor = Bg2;
            s.paddingLeft  = PAD + depth * INDENT;
            s.paddingRight = PAD;
            s.marginTop    = depth == 0 ? 8 : 2;
            s.borderTopLeftRadius    = 8; s.borderTopRightRadius    = 8;
            s.borderBottomLeftRadius  = node.isExpanded ? 0 : 8;
            s.borderBottomRightRadius = node.isExpanded ? 0 : 8;
            s.borderTopWidth    = 2; s.borderRightWidth   = 2;
            s.borderLeftWidth   = depth == 0 ? 4 : 2;
            s.borderBottomWidth = node.isExpanded ? 0 : 2;
            s.borderTopColor    = InkBorder; s.borderRightColor  = InkBorder;
            s.borderBottomColor = InkBorder;
            s.borderLeftColor   = depth == 0 ? Accent : InkBorder;
        });

        var arrow = new Label(node.isExpanded || hasSearch ? "▾" : "▸");
        Inline(arrow, s =>
        {
            s.width = ICON_W; s.flexShrink = 0;
            s.color = TxtFolder; s.fontSize = 13;
            s.unityTextAlign = TextAnchor.MiddleCenter;
        });

        var folderIcon = new Label("📁");
        Inline(folderIcon, s =>
        {
            s.width = ICON_W; s.flexShrink = 0;
            s.fontSize = 13; s.unityTextAlign = TextAnchor.MiddleCenter;
            s.marginRight = 4;
        });

        var nameLbl = new Label(node.name);
        Inline(nameLbl, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.color = TxtFolder; s.fontSize = 13;
            s.unityFontStyleAndWeight = FontStyle.Bold;
            s.unityTextAlign = TextAnchor.MiddleLeft;
            s.overflow = Overflow.Hidden;
        });

        header.Add(arrow); header.Add(folderIcon); header.Add(nameLbl);

        int scriptCount = ScriptCount(node);
        if (scriptCount > 0)
        {
            var selFBtn = Btn($"Select ({scriptCount})",
                () => { SelectFolderRec(node); RefreshTree(); RefreshBottomPanel(); },
                Accent, TxtSel, -1, 20);
            Inline(selFBtn, s => { s.fontSize = 10; s.paddingLeft = 8; s.paddingRight = 8; s.flexShrink = 0; s.marginRight = GAP; });

            var desFBtn = Btn("Deselect",
                () => { DeselectFolderRec(node); RefreshTree(); RefreshBottomPanel(); },
                Danger, TxtSel, -1, 20);
            Inline(desFBtn, s => { s.fontSize = 10; s.paddingLeft = 8; s.paddingRight = 8; s.flexShrink = 0; });

            header.Add(selFBtn); header.Add(desFBtn);
        }

        header.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target is Button) return;
            node.isExpanded = !node.isExpanded;
            RefreshTree();
        });
        header.RegisterCallback<MouseEnterEvent>(_ => header.style.backgroundColor = RowHov);
        header.RegisterCallback<MouseLeaveEvent>(_ => header.style.backgroundColor = Bg2);

        parent.Add(header);

        bool expanded = node.isExpanded || hasSearch;
        if (expanded && node.children.Count > 0)
        {
            var childZone = new VisualElement();
            Inline(childZone, s =>
            {
                s.backgroundColor         = Dark ? RGB(38, 35, 50) : RGB(240, 236, 224);
                s.marginBottom            = depth == 0 ? 4 : 1;
                s.borderBottomLeftRadius  = 8;
                s.borderBottomRightRadius = 8;
                s.borderBottomWidth = 2; s.borderLeftWidth = 2; s.borderRightWidth = 2;
                s.borderBottomColor = InkBorder;
                s.borderLeftColor   = depth == 0 ? Accent : InkBorder;
                s.borderRightColor  = InkBorder;
                s.paddingBottom     = 4;
            });
            foreach (var child in node.children)
                DrawNode(childZone, child, depth + 1, hasSearch);
            parent.Add(childZone);
        }

        return true;
    }

    private bool DrawScript(VisualElement parent, Node node, int depth, bool hasSearch)
    {
        // Filter: hide scripts that don't match the search query
        if (hasSearch && node.name.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) < 0)
            return false;

        bool sel        = node.isSelected;
        int  localIndex = _treeRowIndex++;
        bool matched    = hasSearch;

        var row = Row();
        Inline(row, s =>
        {
            s.height       = ROW_H;
            s.paddingLeft  = PAD + depth * INDENT;
            s.paddingRight = PAD;
            s.marginTop    = 2; s.marginBottom = 2;
            s.borderTopLeftRadius    = 6; s.borderTopRightRadius    = 6;
            s.borderBottomLeftRadius = 6; s.borderBottomRightRadius = 6;
            s.backgroundColor = sel ? RowSel
                              : matched ? RowSearch
                              : (localIndex % 2 == 0 ? RowEven : RowOdd);
            // Ink outline on every row
            s.borderTopWidth    = 1; s.borderBottomWidth = 2; // bottom thicker = shadow
            s.borderLeftWidth   = sel ? 4 : 1;
            s.borderRightWidth  = 1;
            s.borderTopColor    = InkBorder; s.borderBottomColor = InkBorder;
            s.borderRightColor  = InkBorder;
            s.borderLeftColor   = sel ? Accent : InkBorder;
        });

        var check = new Label(sel ? "☑" : "☐");
        Inline(check, s =>
        {
            s.width = ICON_W; s.flexShrink = 0;
            s.color = sel ? TxtSel : TxtDim;
            s.fontSize = 15; s.unityTextAlign = TextAnchor.MiddleCenter;
            s.marginRight = 2;
        });

        var fileIcon = new Label("📄");
        Inline(fileIcon, s =>
        {
            s.width = ICON_W; s.flexShrink = 0;
            s.fontSize = 12; s.unityTextAlign = TextAnchor.MiddleCenter;
            s.marginRight = 6;
        });

        var nameLbl = new Label(node.name);
        Inline(nameLbl, s =>
        {
            s.flexGrow = 1; s.flexShrink = 1;
            s.color  = sel ? TxtSel : matched ? TxtSearch : TxtMain;
            s.fontSize = 12;
            s.unityFontStyleAndWeight = sel || matched ? FontStyle.Bold : FontStyle.Normal;
            s.unityTextAlign = TextAnchor.MiddleLeft;
            s.overflow = Overflow.Hidden;
        });

        row.Add(check); row.Add(fileIcon); row.Add(nameLbl);

        // Optional meta info: line count and/or file size
        if ((showFileSizes || includeLineCount) && node.sizeBytes > 0)
        {
            var parts = new List<string>();
            if (showFileSizes)  parts.Add(FormatBytes(node.sizeBytes));
            if (includeLineCount && node.lineCount > 0) parts.Add($"{node.lineCount} lines");

            var metaLbl = new Label(string.Join("  ", parts));
            Inline(metaLbl, s =>
            {
                s.color    = TxtMeta; s.fontSize = 10;
                s.flexShrink = 0; s.marginLeft = 8;
                s.unityTextAlign = TextAnchor.MiddleRight;
            });
            row.Add(metaLbl);
        }

        // Right-click context menu
        row.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction(sel ? "Deselect" : "Select", _ =>
            {
                node.isSelected = !sel;
                if (node.isSelected) { if (!selectedNodes.Contains(node)) selectedNodes.Add(node); }
                else selectedNodes.Remove(node);
                RefreshTree(); RefreshBottomPanel();
            });
            evt.menu.AppendAction("Preview", _ => OpenPreview(node));
            evt.menu.AppendAction("Copy to Clipboard", _ => CopyNodeToClipboard(node));
            evt.menu.AppendAction("Copy Path", _ =>
            {
                EditorGUIUtility.systemCopyBuffer = node.path;
                _clipboardFeedbackUntil = EditorApplication.timeSinceStartup + 1.5;
            });
            evt.menu.AppendAction("Ping in Project", _ =>
            {
                var a = AssetDatabase.LoadAssetAtPath<Object>(node.path);
                if (a != null) EditorGUIUtility.PingObject(a);
            });
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Open in External Editor", _ =>
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(node.path)));
        }));

        // Left-click to toggle selection
        row.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.button != 0) return;
            node.isSelected = !node.isSelected;
            if (node.isSelected) { if (!selectedNodes.Contains(node)) selectedNodes.Add(node); }
            else selectedNodes.Remove(node);
            RefreshTree(); RefreshBottomPanel();
        });

        // Double-click to preview
        row.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.clickCount == 2) OpenPreview(node);
        });

        row.RegisterCallback<MouseEnterEvent>(_ =>
            row.style.backgroundColor = node.isSelected ? RowSelHov : RowHov);
        row.RegisterCallback<MouseLeaveEvent>(_ =>
            row.style.backgroundColor = node.isSelected ? RowSel
                : matched ? RowSearch
                : (localIndex % 2 == 0 ? RowEven : RowOdd));

        parent.Add(row);
        return true;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PREVIEW
    // ══════════════════════════════════════════════════════════════════════════

    private void OpenPreview(Node node)
    {
        string abs = ToAbs(node.path);
        if (!File.Exists(abs)) return;

        try
        {
            _previewPath    = node.path;
            _previewContent = File.ReadAllText(abs);

            previewHeader.text = $"Preview  —  {node.name}";
            previewTextArea.SetValueWithoutNotify(_previewContent);
            previewPanel.style.display = DisplayStyle.Flex;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Preview failed for {node.name}: {e.Message}");
        }
    }

    private void ClosePreview()
    {
        _previewPath    = null;
        _previewContent = null;
        previewPanel.style.display = DisplayStyle.None;
    }

    private void CopyPreviewToClipboard()
    {
        if (string.IsNullOrEmpty(_previewContent)) return;
        EditorGUIUtility.systemCopyBuffer = _previewContent;
        _clipboardFeedbackUntil = EditorApplication.timeSinceStartup + 1.5;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  BOTTOM PANEL REFRESH
    // ══════════════════════════════════════════════════════════════════════════

    private void RefreshBottomPanel()
    {
        if (bottomPanel == null) return;
        bool has = selectedNodes.Count > 0;

        // Update window title with live selection count
        titleContent = new GUIContent(has
            ? $"Script Exporter ({selectedNodes.Count})"
            : "Script Exporter");

        if (selCountLabel != null)
            selCountLabel.text = has
                ? $"{selectedNodes.Count} script{(selectedNodes.Count == 1 ? "" : "s")} selected"
                : "0 scripts selected";

        // Update total size badge in toolbar
        if (totalSizeLabel != null)
        {
            if (has)
            {
                long total = selectedNodes.Sum(n => n.sizeBytes);
                totalSizeLabel.text = $"Total: {FormatBytes(total)}";
            }
            else
            {
                totalSizeLabel.text = "";
            }
        }

        SetDisplay(bottomPanel.Q<Label>("empty"),           !has);
        SetDisplay(bottomPanel.Q<VisualElement>("sel-hdr"),  has);
        SetDisplay(bottomPanel.Q<VisualElement>("chips"),    has);
        SetDisplay(bottomPanel.Q<Toggle>("exp-toggle"),      has);
        SetDisplay(bottomPanel.Q<VisualElement>("actions"),  has);

        if (!has) return;

        chipStrip.Clear();
        int show = Mathf.Min(selectedNodes.Count, 7);
        for (int i = 0; i < show; i++)
            chipStrip.Add(MakeChip(selectedNodes[i].name));
        if (selectedNodes.Count > 7)
        {
            var overflow = new Label($"+{selectedNodes.Count - 7} more");
            Inline(overflow, s =>
            {
                s.color = TxtDim; s.fontSize = 10;
                s.unityTextAlign = TextAnchor.MiddleCenter;
                s.paddingTop = 3; s.paddingBottom = 3;
                s.paddingLeft = 4; s.paddingRight = 4;
            });
            chipStrip.Add(overflow);
        }

        UpdateExportBtnText();
    }

    private VisualElement MakeChip(string text)
    {
        var chip = new Label(text);
        Inline(chip, s =>
        {
            s.color = TxtSel; s.backgroundColor = Dark ? RGB(90, 68, 180) : RGB(115, 85, 210);
            s.fontSize = 10;
            s.paddingTop = 3; s.paddingBottom = 3;
            s.paddingLeft = 10; s.paddingRight = 10;
            s.marginRight = 5; s.marginBottom = 5;
            s.borderTopLeftRadius    = 14; s.borderTopRightRadius    = 14;
            s.borderBottomLeftRadius = 14; s.borderBottomRightRadius = 14;
            s.borderTopWidth = 2; s.borderBottomWidth = 3;
            s.borderLeftWidth = 2; s.borderRightWidth = 2;
            s.borderTopColor    = InkBorder; s.borderBottomColor = InkBorder;
            s.borderLeftColor   = InkBorder; s.borderRightColor  = InkBorder;
            s.unityFontStyleAndWeight = FontStyle.Bold;
        });
        return chip;
    }

    private void UpdateExportBtnText()
    {
        if (exportBtn == null) return;
        exportBtn.text = exportAsSeparateFiles
            ? "📄  Export to Separate Files"
            : "📋  Export to Single File";
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  UI FACTORY HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    static void Inline(VisualElement e, System.Action<IStyle> fn) => fn(e.style);

    static VisualElement Row()
    {
        var r = new VisualElement();
        Inline(r, s =>
        {
            s.flexDirection = FlexDirection.Row;
            s.alignItems    = Align.Center;
            s.flexShrink    = 0;
        });
        return r;
    }

    static VisualElement MakeSeparator()
    {
        var d = new VisualElement();
        Inline(d, s =>
        {
            s.height = 1; s.flexShrink = 0;
            s.backgroundColor = BorderLine;
            s.marginTop = 0; s.marginBottom = 0;
        });
        return d;
    }

    static Button Btn(string text, System.Action click, Color bg, Color fg, int width, int height)
    {
        var b = new Button(click) { text = text };
        Inline(b, s =>
        {
            s.backgroundColor        = bg;
            s.color                  = fg;
            s.height                 = height;
            s.fontSize               = 11;
            s.borderTopLeftRadius    = 7; s.borderTopRightRadius    = 7;
            s.borderBottomLeftRadius = 7; s.borderBottomRightRadius = 7;
            // Ink outline
            s.borderTopWidth    = 2; s.borderBottomWidth = 3; // bottom = shadow
            s.borderLeftWidth   = 2; s.borderRightWidth  = 2;
            s.borderTopColor    = InkBorder; s.borderBottomColor = InkBorder;
            s.borderLeftColor   = InkBorder; s.borderRightColor  = InkBorder;
            s.paddingLeft  = 10; s.paddingRight = 10;
            s.paddingTop   = 0;  s.paddingBottom = 0;
            s.unityFontStyleAndWeight = FontStyle.Bold;
            s.unityTextAlign = TextAnchor.MiddleCenter;
            if (width > 0) { s.width = width; s.flexShrink = 0; }
        });
        return b;
    }

    static void StyleTextField(TextField tf)
    {
        // Outer element: ink border + rounded corners
        Inline(tf, s =>
        {
            s.borderTopLeftRadius    = 7; s.borderTopRightRadius    = 7;
            s.borderBottomLeftRadius = 7; s.borderBottomRightRadius = 7;
            s.borderTopWidth    = 2; s.borderBottomWidth = 3;
            s.borderLeftWidth   = 2; s.borderRightWidth  = 2;
            s.borderTopColor    = InkBorder; s.borderBottomColor = InkBorder;
            s.borderLeftColor   = InkBorder; s.borderRightColor  = InkBorder;
        });
        tf.schedule.Execute(() =>
        {
            var inner = tf.Q<VisualElement>("unity-text-input");
            if (inner == null) return;
            Inline(inner, s =>
            {
                s.color           = TxtMain;
                s.backgroundColor = Dark ? RGB(48, 44, 62) : RGB(252, 248, 238);
                s.fontSize        = 12;
                s.borderTopLeftRadius    = 5; s.borderTopRightRadius    = 5;
                s.borderBottomLeftRadius = 5; s.borderBottomRightRadius = 5;
            });
        });
    }

    static void StyleToggleLabel(Toggle t)
    {
        t.schedule.Execute(() =>
        {
            var lbl = t.Q<Label>();
            if (lbl != null) Inline(lbl, s => { s.color = TxtMain; s.fontSize = 12; });
        });
    }

    static void SetDisplay(VisualElement e, bool visible)
    {
        if (e != null) e.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CALLBACKS
    // ══════════════════════════════════════════════════════════════════════════

    private void ToggleOptions()
    {
        if (optionsPanel == null) return;
        bool nowVisible = optionsPanel.style.display == DisplayStyle.None;
        optionsPanel.style.display = nowVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void DoRefresh() { BuildTree(); RefreshTree(); RefreshBottomPanel(); }

    private void BrowseFolder()
    {
        string folder = EditorUtility.OpenFolderPanel("Select Root Folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder)) return;

        if (folder.StartsWith(Application.dataPath))
        {
            rootFolder = "Assets" + folder.Substring(Application.dataPath.Length).Replace('\\', '/');
            rootField?.SetValueWithoutNotify(rootFolder);
            DoRefresh();
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid Folder",
                "Please select a folder inside the Assets folder.", "OK");
        }
    }

    private void SelectAll()
    {
        if (rootNode == null) return;
        SelectAllRec(rootNode); RefreshTree(); RefreshBottomPanel();
    }

    private void DeselectAll()
    {
        foreach (var n in selectedNodes) n.isSelected = false;
        selectedNodes.Clear();
        RefreshTree(); RefreshBottomPanel();
    }

    private void PingAll()
    {
        foreach (var n in selectedNodes)
        {
            var a = AssetDatabase.LoadAssetAtPath<Object>(n.path);
            if (a != null) EditorGUIUtility.PingObject(a);
        }
    }

    private void CopyAllToClipboard()
    {
        if (selectedNodes.Count == 0) return;
        EditorGUIUtility.systemCopyBuffer = BuildCombinedString();
        _clipboardFeedbackUntil = EditorApplication.timeSinceStartup + 1.5;
        // Show brief feedback in the selection-count label
        var prev = selCountLabel.text;
        selCountLabel.text = "✔ Copied to clipboard!";
        selCountLabel.schedule.Execute(() => selCountLabel.text = prev).StartingIn(1500);
    }

    private void CopyNodeToClipboard(Node node)
    {
        string abs = ToAbs(node.path);
        if (!File.Exists(abs)) return;
        try
        {
            EditorGUIUtility.systemCopyBuffer = File.ReadAllText(abs);
            _clipboardFeedbackUntil = EditorApplication.timeSinceStartup + 1.5;
        }
        catch (System.Exception e) { Debug.LogError($"Copy failed for {node.name}: {e.Message}"); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SELECTION HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    private int ScriptCount(Node folder)
    {
        int c = 0; CountRec(folder, ref c); return c;
    }

    private void CountRec(Node n, ref int c)
    {
        if (n == null) return;
        if (!n.isFolder) c++;
        else foreach (var ch in n.children) CountRec(ch, ref c);
    }

    private bool NodeMatchesSearch(Node n)
    {
        if (!n.isFolder)
            return n.name.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0;
        return n.children.Any(ch => NodeMatchesSearch(ch));
    }

    private void SelectFolderRec(Node n)
    {
        if (n == null) return;
        if (!n.isFolder) { n.isSelected = true; if (!selectedNodes.Contains(n)) selectedNodes.Add(n); }
        else foreach (var ch in n.children) SelectFolderRec(ch);
    }

    private void DeselectFolderRec(Node n)
    {
        if (n == null) return;
        if (!n.isFolder) { if (n.isSelected) { n.isSelected = false; selectedNodes.Remove(n); } }
        else foreach (var ch in n.children) DeselectFolderRec(ch);
    }

    private void SelectAllRec(Node n)
    {
        if (n == null) return;
        if (!n.isFolder) { n.isSelected = true; if (!selectedNodes.Contains(n)) selectedNodes.Add(n); }
        else foreach (var ch in n.children) SelectAllRec(ch);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  EXPORT
    // ══════════════════════════════════════════════════════════════════════════

    private void ExportSelectedScripts()
    {
        if (selectedNodes.Count == 0) return;
        if (exportAsSeparateFiles) ExportSeparate();
        else ExportCombined();
    }

    private string ResolveExportDir()
    {
        if (!string.IsNullOrEmpty(exportDestFolder) && Directory.Exists(exportDestFolder))
            return exportDestFolder;
        return Application.dataPath;
    }

    private string BuildFileHeader(Node node)
    {
        var sb = new System.Text.StringBuilder();
        if (includeFilePath)
            sb.AppendLine($"// Path: {node.path}");
        if (includeLineCount && node.lineCount > 0)
            sb.AppendLine($"// Lines: {node.lineCount}");
        if (sb.Length > 0) sb.AppendLine();
        return sb.ToString();
    }

    private void ExportSeparate()
    {
        string destDir = ResolveExportDir();
        int ok = 0, fail = 0;
        foreach (var n in selectedNodes)
        {
            string abs = ToAbs(n.path);
            if (!File.Exists(abs)) { Debug.LogWarning("Not found: " + abs); fail++; continue; }
            try
            {
                string content = BuildFileHeader(n) + File.ReadAllText(abs);
                string dest    = Path.Combine(destDir, n.name + ".txt");
                File.WriteAllText(dest, content);
                ok++;
            }
            catch (System.Exception e) { Debug.LogError($"Export failed ({n.name}): {e.Message}"); fail++; }
        }
        AssetDatabase.Refresh();
        if (ok > 0)
        {
            Debug.Log($"Exported {ok} script(s) as separate .txt files → {ResolveExportDir()}");
            if (EditorUtility.DisplayDialog("Export Complete",
                $"Exported {ok} script{(ok == 1 ? "" : "s")} successfully.\n\nDestination:\n{ResolveExportDir()}",
                "Show in Explorer", "OK"))
                EditorUtility.RevealInFinder(ResolveExportDir());
        }
        if (fail > 0) Debug.LogWarning($"Failed to export {fail} script(s).");
    }

    private void ExportCombined()
    {
        string content = BuildCombinedString();
        string ts      = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string dest    = Path.Combine(ResolveExportDir(), $"CombinedScripts_{ts}.txt");
        try
        {
            File.WriteAllText(dest, content);
            AssetDatabase.Refresh();
            Debug.Log($"Combined export → {dest}");
            if (EditorUtility.DisplayDialog("Export Complete",
                $"Combined export written.\n\n{dest}", "Show in Explorer", "OK"))
                EditorUtility.RevealInFinder(dest);
        }
        catch (System.Exception e) { Debug.LogError($"Failed to write combined file: {e.Message}"); }
    }

    private string BuildCombinedString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// Combined Script Export");
        sb.AppendLine($"// Generated : {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"// File count: {selectedNodes.Count}");
        sb.AppendLine("// " + new string('=', 60));
        sb.AppendLine();

        foreach (var n in selectedNodes)
        {
            string abs = ToAbs(n.path);
            sb.AppendLine($"// ── FILE: {n.name} ──");
            if (includeFilePath) sb.AppendLine($"// Path: {n.path}");
            if (includeLineCount && n.lineCount > 0) sb.AppendLine($"// Lines: {n.lineCount}");
            sb.AppendLine();
            if (File.Exists(abs))
            {
                try { sb.AppendLine(File.ReadAllText(abs)); }
                catch (System.Exception e) { sb.AppendLine($"// ERROR: {e.Message}"); }
            }
            else sb.AppendLine("// FILE NOT FOUND");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  TREE BUILDER
    // ══════════════════════════════════════════════════════════════════════════

    private void BuildTree()
    {
        selectedNodes.Clear();
        if (string.IsNullOrEmpty(rootFolder)) { rootNode = null; return; }

        string rel = rootFolder.Trim();
        if (!rel.StartsWith("Assets")) { Debug.LogWarning("Root folder must start with 'Assets'."); rootNode = null; return; }

        string abs = Path.Combine(Application.dataPath, rel.Substring("Assets".Length).TrimStart('/', '\\'));
        if (!Directory.Exists(abs)) { Debug.LogWarning("Folder not found: " + abs); rootNode = null; return; }

        rootNode = new Node
        {
            name     = Path.GetFileName(abs.TrimEnd('/', '\\')),
            path     = rootFolder,
            isFolder = true
        };
        BuildTreeRec(rootNode);
    }

    private void BuildTreeRec(Node parent)
    {
        string abs = ToAbs(parent.path);

        var dirs  = Directory.GetDirectories(abs);
        var files = Directory.GetFiles(abs, "*.cs");

        if (sortAlpha)
        {
            System.Array.Sort(dirs,  System.StringComparer.OrdinalIgnoreCase);
            System.Array.Sort(files, System.StringComparer.OrdinalIgnoreCase);
        }

        foreach (var dir in dirs)
        {
            string n = Path.GetFileName(dir);
            var child = new Node { name = n, path = parent.path.TrimEnd('/') + "/" + n, isFolder = true };
            BuildTreeRec(child);
            // Only include folders that contain at least one .cs file
            if (child.children.Count > 0 || child.children.Any(c => !c.isFolder))
                parent.children.Add(child);
        }
        foreach (var file in files)
        {
            string n = Path.GetFileName(file);
            var fi   = new FileInfo(file);

            int lines = 0;
            try { lines = File.ReadAllLines(file).Length; } catch { }

            parent.children.Add(new Node
            {
                name      = n,
                path      = parent.path.TrimEnd('/') + "/" + n,
                isFolder  = false,
                sizeBytes = fi.Exists ? fi.Length : 0,
                lineCount = lines
            });
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  UTILITIES
    // ══════════════════════════════════════════════════════════════════════════

    static string ToAbs(string assetPath) =>
        Path.Combine(Application.dataPath, assetPath.Substring("Assets".Length).TrimStart('/', '\\'));

    static string FormatBytes(long bytes)
    {
        if (bytes < 1024)        return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}