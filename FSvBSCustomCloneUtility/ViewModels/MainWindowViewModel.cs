using Caliburn.Micro;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Path = System.IO.Path;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class MainWindowViewModel : Conductor<ObserverControl> {
        private List<ObserverControl> _observers = new();
        public ObserverControl CustomMorph { get; set; }
        public ObserverControl ConditionalsControl { get; set; }
        public ObserverControl StatusBar { get; set; }

        // Prevent us from trying to set the path if it's already set
        private bool ME3PathChecked = false;
        private bool LE3PathChecked = false;

        public void TargetME3() {
            TargetGame(MEGame.ME3);
        }
        public void TargetLE3() {
            TargetGame(MEGame.LE3);
        }

        private void TargetGame(MEGame game) {
            // Verify game is installed
            if (game.IsOTGame()) {
                if (!ME3PathChecked) {
                    if (!SetGamePath(game)) {
                        Notify("SetStatus", "Error: ME3 game path not set");
                        return;
                    } else { ME3PathChecked = true; }
                }

            } else {
                if (!LE3PathChecked) {
                    if (!SetGamePath(game)) {
                        Notify("SetStatus", "Error: LE3 game path not set");
                        return;
                    } else { LE3PathChecked = true; }
                }

            }

            // Verify mod installation
            if (!VerifyMod(game)) {
                Notify("SetStatus", "Error: Mod not installed or invalid");
                return;
            }
            
            SelectedTargetPath = MEDirectories.GetExecutablePath(game);
            Notify("TargetGame", game);
            Notify("SetStatus", $"Selected Mass Effect 3 {(game.IsOTGame() ? "" : "LE ")}as the game target");
        }

        private string _selectedTargetPath = "";
        public string SelectedTargetPath {
            get { return _selectedTargetPath; }
            set {
                _selectedTargetPath = value;
                NotifyOfPropertyChange(() => SelectedTargetPath);
            }
        }

        public MainWindowViewModel() {
            initCoreLib();

            // Create views and adds them to the observers
            StatusBar = new StatusBarViewModel();
            ConditionalsControl = new ConditionalsControlViewModel(new List<ObserverControl> { StatusBar });
            CustomMorph = new CustomMorphViewModel(new List<ObserverControl> { ConditionalsControl, StatusBar });
            _observers.Add(StatusBar);
            _observers.Add(ConditionalsControl);
            _observers.Add(CustomMorph);

            LoadViewAsync();
        } 

        /// <summary>
        /// Initialize Legendary Explorer Core Library
        /// </summary>
        private static void initCoreLib() {
            static void packageSaveFailed(string message) {
                // I'm not sure if this requires ui thread since it's win32 but i'll just make sure
                Application.Current.Dispatcher.Invoke(() => {
                    MessageBox.Show(message);
                });
            }
 
            LegendaryExplorerCoreLib.InitLib(TaskScheduler.FromCurrentSynchronizationContext(), packageSaveFailed);
        }

        /// <summary>
        /// Prompt the user to point to the input game executabe, if it's not found
        /// </summary>
        /// <param name="game">Target game</param>
        /// <returns></returns>
        private bool SetGamePath(MEGame game) {
            // Get the user to point to the game path if it's not found
            if (string.IsNullOrEmpty(MEDirectories.GetDefaultGamePath(game))) {
                string file = Misc.PromptForFile("MassEffect3.exe|MassEffect3.exe",
                                                 $"Select Mass Effect 3 {(game.IsOTGame() ? "" : "LE ")}executable");

                if (string.IsNullOrEmpty(file)) { return false; } // User didn't select a file

                if (game.IsOTGame()) {
                    ME3Directory.DefaultGamePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(file)));
                } else {
                    LE3Directory.DefaultGamePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(file)));
                }
            }

            return true;
        }

        /// <summary>
        /// Verify that the mod is installed in its compatible version for the input game
        /// </summary>
        /// <param name="game">Target game</param>
        /// <returns></returns>
        private bool VerifyMod(MEGame game) {
            if (!FSvBSDirectories.IsModInstalled(game)) {
                MessageBox.Show("The FemShep v BroShep mod was not found. Make sure to install the mod before running this tool.",
                    "Error", MessageBoxButton.OK);
                return false;
            } else if (!FSvBSDirectories.IsValidDummies(game, FSvBSDirectories.GetDummiesPath(game))) {
                MessageBox.Show("The FeemShep v BroShep mod version is incompatible. Make sure to have version 1.1.0 or higher installed.",
                    "Error", MessageBoxButton.OK);
                return false;
            } else { return true; }
        }

        private async Task LoadViewAsync() {
            foreach (ObserverControl observer in _observers) {
                await ActivateItemAsync(observer);
            }
        }

        private void Notify<Type>(string name, Type value) {
            foreach(ObserverControl observer in _observers) {
                observer.Update(name, value);
            }
        }
    }
}
