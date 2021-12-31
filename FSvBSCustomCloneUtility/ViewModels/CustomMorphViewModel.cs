using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Unreal;
using System;
using System.Collections.Generic;
using System.Windows;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class CustomMorphViewModel : ObserverControl {
        private List<ObserverControl> observers = new();

        private bool _applyToActor = true;
        public bool ApplyToActor {
            get { return _applyToActor; }
            set {
                _applyToActor = value;
                NotifyOfPropertyChange(() => ApplyToActor);
            }
        }

        private bool _isValid = false;
        public bool IsValid {
            get { return _isValid; }
            set {
                _isValid = value;
                NotifyOfPropertyChange(() => IsValid);
            }
        }

        private MEGame? _targetGame;
        public MEGame? TargetGame {
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

        /// <summary>
        /// Set IsValid to true if the TargetGame has been set, and at least one morph file has been selected
        /// </summary>
        private void CheckIfApply() {
            IsValid = TargetGame != null && (RonMFile != "" || RonFFile != "");
        }

        public void Apply() {
            bool tocced = false;
            FSvBSDirectories.ApplyCleanFiles((MEGame) TargetGame, ApplyToActor);
            Notify("ClearConds", "");

            if (!string.IsNullOrEmpty(RonMFile)) { ApplyMorph(Gender.Male, RonMFile); }
            if (!string.IsNullOrEmpty(RonFFile)) { ApplyMorph(Gender.Female, RonFFile); }

            FSvBSDirectories.TOCMod((MEGame)TargetGame);
        }

        private void ApplyMorph(Gender gender, string ronFile) {
            MorphWriter writer = new(ronFile, (MEGame)TargetGame, gender, ApplyToActor);
            bool res = writer.ApplyMorph();
            if (res) {
                if (ApplyToActor) {
                    MorphRelinker relinker = new((MEGame)TargetGame, gender);
                    relinker.RelinkMorph();
                }

                Notify("Apply", $"{(gender.IsFemale() ? "F" : "M")}");
                MessageBox.Show($"The {(gender.IsFemale() ? "female" : "male")} headmorph was applied succesfully.", "Success", MessageBoxButton.OK);
            }
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
