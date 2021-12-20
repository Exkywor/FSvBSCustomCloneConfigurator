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

        private const string BUTTONSELECTEDCOLOR = "#5c5f72";
        private const string BUTTONDEFAULTCOLOR = "#3e3d4b";

        private string ME3Path = "";
        private string LE3Path = "";
        private bool IsME3 = false;

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

        public void ME3Clicked() {
            string path = "";

            if (ME3Path == "") {
                path = GetPath("ME3");

                if (path == "") {
                    // Prompt the user for the path
                    // Potentially return and stop everything here
                }

                ME3Path = path;
            } 

            IsME3 = true;
            ME3ButtonColor = BUTTONSELECTEDCOLOR;
            LE3ButtonColor = BUTTONDEFAULTCOLOR;
            Notify("TargetGame");
        }
        public void LE3Clicked() {
            string path = "";

            if (LE3Path == "") {
                path = GetPath("LE3");

                if (path == "") {
                    // Prompt the user for the path
                    // Potentially return and stop everything here
                }

                LE3Path = path;
            } 

            IsME3 = false;
            LE3ButtonColor = BUTTONSELECTEDCOLOR;
            ME3ButtonColor = BUTTONDEFAULTCOLOR;
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

        private string GetPath(string game) {
            return "";
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
                switch(property) {
                    case "TargetGame":
                        observer.Update(property, IsME3 ? "ME3" : "LE3", IsME3 ? ME3Path : LE3Path);
                        break;
                }
            }
        }
    }
}
