using Caliburn.Micro;
using FSvBSCustomCloneUtility.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class FAQWindowViewModel : Screen {
        public List<FAQItem> FAQ { get; }

        public FAQWindowViewModel(List<FAQItem> faq) {
            FAQ = faq;
        }

        public void NewIssue() {
            Process.Start(new ProcessStartInfo {
                FileName = $"https://github.com/Exkywor/FSvBSCustomCloneUtility/issues/new?assignees=&labels=&template=headmorph-bug-report.md&title=",
                UseShellExecute = true
            });
        }

        public void Close() {
            TryCloseAsync();
        }
    }
}
