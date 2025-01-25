using System;
using System.IO;
using System.Windows.Forms;

namespace SimpleHtmlEditor
{
    public static class HtmlEditorHelper
    {
        public static string GetInitialHtml()
        {
            return @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>HTML Editor</title>
                <style>
                    body { font-family: Arial, sans-serif; padding: 10px; }
                    #editor { 
                        border: 1px solid #ddd; 
                        min-height: 300px; 
                        padding: 10px; 
                        background-color: #ddd; /* 背景色を設定 */
                    }
                </style>
            </head>
            <body>
                <div id='editor' contenteditable='true'>
                </div>
            </body>
            </html>";
        }

        public static string GetHtmlContent(WebBrowser webBrowser)
        {
            if (webBrowser.Document != null && webBrowser.Document.GetElementsByTagName("html").Count > 0)
            {
                return webBrowser.Document.GetElementsByTagName("html")[0].OuterHtml;
            }
            return string.Empty;
        }

        public static bool SaveHtmlAsFile(string htmlContent, out string filePath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "HTML Files|*.html",
                Title = "HTMLファイルを保存"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, htmlContent);
                filePath = saveFileDialog.FileName;
                return true;
            }

            filePath = null;
            return false;
        }

        public static void SaveHtmlToFile(string htmlContent, string filePath)
        {
            File.WriteAllText(filePath, htmlContent);
        }

        public static bool LoadHtmlFromFile(out string htmlContent, out string filePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "HTML Files|*.html",
                Title = "HTMLファイルを開く"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                htmlContent = File.ReadAllText(filePath);
                return true;
            }

            htmlContent = null;
            filePath = null;
            return false;
        }

        public static void LoadHtmlToWebBrowser(WebBrowser webBrowser, string htmlContent)
        {
            string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_editor.html");
            File.WriteAllText(tempPath, htmlContent);
            webBrowser.Navigate(tempPath);
        }

        public static void InsertBase64Image(WebBrowser webBrowser, string base64Image)
        {
            webBrowser.Document.InvokeScript("eval", new object[]
            {
                "document.getElementById('editor').innerHTML += '<img src=\"data:image/png;base64," + base64Image + "\">'"
            });
        }
    }
}
