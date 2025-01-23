using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

class MainForm : Form
{
    private WebBrowser webBrowser;
    private Button saveButton;
    private Button saveOverwriteButton;
    private Button pasteImageButton;
    private Button loadHtmlButton;
    private string currentFilePath; // 現在のHTMLファイルパスを保持

    public MainForm()
    {
        // フォームの設定
        this.Text = "シンプルなHTMLエディタ";
        this.Width = 800;
        this.Height = 600;

        // WebBrowserコントロールを追加
        webBrowser = new WebBrowser
        {
            Dock = DockStyle.Fill,
            ScriptErrorsSuppressed = true // JavaScriptエラーを無視
        };
        webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
        this.Controls.Add(webBrowser);

        // HTMLを初期化
        string initialHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>HTML Editor</title>
            <style>
                body { font-family: Arial, sans-serif; padding: 10px; }
                #editor { border: 1px solid #ddd; min-height: 300px; padding: 10px; }
            </style>
        </head>
        <body>
            <div id='editor' contenteditable='true'>
            </div>
        </body>
        </html>";
        currentFilePath = null; // 初期状態ではファイル未指定
        string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.html");
        File.WriteAllText(htmlPath, initialHtml);
        webBrowser.Navigate(htmlPath);

        // 保存ボタンを追加
        saveButton = new Button
        {
            Text = "保存",
            Dock = DockStyle.Bottom
        };
        saveButton.Click += SaveButton_Click;
        this.Controls.Add(saveButton);

        // 上書き保存ボタンを追加
        saveOverwriteButton = new Button
        {
            Text = "上書き保存",
            Dock = DockStyle.Bottom
        };
        saveOverwriteButton.Click += SaveOverwriteButton_Click;
        this.Controls.Add(saveOverwriteButton);

        // 「クリップボード画像を貼り付ける」ボタンを追加
        pasteImageButton = new Button
        {
            Text = "クリップボード画像を貼り付ける",
            Dock = DockStyle.Bottom
        };
        pasteImageButton.Click += PasteImageButton_Click;
        this.Controls.Add(pasteImageButton);

        // 「HTMLファイルを読み込む」ボタンを追加
        loadHtmlButton = new Button
        {
            Text = "HTMLファイルを読み込む",
            Dock = DockStyle.Bottom
        };
        loadHtmlButton.Click += LoadHtmlButton_Click;
        this.Controls.Add(loadHtmlButton);
    }

    // WebBrowserのDocumentが読み込まれた際にイベントを登録
    private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
        if (webBrowser.Document != null)
        {
            webBrowser.Document.Body.KeyDown += Body_KeyDown;
        }
    }

    // KeyDownイベントでCtrl+SやCtrl+Vを処理
    private void Body_KeyDown(object sender, HtmlElementEventArgs e)
    {
        if (e.CtrlKeyPressed)
        {
            if (e.KeyPressedCode == 83) // 83は"S"のキーコード
            {
                SaveOverwriteButton_Click(this, EventArgs.Empty);
                e.ReturnValue = false; // デフォルト動作をキャンセル
            }
            else if (e.KeyPressedCode == 86) // 86は"V"のキーコード
            {
                if (Clipboard.ContainsImage()) // クリップボードに画像がある場合のみ処理
                {
                    PasteImageFromClipboard();
                    e.ReturnValue = false; // デフォルト動作をキャンセル
                }
            }
        }
    }

    // 保存ボタンのクリックイベント
    private void SaveButton_Click(object sender, EventArgs e)
    {
        if (webBrowser.Document != null)
        {
            var htmlContent = webBrowser.Document.GetElementsByTagName("html")[0].OuterHtml;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "HTML Files|*.html",
                Title = "HTMLファイルを保存"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, htmlContent);
                currentFilePath = saveFileDialog.FileName; // 保存したファイルを現在のファイルとして設定
                MessageBox.Show("HTMLが保存されました！");
            }
        }
    }

    // 上書き保存ボタンのクリックイベント
    private void SaveOverwriteButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(currentFilePath) && webBrowser.Document != null)
        {
            // 現在のファイルに上書き保存
            var htmlContent = webBrowser.Document.GetElementsByTagName("html")[0].OuterHtml;
            File.WriteAllText(currentFilePath, htmlContent);
            MessageBox.Show("現在のファイルに上書き保存しました！");
        }
        else
        {
            // 現在のファイルが指定されていない場合、通常の保存機能を呼び出す
            SaveButton_Click(sender, e);
        }
    }

    // クリップボード画像を貼り付けるボタンのクリックイベント
    private void PasteImageButton_Click(object sender, EventArgs e)
    {
        PasteImageFromClipboard();
    }

    // クリップボード画像をHTMLエディタに貼り付け
    private void PasteImageFromClipboard()
    {
        string base64Image = ClipboardHelper.GetBase64ImageFromClipboard();
        if (!string.IsNullOrEmpty(base64Image))
        {
            webBrowser.Document.InvokeScript("eval", new object[]
            {
                "document.getElementById('editor').innerHTML += '<img src=\"data:image/png;base64," + base64Image + "\">'"
            });
        }
        else
        {
            MessageBox.Show("クリップボードに画像がありません。");
        }
    }

    // HTMLファイルを読み込むボタンのクリックイベント
    private void LoadHtmlButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "HTML Files|*.html",
            Title = "HTMLファイルを開く"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            // HTMLファイルの内容を読み込み、エディタに表示
            currentFilePath = openFileDialog.FileName;
            string htmlContent = File.ReadAllText(currentFilePath);
            string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_editor.html");
            File.WriteAllText(tempPath, htmlContent);
            webBrowser.Navigate(tempPath);
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

// クリップボードヘルパークラス
public static class ClipboardHelper
{
    public static string GetBase64ImageFromClipboard()
    {
        if (Clipboard.ContainsImage())
        {
            Image image = Clipboard.GetImage();
            if (image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }
        return null;
    }
}
