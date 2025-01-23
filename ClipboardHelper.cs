using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SimpleHtmlEditor
{
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
}
