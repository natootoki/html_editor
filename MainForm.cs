using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SimpleHtmlEditor
{
    public class MainForm : Form
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

            // 初期HTMLのロード
            LoadInitialHtml();

            // ボタンの作成
            InitializeButtons();
        }

        private void LoadInitialHtml()
        {
            string initialHtml = HtmlEditorHelper.GetInitialHtml();
            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.html");
            File.WriteAllText(htmlPath, initialHtml);
            webBrowser.Navigate(htmlPath);
        }

        private void InitializeButtons()
        {
            // ボタンの共通スタイル
            var buttonHeight = 40; // 高さを設定
            var buttonMargin = new Padding(10); // ボタン間の余白を設定

            // 保存ボタン
            saveButton = new Button
            {
                Text = "保存",
                Height = buttonHeight,
                Dock = DockStyle.Bottom,
                Margin = buttonMargin,
                BackColor = System.Drawing.Color.LightBlue, // 背景色を設定
                ForeColor = System.Drawing.Color.Black, // 文字色を設定
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold) // フォントサイズとスタイルを設定
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            // 上書き保存ボタン
            saveOverwriteButton = new Button
            {
                Text = "上書き保存",
                Height = buttonHeight,
                Dock = DockStyle.Bottom,
                Margin = buttonMargin,
                BackColor = System.Drawing.Color.LightGreen, // 背景色を設定
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold)
            };
            saveOverwriteButton.Click += SaveOverwriteButton_Click;
            this.Controls.Add(saveOverwriteButton);

            // 「クリップボード画像を貼り付ける」ボタン
            pasteImageButton = new Button
            {
                Text = "クリップボード画像を貼り付ける",
                Height = buttonHeight,
                Dock = DockStyle.Bottom,
                Margin = buttonMargin,
                BackColor = System.Drawing.Color.LightPink,
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold)
            };
            pasteImageButton.Click += PasteImageButton_Click;
            this.Controls.Add(pasteImageButton);

            // 「HTMLファイルを読み込む」ボタン
            loadHtmlButton = new Button
            {
                Text = "HTMLファイルを読み込む",
                Height = buttonHeight,
                Dock = DockStyle.Bottom,
                Margin = buttonMargin,
                BackColor = System.Drawing.Color.LightYellow,
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold)
            };
            loadHtmlButton.Click += LoadHtmlButton_Click;
            this.Controls.Add(loadHtmlButton);
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.Document != null)
            {
                webBrowser.Document.Body.KeyDown += Body_KeyDown;
            }
        }

        // KeyDownイベントでCtrl+S、Ctrl+V、Ctrl+Oを処理
        private void Body_KeyDown(object sender, HtmlElementEventArgs e)
        {
            if (e.CtrlKeyPressed)
            {
                if (e.KeyPressedCode == 83) // Ctrl+S
                {
                    SaveOverwriteButton_Click(this, EventArgs.Empty);
                    e.ReturnValue = false; // デフォルト動作をキャンセル
                }
                else if (e.KeyPressedCode == 86 && Clipboard.ContainsImage()) // Ctrl+V
                {
                    PasteImageFromClipboard();
                    e.ReturnValue = false; // デフォルト動作をキャンセル
                }
                else if (e.KeyPressedCode == 79) // Ctrl+O
                {
                    LoadHtmlButton_Click(this, EventArgs.Empty);
                    e.ReturnValue = false; // デフォルト動作をキャンセル
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string htmlContent = HtmlEditorHelper.GetHtmlContent(webBrowser);
            if (HtmlEditorHelper.SaveHtmlAsFile(htmlContent, out currentFilePath))
            {
                this.Text = string.Format("シンプルなHTMLエディタ - {0}", currentFilePath);
                MessageBox.Show("HTMLが保存されました！");
            }
        }


        private void SaveOverwriteButton_Click(object sender, EventArgs e)
        {
            string htmlContent = HtmlEditorHelper.GetHtmlContent(webBrowser);
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                HtmlEditorHelper.SaveHtmlToFile(htmlContent, currentFilePath);
                this.Text = string.Format("シンプルなHTMLエディタ - {0}", currentFilePath);
                MessageBox.Show("現在のファイルに上書き保存しました！");
            }
            else
            {
                SaveButton_Click(sender, e);
            }
        }

        private void PasteImageButton_Click(object sender, EventArgs e)
        {
            PasteImageFromClipboard();
        }

        private void PasteImageFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                string base64Image = ClipboardHelper.GetBase64ImageFromClipboard();
                HtmlEditorHelper.InsertBase64Image(webBrowser, base64Image);
            }
            else
            {
                MessageBox.Show("クリップボードに画像がありません。");
            }
        }

        private void LoadHtmlButton_Click(object sender, EventArgs e)
        {
            string loadedHtml;
            if (HtmlEditorHelper.LoadHtmlFromFile(out loadedHtml, out currentFilePath))
            {
                HtmlEditorHelper.LoadHtmlToWebBrowser(webBrowser, loadedHtml);
                this.Text = string.Format("シンプルなHTMLエディタ - {0}", currentFilePath);
            }
        }
    }
}
