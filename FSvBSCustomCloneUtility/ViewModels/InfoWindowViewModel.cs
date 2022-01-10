using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class InfoWindowViewModel : Screen {
        public string FileVersion { get; set; }

        public InfoWindowViewModel() {
            FileVersion = $"Version {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}";
        }

        public void OpenGitHub() {
            Process.Start(new ProcessStartInfo {
                FileName = $"https://github.com/Exkywor/FSvBSCustomCloneUtility/",
                UseShellExecute = true
            });
        }

        public void Close() {
            TryCloseAsync();
        }
    }
}
