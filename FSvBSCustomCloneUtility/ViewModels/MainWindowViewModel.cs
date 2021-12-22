using Caliburn.Micro;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FSvBSCustomCloneUtility.ViewModels {
    public enum Gender {
        Male,
        Female
    }

    public class MainWindowViewModel : Conductor<ObserverControl> {
        private List<ObserverControl> observers = new();
        public ObserverControl CustomMorph { get; set; }
        public ObserverControl ConditionalsControl { get; set; }

        private const string BUTTONSELECTEDCOLOR = "#5c5f72";
        private const string BUTTONDEFAULTCOLOR = "#3e3d4b";

        private bool ME3PathChecked = false;
        private bool LE3PathChecked = false;

        private string _ME3ButtonColor = BUTTONDEFAULTCOLOR;
        private string _LE3ButtonColor = BUTTONDEFAULTCOLOR;
        public string ME3ButtonColor {
            get { return _ME3ButtonColor; }
            set {
                _ME3ButtonColor = value;
                NotifyOfPropertyChange(() => ME3ButtonColor);
            }
        }
        public string LE3ButtonColor {
            get { return _LE3ButtonColor; }
            set {
                _LE3ButtonColor = value;
                NotifyOfPropertyChange(() => LE3ButtonColor);
            }
        }

        public void TargetME3(string game) {
            if (!ME3PathChecked) {
                if (!SetGamePath(MEGame.ME3)) { return; }
                else { ME3PathChecked = true; }
            }

            if (!VerifyMod(MEGame.ME3)) { return; }
            
            ME3ButtonColor = BUTTONSELECTEDCOLOR;
            LE3ButtonColor = BUTTONDEFAULTCOLOR;
            Notify("TargetGame", MEGame.ME3);
        }
        public void TargetLE3() {
            if (!LE3PathChecked) {
                if (!SetGamePath(MEGame.LE3)) { return; }
                else { LE3PathChecked = true; }
            }

            if (!VerifyMod(MEGame.LE3)) { return; }
            
            LE3ButtonColor = BUTTONSELECTEDCOLOR;
            ME3ButtonColor = BUTTONDEFAULTCOLOR;
            Notify("TargetGame", MEGame.LE3);
        }

        public MainWindowViewModel() {
            initCoreLib();

            // Create views and adds them to the observers
            ConditionalsControl = new ConditionalsControlViewModel();
            CustomMorph = new CustomMorphViewModel(new List<ObserverControl> { ConditionalsControl });
            observers.Add(CustomMorph);
            observers.Add(ConditionalsControl);

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
                                                 $"Select Mass Effect 3 {(game.IsGame3() ? "" : "LE ")}executable");

                if (string.IsNullOrEmpty(file)) { return false; } // User didn't select a file

                if (game == MEGame.ME3) {
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
            foreach (ObserverControl observer in observers) {
                await ActivateItemAsync(observer);
            }
        }

        private void Notify<Type>(string name, Type value) {
            foreach(ObserverControl observer in observers) {
                observer.Update(name, value);
            }
        }
    }
}
