using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    class ErrorDialogueViewModel : Screen {
        public List<string> errors = new();

        public string Title { get; set; }
        private string _header = "";
        public string Header { get { return _header; } set { _header = value; } }

        private string _footer = "";
        public string Footer { get { return _footer; } set { _footer = value; } }

        public string errFile = "";

        /// <summary>
        /// Instantiate the ErrorDialogue window
        /// </summary>
        /// <param name="title">Window title</param>
        /// <param name="errors">List of errors to display</param>
        /// <param name="header">Header message</param>
        /// <param name="footer">Footer message</param>
        /// <param name="errFile">Path to error file</param>
        public ErrorDialogueViewModel(string title, List<string> errors, string header, string footer, string errFile) {
            Title = title;
            this.errors = errors;
            Header = header;
            Footer = footer;
            this.errFile = errFile;
        }

        public void Close() {
            TryCloseAsync();
        }
    }
}
