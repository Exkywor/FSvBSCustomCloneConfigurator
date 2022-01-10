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

        public void OpenSite(string target) {
            string url = "";

            switch (target) {
                case "GH":
                    url = "https://github.com/Exkywor/FSvBSCustomCloneUtility/";
                    break;
                case "ME3":
                    url = "https://www.nexusmods.com/masseffect3/mods/975";
                    break;
                case "LE3":
                    url = "https://www.nexusmods.com/masseffectlegendaryedition/mods/850";
                    break;
                case "LEX":
                    url = "https://github.com/ME3Tweaks/LegendaryExplorer/blob/Beta/LegendaryExplorer/LegendaryExplorerCore/";
                    break;
                case "M3":
                    url = "https://github.com/ME3Tweaks/ME3TweaksModManager/tree/master/MassEffectModManagerCore";
                    break;
                case "CM":
                    url = "https://caliburnmicro.com/";
                    break;
                case "FA":
                    url = "https://fontawesome.com/";
                    break;
                case "AU":
                    url = "https://benruehl.github.io/adonis-ui/";
                    break;
                case "MW":
                    url = "https://www.nuget.org/packages/Microsoft-WindowsAPICodePack-Shell/";
                    break;
            }

            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            });
        }

        public void Close() {
            TryCloseAsync();
        }
    }
}
