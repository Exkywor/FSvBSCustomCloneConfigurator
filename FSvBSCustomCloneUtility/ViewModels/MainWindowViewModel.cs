using Caliburn.Micro;
using FSvBSCustomCloneUtility.InterfacesAndClasses;
using FSvBSCustomCloneUtility.Models;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Path = System.IO.Path;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class MainWindowViewModel : Conductor<PropertyChangedBase> {
        private static IWindowManager windowManager = new WindowManager();
        private List<PropertyChangedBase> controls = new();
        private List<FAQItem> faq = new();

        public PropertyChangedBase MorphControl { get; set; }
        public StatusBar StatusBar { get; set; }


        public void VisitME3() {
            VisitModSite(MEGame.ME3);
        }

        public void VisitLE3() {
            VisitModSite(MEGame.LE3);
        }

        public void VisitModSite(MEGame game) {
            string gameSite = game.IsLEGame() ? "masseffectlegendaryedition" : "masseffect3";
            string modNumber = game.IsLEGame() ? "850" : "975";
            Process.Start(new ProcessStartInfo {
                FileName = @$"http://www.nexusmods.com/{gameSite}/mods/{modNumber}",
                UseShellExecute = true
            });
        }

        public MainWindowViewModel() {
            initCoreLib();

            // Create views and adds them to the observers
            StatusBar = new StatusBarViewModel();
            MorphControl = new MorphControlViewModel(StatusBar);
            controls.Add(StatusBar);
            controls.Add(MorphControl);

            LoadFAQ();
            LoadViewAsync();
        } 

        /// <summary>
        /// Initialize Legendary Explorer Core Library
        /// </summary>
        private static void initCoreLib() {
            static void packageSaveFailed(string message) {
                // I'm not sure if this requires ui thread since it's win32 but i'll just make sure
                Application.Current.Dispatcher.Invoke(() => {
                    windowManager.ShowDialogAsync(message);
                });
            }
 
            LegendaryExplorerCoreLib.InitLib(TaskScheduler.FromCurrentSynchronizationContext(), packageSaveFailed);
        }

        private async Task LoadViewAsync() {
            foreach (PropertyChangedBase observer in controls) {
                await ActivateItemAsync(observer);
            }
        }

        /// <summary>
        /// Load the FAQ json to a dictionary
        /// Done here to avoid rereading every time you launch the help window
        /// </summary>
        private void LoadFAQ() {
            using StreamReader sr = new("../../../resources/FAQ.json");
            string faqString = sr.ReadToEnd();
            Dictionary<string, string> faqParsed = JsonSerializer.Deserialize<Dictionary<string, string>>(faqString);
            foreach (string i in faqParsed.Keys) {
                faq.Add(new FAQItem(i, faqParsed[i]));
            }
        }

        public void LaunchInfoWindow() {

        }
        public void LaunchHelpWindow() {
            windowManager.ShowWindowAsync(new FAQWindowViewModel(faq),
                null, null);
        }
    }
}
