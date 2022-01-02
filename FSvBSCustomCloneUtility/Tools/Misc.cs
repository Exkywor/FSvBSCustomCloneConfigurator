using Microsoft.Win32;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Misc static tools
    /// </summary>
    public static class Misc {
        /// <summary>
        /// Prompt the user for a file
        /// </summary>
        /// <param name="filter">File filter</param>
        /// <param name="title">Title of the file dialog</param>
        /// <returns></returns>
        public static string PromptForFile(string filter, string title = "") {
            OpenFileDialog dlg = new OpenFileDialog { Title = title };
            dlg.Filter = filter;

            if (dlg.ShowDialog() != true) {
                return null;
            } else {
                return dlg.FileName;
            }
        }

        /// <summary>
        /// Measure the width and height of an input text based on the input typeFace and input fontSize
        /// </summary>
        /// <param name="text">Input text to measure</param>
        /// <param name="typeFace">Input type face</param>
        /// <param name="fontSize">Input fontSize</param>
        /// <returns>The Size of the measured text</returns>
        public static Size MeasureString(string text,  string typeFace, int fontSize) {
            FormattedText formattedText = new(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(typeFace),
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
