using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DiffLite;

/// <summary>
/// Diff-Lite 主窗口 — 轻量级代码比对工具
/// </summary>
public partial class MainWindow : Window
{
    private bool _isDarkMode = true;
    private string? _leftFilePath;
    private string? _rightFilePath;

    // 行高亮渲染器
    private DiffLineHighlighter? _leftHighlighter;
    private DiffLineHighlighter? _rightHighlighter;

    // 语言文件扩展名 → AvalonEdit 语法高亮名称映射
    private static readonly Dictionary<string, string> LangMap = new()
    {
        [".js"] = "JavaScript", [".jsx"] = "JavaScript", [".mjs"] = "JavaScript",
        [".ts"] = "C#", // TypeScript 用 C# 风格
        [".json"] = "C#", [".html"] = "XML", [".htm"] = "XML",
        [".css"] = "CSS", [".scss"] = "CSS",
        [".md"] = "Markdown",
        [".xml"] = "XML", [".yaml"] = "XML", [".yml"] = "XML",
        [".py"] = "Python", [".rb"] = "Python", [".go"] = "C#", [".rs"] = "C#",
        [".java"] = "Java", [".kt"] = "C#", [".swift"] = "C#",
        [".c"] = "C#", [".cpp"] = "C#", [".h"] = "C#", [".hpp"] = "C#",
        [".cs"] = "C#", [".php"] = "PHP",
        [".sql"] = "SQL",
        [".sh"] = "PowerShell", [".ps1"] = "PowerShell",
        [".txt"] = "", [".log"] = "", [".csv"] = "",
        [".toml"] = "", [".ini"] = "", [".cfg"] = "",
    };

    public MainWindow()
    {
        InitializeComponent();
        InitializeEditors();
        SetupKeyboardShortcuts();
        UpdateStatus("就绪 — 点击上方按钮打开文件开始比对");

        // 设置编辑器深色主题
        ApplyDarkTheme();
    }

    private void InitializeEditors()
    {
        // 配置左侧编辑器
        leftEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(0x99, 0x26, 0x4F, 0x78));
        leftEditor.TextArea.SelectionForeground = null;
        leftEditor.Options.HighlightCurrentLine = true;

        // 配置右侧编辑器
        rightEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(0x99, 0x26, 0x4F, 0x78));
        rightEditor.TextArea.SelectionForeground = null;
        rightEditor.Options.HighlightCurrentLine = true;

        // 中文文本
        leftEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Clear();
        rightEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Clear();

        // 初始化高亮渲染器
        _leftHighlighter = new DiffLineHighlighter(leftEditor.TextArea.TextView);
        leftEditor.TextArea.TextView.BackgroundRenderers.Add(_leftHighlighter);
        _rightHighlighter = new DiffLineHighlighter(rightEditor.TextArea.TextView);
        rightEditor.TextArea.TextView.BackgroundRenderers.Add(_rightHighlighter);
    }

    private void SetupKeyboardShortcuts()
    {
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenLeft_Click(null, null!);
                e.Handled = true;
            }
            else if (e.Key == Key.O && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenRight_Click(null, null!);
                e.Handled = true;
            }
            else if (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Compare_Click(null, null!);
                e.Handled = true;
            }
            else if (e.Key == Key.R && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                Clear_Click(null, null!);
                e.Handled = true;
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleTheme_Click(null, null!);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && diffResultPanel.Visibility == Visibility.Visible)
            {
                HideDiffResult();
                e.Handled = true;
            }
        };
    }

    // ========== 主题切换 ==========

    private void ApplyDarkTheme()
    {
        // 设置为深色
        var darkBg = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
        var darkFg = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
        var lineNumColor = new SolidColorBrush(Color.FromRgb(0x85, 0x85, 0x85));

        leftEditor.Background = darkBg;
        leftEditor.Foreground = darkFg;
        leftEditor.LineNumbersForeground = lineNumColor;

        rightEditor.Background = darkBg;
        rightEditor.Foreground = darkFg;
        rightEditor.LineNumbersForeground = lineNumColor;
    }

    private void ToggleTheme_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            _isDarkMode = !_isDarkMode;
            btnTheme.Content = _isDarkMode ? "🌙" : "☀️";
            txtThemeMode.Text = _isDarkMode ? "深色模式" : "浅色模式";

            if (_isDarkMode)
            {
                var darkBg = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));
                leftEditor.Background = darkBg;
                rightEditor.Background = darkBg;
            }
            else
            {
                var lightBg = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3));
                leftEditor.Background = lightBg;
                rightEditor.Background = lightBg;
            }
        }
        catch (Exception ex)
        {
            ShowError("切换主题失败", ex);
        }
    }

    // ========== 文件操作 ==========

    private void OpenLeft_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择旧文件（左侧）",
                Filter = "代码与文本文件|*.js;*.ts;*.jsx;*.tsx;*.json;*.html;*.htm;*.css;*.scss;*.less;*.vue;*.svelte;*.py;*.rb;*.go;*.rs;*.java;*.kt;*.swift;*.c;*.cpp;*.h;*.hpp;*.cs;*.php;*.pl;*.sh;*.bash;*.ps1;*.bat;*.cmd;*.xml;*.yaml;*.yml;*.toml;*.ini;*.cfg;*.md;*.txt;*.log;*.csv;*.tsv;*.sql;*.r;*.dart;*.lua;*.ex;*.exs;*.hs;*.scala;*.clj;*.coffee|所有文件|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadFile(dialog.FileName, isLeft: true);
            }
        }
        catch (Exception ex)
        {
            ShowError("打开左侧文件失败", ex);
        }
    }

    private void OpenRight_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择新文件（右侧）",
                Filter = "代码与文本文件|*.js;*.ts;*.jsx;*.tsx;*.json;*.html;*.htm;*.css;*.scss;*.less;*.vue;*.svelte;*.py;*.rb;*.go;*.rs;*.java;*.kt;*.swift;*.c;*.cpp;*.h;*.hpp;*.cs;*.php;*.pl;*.sh;*.bash;*.ps1;*.bat;*.cmd;*.xml;*.yaml;*.yml;*.toml;*.ini;*.cfg;*.md;*.txt;*.log;*.csv;*.tsv;*.sql;*.r;*.dart;*.lua;*.ex;*.exs;*.hs;*.scala;*.clj;*.coffee|所有文件|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadFile(dialog.FileName, isLeft: false);
            }
        }
        catch (Exception ex)
        {
            ShowError("打开右侧文件失败", ex);
        }
    }

    private void LoadFile(string filePath, bool isLeft)
    {
        try
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"文件不存在：\n{filePath}", "Diff-Lite 错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 检查是否为二进制文件
            if (IsBinaryFile(filePath))
            {
                MessageBox.Show("所选文件似乎是二进制文件或无法识别的格式，\n请选择纯文本或代码文件。",
                    "无法打开文件", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 读取内容
            var content = File.ReadAllText(filePath, Encoding.UTF8);
            var fileName = Path.GetFileName(filePath);

            if (isLeft)
            {
                _leftFilePath = filePath;
                leftEditor.Document.Text = content;
                txtLeftFile.Text = $"  —  {fileName}";
                txtLeftFile.ToolTip = filePath;
                ApplySyntaxHighlighting(leftEditor, filePath);
            }
            else
            {
                _rightFilePath = filePath;
                rightEditor.Document.Text = content;
                txtRightFile.Text = $"  —  {fileName}";
                txtRightFile.ToolTip = filePath;
                ApplySyntaxHighlighting(rightEditor, filePath);
            }

            HideDiffResult();
            var side = isLeft ? "左侧" : "右侧";
            UpdateStatus($"已加载 {side} 文件：{fileName}");
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("无权读取该文件，请检查文件权限。", "读取失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (IOException ex)
        {
            MessageBox.Show($"读取文件时发生错误：\n{ex.Message}", "读取失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>检查文件是否为二进制文件</summary>
    private static bool IsBinaryFile(string filePath)
    {
        try
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
            {
                var name = Path.GetFileName(filePath).ToLower();
                if (name is "makefile" or "dockerfile" or "gemfile")
                    return false;
            }

            // 读取前 1024 字节检查空字节
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (fs.Length > 1024 * 1024 * 10) // 超过 10MB 警告
                return true;

            var buffer = new byte[Math.Min(1024, fs.Length)];
            fs.Read(buffer, 0, buffer.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0) return true;
                if (buffer[i] < 32 && buffer[i] != 9 && buffer[i] != 10 && buffer[i] != 13)
                    return true;
            }
            return false;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>应用语法高亮</summary>
    private static void ApplySyntaxHighlighting(TextEditor editor, string filePath)
    {
        try
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (LangMap.TryGetValue(ext, out var lang) && !string.IsNullOrEmpty(lang))
            {
                editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(lang);
            }
            else
            {
                editor.SyntaxHighlighting = null;
            }
        }
        catch
        {
            editor.SyntaxHighlighting = null;
        }
    }

    // ========== 比对功能 ==========

    private void Compare_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            var leftText = leftEditor.Document.Text;
            var rightText = rightEditor.Document.Text;

            if (string.IsNullOrEmpty(leftText) && string.IsNullOrEmpty(rightText))
            {
                ShowError("两侧均为空", "请先打开文件或输入内容后再进行比对。");
                return;
            }

            if (leftText == rightText)
            {
                ShowDiffResult("✅ 两侧内容完全相同，无差异。");
                ClearDiffHighlights();
                return;
            }

            // 使用 DiffPlex 进行比对
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diffResult = diffBuilder.BuildDiffModel(leftText, rightText);

            // 统计差异
            int added = 0, removed = 0, modified = 0;
            foreach (var line in diffResult.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted: added++; break;
                    case ChangeType.Deleted: removed++; break;
                    case ChangeType.Modified: modified++; break;
                }
            }

            // 在编辑器中标记差异
            HighlightDiffInEditor(leftEditor, diffResult, isLeft: true);
            HighlightDiffInEditor(rightEditor, diffResult, isLeft: false);

            var total = added + removed + modified;
            var sb = new StringBuilder();
            sb.Append("🔍 比对完成 — ");
            if (added > 0) sb.Append($"🟢 +{added} 行（新增） ");
            if (removed > 0) sb.Append($"🔴 -{removed} 行（删除） ");
            if (modified > 0) sb.Append($"🟡 ~{modified} 行（修改） ");

            ShowDiffResult(sb.ToString().Trim());
            UpdateStatus($"比对完成 — 共 {total} 处差异");
        }
        catch (Exception ex)
        {
            ShowError("比对失败", ex);
        }
    }

    private void HighlightDiffInEditor(TextEditor editor, DiffPaneModel diffResult, bool isLeft)
    {
        try
        {
            var highlighter = isLeft ? _leftHighlighter : _rightHighlighter;
            if (highlighter == null) return;

            var highlights = new Dictionary<int, Brush>();
            var addedBg = new SolidColorBrush(Color.FromArgb(0x60, 0x4E, 0xC9, 0xB0));
            var removedBg = new SolidColorBrush(Color.FromArgb(0x60, 0xF1, 0x4C, 0x4C));
            var modifiedBg = new SolidColorBrush(Color.FromArgb(0x60, 0xCC, 0xA7, 0x00));

            int lineNum = 1;
            foreach (var line in diffResult.Lines)
            {
                ChangeType relevantType;
                if (isLeft)
                {
                    relevantType = line.Type is ChangeType.Deleted or ChangeType.Unchanged
                        ? line.Type : ChangeType.Unchanged;
                }
                else
                {
                    relevantType = line.Type is ChangeType.Inserted or ChangeType.Unchanged
                        ? line.Type : ChangeType.Unchanged;
                }

                if (relevantType != ChangeType.Unchanged && lineNum <= editor.Document.LineCount)
                {
                    var brush = relevantType switch
                    {
                        ChangeType.Inserted => addedBg,
                        ChangeType.Deleted => removedBg,
                        ChangeType.Modified => modifiedBg,
                        _ => null
                    };

                    if (brush != null)
                    {
                        highlights[lineNum] = brush;
                    }
                }
                lineNum++;
            }

            highlighter.SetHighlights(highlights);
            editor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"高亮失败: {ex.Message}");
        }
    }

    private void ClearDiffHighlights()
    {
        _leftHighlighter?.Clear();
        _rightHighlighter?.Clear();
        leftEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        rightEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    // ========== 清空 ==========

    private void Clear_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            leftEditor.Document.Text = "";
            rightEditor.Document.Text = "";
            _leftFilePath = null;
            _rightFilePath = null;
            txtLeftFile.Text = "  —  未选择文件";
            txtRightFile.Text = "  —  未选择文件";

            ClearDiffHighlights();
            HideDiffResult();
            UpdateStatus("已清空 — 点击上方按钮打开文件开始比对");
        }
        catch (Exception ex)
        {
            ShowError("清空失败", ex);
        }
    }

    // ========== UI 辅助方法 ==========

    private void UpdateStatus(string message)
    {
        txtStatus.Text = message;
    }

    private void ShowDiffResult(string message)
    {
        txtDiffResult.Text = message;
        diffResultPanel.Height = 30;
        diffResultPanel.Visibility = Visibility.Visible;
    }

    private void HideDiffResult()
    {
        diffResultPanel.Height = 0;
        diffResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        UpdateStatus($"❌ {title}：{message}");
    }

    private void ShowError(string title, Exception ex)
    {
        ShowError(title, ex.Message);
    }

    // ========== 其他事件 ==========

    private void Exit_Click(object? sender, RoutedEventArgs? e)
    {
        Close();
    }

    private void About_Click(object? sender, RoutedEventArgs? e)
    {
        MessageBox.Show(
            "Diff-Lite v2.0.0\n\n轻量级代码比对工具\n适用于 Windows 系统\n\n纯中文界面 · 深色模式 · WPF 原生引擎",
            "关于 Diff-Lite",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

// ========== 辅助类 ==========

/// <summary>行高亮渲染器 — 使用 VisualLine API 兼容 AvalonEdit 6.x</summary>
public class DiffLineHighlighter : IBackgroundRenderer
{
    private readonly TextView _textView;
    private readonly Dictionary<int, Brush> _highlights = new();

    public DiffLineHighlighter(TextView textView)
    {
        _textView = textView;
    }

    public void SetHighlights(Dictionary<int, Brush> highlights)
    {
        _highlights.Clear();
        foreach (var kv in highlights) _highlights[kv.Key] = kv.Value;
    }

    public void Clear()
    {
        _highlights.Clear();
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        foreach (var (lineNum, brush) in _highlights)
        {
            var visualLine = textView.GetVisualLine(lineNum);
            if (visualLine != null)
            {
                var rect = new Rect(0, visualLine.VisualTop, textView.ActualWidth, visualLine.Height);
                drawingContext.DrawRectangle(brush, null, rect);
            }
        }
    }

    public KnownLayer Layer => KnownLayer.Background;
}
