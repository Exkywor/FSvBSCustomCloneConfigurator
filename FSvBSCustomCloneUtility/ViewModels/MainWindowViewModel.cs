using Caliburn.Micro;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
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

namespace FSvBSCustomCloneUtility.ViewModels {
    public enum Gender {
        Male,
        Female
    }

    public class MainWindowViewModel : Conductor<ObserverControl> {
        private List<ObserverControl> observers = new();
        public ObserverControl CustomMorph { get; set; }

        private string _targetGame = "ME3";
        public string TargetGame {
            get {
                return _targetGame;
            }

            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }

        private bool IsME3 = true;
        public bool ME3Checked {
            get { return IsME3; }
            set {
                if (value.Equals(IsME3)) return;
                ToggleMEGame();
            }
        }
        public bool LE3Checked {
            get { return !IsME3; }
            set {
                if (value.Equals(!IsME3)) return;
                ToggleMEGame();
            }
        }

        public void ToggleMEGame() {
            IsME3 = !IsME3;
            TargetGame = IsME3 ? "ME3" : "LE3";
            Notify("TargetGame");
        }

        public MainWindowViewModel() {
            initCoreLib();
            LoadViewAsync();

            // ConditionalsManager.SetConditional(Gender.Male, false, targetFile);
            // ConditionalsManager.SetConditional(Gender.Female, true, targetFile);
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

        private async Task LoadViewAsync() {
            CustomMorph = new CustomMorphViewModel();
            observers.Add(CustomMorph);
            foreach (ObserverControl observer in observers) {

                await ActivateItemAsync(observer);
            }
        }

        private void Notify(string property) {
            foreach(ObserverControl observer in observers) {
                observer.Update(property, TargetGame);
            }
        }
    }
}
