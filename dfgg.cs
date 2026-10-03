using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSharpRunHtml
{
    public partial class MainForm : Form
    {
        private WebBrowser webBrowser;
        private RichTextBox htmlEditor;
        private Button btnRun;
        private Button btnReset;
        private Button btnClear;
        private SplitContainer splitContainer;
        private Label lblStatus;
        private Timer debounceTimer;

        // 默认 HTML 示例内容
        private const string DefaultHtml = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {
            background: linear-gradient(135deg, #f0f6ff 0%, #d9e8ff 100%);
            font-family: 'Segoe UI', Roboto, sans-serif;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            margin: 0;
            padding: 20px;
        }
        .card {
            background: white;
            border-radius: 32px;
            padding: 40px 48px;
            box-shadow: 0 20px 40px rgba(0, 30, 70, 0.25);
            text-align: center;
            max-width: 520px;
            width: 100%;
            border: 1px solid #c3dbff;
        }
        .logo {
            font-size: 3rem;
            font-weight: 700;
            color: white;
            background: #2c4c7c;
            width: 90px;
            height: 90px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            margin: 0 auto 20px;
            box-shadow: 0 12px 18px -6px #2c4c7c88;
            font-family: Consolas, monospace;
        }
        h1 {
            color: #1a3555;
            font-weight: 500;
            font-size: 2rem;
            margin: 0 0 8px;
            letter-spacing: -0.5px;
        }
        .badge {
            background: #e2edff;
            color: #1f5590;
            padding: 6px 22px;
            border-radius: 40px;
            display: inline-block;
            font-weight: 500;
            font-size: 0.9rem;
            margin-bottom: 28px;
            border: 1px solid #b4d2ff;
        }
        .info {
            background: #eef4ff;
            padding: 22px 26px;
            border-radius: 22px;
            text-align: left;
            font-family: 'Consolas', monospace;
            font-size: 0.95rem;
            color: #1a3550;
            border-left: 6px solid #3d7ec9;
            margin-bottom: 24px;
        }
        .info p {
            margin: 8px 0;
            line-height: 1.6;
        }
        .green { color: #1f6e3a; font-weight: 600; }
        .footer {
            display: flex;
            justify-content: center;
            gap: 16px;
            color: #5e7c9e;
            font-size: 0.85rem;
            margin-top: 16px;
        }
        .footer span {
            background: #d9e6ff;
            padding: 4px 14px;
            border-radius: 30px;
        }
    </style>
</head>
<body>
    <div class='card'>
        <div class='logo'>C#</div>
        <h1>Hello, HTML</h1>
        <div class='badge'>📄 WebBrowser 已加载</div>
        <div class='info'>
            <p><span class='green'>▶</span> 这是可编辑的 HTML 内容</p>
            <p><span class='green'>▶</span> 修改左侧代码并点击运行</p>
            <p><span class='green'>▶</span> 支持完整的 HTML/CSS 语法</p>
        </div>
        <div class='footer'>
            <span>WinForms</span>
            <span>WebBrowser</span>
            <span>Live Preview</span>
        </div>
    </div>
</body>
</html>";

        public MainForm()
        {
            InitializeComponent();
            SetupCustomUI();
            LoadDefaultHtml();
        }

        private void InitializeComponent()
        {
            this.Text = "C# 运行 HTML - WebBrowser 环境";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(26, 37, 51);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
        }

        private void SetupCustomUI()
        {
            // 主容器 - 使用 TableLayoutPanel 进行整体布局
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(26, 37, 51),
                Padding = new Padding(12)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 标题栏
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 内容区
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));  // 状态栏

            // ========== 标题栏 ==========
            var titlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 24, 36),
                Padding = new Padding(15, 0, 15, 0)
            };

            var lblTitle = new Label
            {
                Text = "⚙️ C# 运行 HTML — WebBrowser 环境",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(182, 220, 255),
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblHint = new Label
            {
                Text = "✏️ 编辑 HTML  →  ▶ 运行  →  🌐 实时预览",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(138, 169, 201),
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            titlePanel.Controls.Add(lblTitle);
            titlePanel.Controls.Add(lblHint);

            // ========== 工具栏 ==========
            var toolPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 30, 44),
                Padding = new Padding(10, 6, 10, 6)
            };

            btnRun = CreateButton("▶ 运行 (F5)", Color.FromArgb(46, 108, 176), Color.White);
            btnRun.Click += (s, e) => RunHtml();

            btnReset = CreateButton("↺ 重置示例", Color.FromArgb(58, 78, 100), Color.FromArgb(196, 225, 255));
            btnReset.Click += (s, e) =>
            {
                htmlEditor.Text = DefaultHtml;
                RunHtml();
                UpdateStatus("已恢复默认 HTML");
            };

            btnClear = CreateButton("🗑️ 清空", Color.FromArgb(58, 78, 100), Color.FromArgb(196, 225, 255));
            btnClear.Click += (s, e) =>
            {
                htmlEditor.Text = "";
                webBrowser.DocumentText = "<html><body style='background:#0b1219;'></body></html>";
                UpdateStatus("编辑器已清空");
            };

            // 使用 FlowLayoutPanel 排列按钮
            var buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0)
            };
            buttonFlow.Controls.Add(btnRun);
            buttonFlow.Controls.Add(btnReset);
            buttonFlow.Controls.Add(btnClear);

            toolPanel.Controls.Add(buttonFlow);

            // ========== 内容区: SplitContainer ==========
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(26, 37, 51),
                SplitterDistance = 550,
                Panel1MinSize = 300,
                Panel2MinSize = 300
            };

            // 左侧: 编辑器
            var editorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 18, 25),
                Padding = new Padding(1)
            };

            htmlEditor = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(13, 22, 33),
                ForeColor = Color.FromArgb(224, 239, 255),
                Font = new Font("Consolas", 10.5F),
                BorderStyle = BorderStyle.None,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both,
                AcceptsTab = true,
                DetectUrls = false,
                LanguageOption = RichTextBoxLanguageOptions.DualFont
            };
            // 设置 Tab 键宽度为 4 空格
            htmlEditor.SelectionTabs = new int[] { 20, 40, 60, 80 };

            // 编辑器头部
            var editorHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(22, 36, 51),
                Padding = new Padding(10, 0, 10, 0)
            };
            var lblEditor = new Label
            {
                Text = "📝 HTML 编辑器  (DocumentText)",
                Dock = DockStyle.Left,
                ForeColor = Color.FromArgb(152, 185, 221),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblCharCount = new Label
            {
                Text = "0 字符",
                Dock = DockStyle.Right,
                ForeColor = Color.FromArgb(110, 141, 171),
                Font = new Font("Consolas", 9F),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            editorHeader.Controls.Add(lblEditor);
            editorHeader.Controls.Add(lblCharCount);

            editorPanel.Controls.Add(htmlEditor);
            editorPanel.Controls.Add(editorHeader);

            // 右侧: WebBrowser 预览
            var browserPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 18, 25),
                Padding = new Padding(1)
            };

            // 浏览器头部 (模拟地址栏)
            var browserHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(22, 36, 51),
                Padding = new Padding(10, 0, 10, 0)
            };
            var lblBrowser = new Label
            {
                Text = "🌐 WebBrowser 预览  (实时渲染)",
                Dock = DockStyle.Left,
                ForeColor = Color.FromArgb(152, 185, 221),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblAddr = new Label
            {
                Text = "about:html",
                Dock = DockStyle.Right,
                ForeColor = Color.FromArgb(127, 159, 191),
                Font = new Font("Consolas", 9F),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            browserHeader.Controls.Add(lblBrowser);
            browserHeader.Controls.Add(lblAddr);

            webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                IsWebBrowserContextMenuEnabled = false,
                WebBrowserShortcutsEnabled = true,
                AllowWebBrowserDrop = false,
                ScrollBarsEnabled = true
            };
            // 禁止拖放和默认右键菜单（简化环境）
            webBrowser.AllowNavigation = true;

            browserPanel.Controls.Add(webBrowser);
            browserPanel.Controls.Add(browserHeader);

            splitContainer.Panel1.Controls.Add(editorPanel);
            splitContainer.Panel2.Controls.Add(browserPanel);

            // ========== 状态栏 ==========
            var statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 24, 36),
                Padding = new Padding(15, 0, 15, 0)
            };

            lblStatus = new Label
            {
                Text = "就绪 — 编辑 HTML 后点击「运行」或按 F5",
                Dock = DockStyle.Left,
                ForeColor = Color.FromArgb(110, 141, 171),
                Font = new Font("Consolas", 9F),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTip = new Label
            {
                Text = "💡 F5 运行  |  Ctrl+Enter 运行  |  双击编辑器全选",
                Dock = DockStyle.Right,
                ForeColor = Color.FromArgb(91, 142, 201),
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            statusPanel.Controls.Add(lblStatus);
            statusPanel.Controls.Add(lblTip);

            // 组装主布局
            mainLayout.Controls.Add(titlePanel, 0, 0);
            mainLayout.Controls.Add(toolPanel, 0, 0);  // 将工具栏叠加到标题栏区域不合适，重新调整
            // 重新调整：使用 Panel 包装工具栏和标题
            // 由于 TableLayoutPanel 只有 3 行，我们把标题和工具栏合并到第一行
            // 更简单：修改行数，这里直接调整

            // 重建主布局为 4 行: 标题+工具栏合并为顶部区域
            this.Controls.Remove(mainLayout);
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(26, 37, 51),
                Padding = new Padding(12)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));  // 标题+工具栏
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 内容区
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));  // 状态栏

            // 顶部区域容器
            var topContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 24, 36)
            };
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 50;
            toolPanel.Dock = DockStyle.Fill;
            topContainer.Controls.Add(toolPanel);
            topContainer.Controls.Add(titlePanel);

            mainLayout.Controls.Add(topContainer, 0, 0);
            mainLayout.Controls.Add(splitContainer, 0, 1);
            mainLayout.Controls.Add(statusPanel, 0, 2);

            this.Controls.Add(mainLayout);

            // ========== 事件绑定 ==========
            // 编辑器文本变化时更新字符计数
            htmlEditor.TextChanged += (s, e) =>
            {
                lblCharCount.Text = $"{htmlEditor.Text.Length} 字符";
                // 可选：自动延迟运行（注释掉，保持手动运行）
                // RestartDebounce();
            };

            // 键盘快捷键
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    e.SuppressKeyPress = true;
                    RunHtml();
                }
                if (e.Control && e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    RunHtml();
                }
            };

            // 初始化防抖计时器（可选自动运行，这里保留以备扩展）
            debounceTimer = new Timer { Interval = 500 };
            debounceTimer.Tick += (s, e) =>
            {
                debounceTimer.Stop();
                RunHtml();
            };
        }

        private Button CreateButton(string text, Color backColor, Color foreColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(130, 34),
                Margin = new Padding(0, 0, 10, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(70, 110, 160) }
            };
        }

        private void LoadDefaultHtml()
        {
            htmlEditor.Text = DefaultHtml;
            RunHtml();
            UpdateStatus("示例 HTML 已加载 — 点击「运行」或按 F5 重新渲染");
        }

        private void RunHtml()
        {
            try
            {
                string html = htmlEditor.Text;
                if (string.IsNullOrWhiteSpace(html))
                {
                    webBrowser.DocumentText = "<html><body style='background:#0b1219;color:#5e7c9e;display:flex;align-items:center;justify-content:center;height:100vh;font-family:Segoe UI;'><h2>编辑器为空</h2></body></html>";
                    UpdateStatus("编辑器为空 — 请粘贴 HTML 代码");
                    return;
                }

                // 核心：将 HTML 内容设置到 WebBrowser
                webBrowser.DocumentText = html;
                UpdateStatus($"✔ 已运行 — {DateTime.Now:HH:mm:ss}  |  {html.Length} 字符");
            }
            catch (Exception ex)
            {
                UpdateStatus($"⚠ 错误: {ex.Message}");
                MessageBox.Show($"运行 HTML 时出错:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
                lblStatus.Text = message;
        }

        // 可选：自动运行（使用防抖）— 默认不启用，留给用户选择
        private void RestartDebounce()
        {
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        // 程序入口
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}