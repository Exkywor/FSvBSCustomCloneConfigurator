using Microsoft.Win32;

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
    }
}
