using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    public static class Misc {
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
