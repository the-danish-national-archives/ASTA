using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rigsarkiv.Athena.Extensions
{
    /// <summary>
    /// Rich TextBox Extension
    /// </summary>
    public static class RichTextBoxExtension
    {
        /// <summary>
        /// Append colored Text 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.AppendText(Environment.NewLine);
            box.SelectionColor = box.ForeColor;
            box.ScrollToCaret();
            box.ResumeLayout();
        }
    }
}
