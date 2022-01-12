using Caliburn.Micro;
using FSvBSCustomCloneUtility.InterfacesAndClasses;
using FSvBSCustomCloneUtility.Models;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using Serilog;
using System;
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

        public MainWindowViewModel() {
            initCoreLib();
            
            StatusBar = new StatusBarViewModel();

            // Snippet from https://github.com/Kinkojiro/EGM-Settings/blob/master/EGMSettings/EGMSettings.xaml.cs
            // Check if game path was passed in the command line
            string[] args = Environment.GetCommandLineArgs();
            string argDir = "";
            MEGame? argGame = null;

            // Set the path and game if it's either ME3 or LE3
            if (args.Length > 1 && !string.IsNullOrEmpty(args[1]) && Directory.Exists(argDir = Path.GetFullPath(args[1].Trim('"')))) {
                //test if ME3 directory has been passed
                if (File.Exists(Path.Combine(argDir, "Binaries\\Win32\\MassEffect3.exe"))) {
                    argGame = MEGame.ME3;
                }
                //test if LE3 directory has been passed
                if (File.Exists(Path.Combine(argDir, "Binaries\\Win64\\MassEffect3.exe"))) {
                    argGame = MEGame.LE3;
                }
            }

            // Call MorphControlViewModel with the cl argument only if its a valid one
            if (!string.IsNullOrEmpty(argDir) && argGame != null) {
                MorphControl = new MorphControlViewModel(StatusBar, (MEGame) argGame, argDir);
            } else {
                MorphControl = new MorphControlViewModel(StatusBar);
            }

            controls.AddRange(new List<PropertyChangedBase> {StatusBar, MorphControl});

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
            try {
                using StreamReader sr = new(Path.Combine(Environment.CurrentDirectory, "resources/FAQ.json"));
                string faqString = sr.ReadToEnd();
                Dictionary<string, string> faqParsed = JsonSerializer.Deserialize<Dictionary<string, string>>(faqString);
                foreach (string i in faqParsed.Keys) {
                    faq.Add(new FAQItem(i, faqParsed[i]));
                }
            } catch (Exception e) {
                Log.Error(e.ToString());
                windowManager.ShowDialogAsync(new ExceptionHandlerViewModel(e), null, null); ;
            }
        }

        /// <summary>
        /// Open the mod website for the input game
        /// </summary>
        /// <param name="game">Game to open the mod site for</param>
        public void VisitModSite(MEGame game) {
            string gameSite = game.IsLEGame() ? "masseffectlegendaryedition" : "masseffect3";
            string modNumber = game.IsLEGame() ? "850" : "975";
            Process.Start(new ProcessStartInfo {
                FileName = @$"http://www.nexusmods.com/{gameSite}/mods/{modNumber}",
                UseShellExecute = true
            });
        }

        public void LaunchInfoWindow() {
            windowManager.ShowWindowAsync(new InfoWindowViewModel(), null, null);

        }

        public void LaunchFAQWindow() {
            windowManager.ShowWindowAsync(new FAQWindowViewModel(faq), null, null);
        }
    }
}
