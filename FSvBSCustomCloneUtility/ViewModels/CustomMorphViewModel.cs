using Caliburn.Micro;
using FSvBSCustomCloneUtility;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.ViewModels;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class CustomMorphViewModel : ObserverControl {
        private List<ObserverControl> observers = new();

        private bool _isValid = false;
        public bool IsValid {
            get { return _isValid; }
            set {
                _isValid = value;
                NotifyOfPropertyChange(() => IsValid);
            }
        }

        private MEGame _targetGame;
        public MEGame TargetGame {
            get { return _targetGame; }
            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }

        private string _ronMFile = "";
        public string RonMFile {
            get { return _ronMFile; }
            set {
                _ronMFile = value;
                NotifyOfPropertyChange(() => RonMFile);
            }
        }
        private string _ronFFile = "";
        public string RonFFile {
            get { return _ronFFile; }
            set {
                _ronFFile = value;
                NotifyOfPropertyChange(() => RonFFile);
            }
        }

        public CustomMorphViewModel() { }
        public CustomMorphViewModel(List<ObserverControl> observers) {
            this.observers = observers;
            // DataContext = this;

            // MorphWriter writerMale = new(ronFile, targetFile, Gender.Male);
            // writerMale.ApplyMorph();
            // MorphWriter writerFemale = new(ronFileF, targetFile, Gender.Female, new List<string>() { customHair });
            // writerFemale.ApplyMorph();
        }

        public void RonMFileButton() {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", "Select male headmorph");

            if (!string.IsNullOrEmpty(file)) {
                RonMFile = file;
                CheckIfApply();
            }
        }
        public void RonFFileButton() {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", "Select female headmorph");

            if (!string.IsNullOrEmpty(file)) {
                RonFFile = file;
                CheckIfApply();
            }
        }

        public void ClearMRon() {
            RonMFile = "";
            CheckIfApply();
        }

        public void ClearFRon() {
            RonFFile = "";
            CheckIfApply();
        }

        private void CheckIfApply() {
            // If both ron files are empty it will check as false, meaning the file is invalid
            IsValid = !(String.IsNullOrEmpty(RonMFile) && String.IsNullOrEmpty(RonFFile));
        }

        public void Apply() {
            Notify<bool>("Apply", true);
        }

        public override void Update<Type>(string name, Type value) {
            switch (name) {
                case "TargetGame":
                    TargetGame = (MEGame) Convert.ChangeType(value, typeof(MEGame));
                    CheckIfApply();
                    break;
                default:
                    break;
            }
        }

        private void Notify<Type>(string name, Type value) {
            foreach(ObserverControl observer in observers) {
                observer.Update(name, value);
            }
        }
    }
}
