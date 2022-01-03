using Caliburn.Micro;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Unreal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class CustomMorphViewModel : ObserverControl {
        private IWindowManager windowManager = new WindowManager();
        private List<ObserverControl> _observers = new();

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
            _observers = observers;
        }

        public void RonMFileButton() {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", "Select male headmorph");

            if (!string.IsNullOrEmpty(file)) {
                RonMFile = file;
                CheckIfApply();
            }

            Notify("SetStatus", "Added male headmorph file");
        }
        public void RonFFileButton() {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", "Select female headmorph");

            if (!string.IsNullOrEmpty(file)) {
                RonFFile = file;
                CheckIfApply();
            }

            Notify("SetStatus", "Added female headmorph file");
        }

        public void ClearMRon() {
            RonMFile = "";
            CheckIfApply();
            Notify("SetStatus", "Removed male headmorph file");
        }

        public void ClearFRon() {
            RonFFile = "";
            CheckIfApply();
            Notify("SetStatus", "Removed female headmorph file");
        }

        /// <summary>
        /// Set IsValid to true if the TargetGame has been set, and at least one morph file has been selected
        /// </summary>
        private void CheckIfApply() {
            IsValid = TargetGame != null && (RonMFile != "" || RonFFile != "");
        }

        public async void Apply() {
            FSvBSDirectories.ApplyCleanFiles((MEGame) TargetGame, ApplyToActor);
            Notify("ClearConds", "");
            Notify("SetStatus", "Cleared clone files");
            await Task.Delay(1000);

            if (!string.IsNullOrEmpty(RonMFile)) {
                Notify("SetStatus", "Applying male headmorph");
                await Task.Delay(1000);
                ApplyMorph(Gender.Male, RonMFile);
            }
            if (!string.IsNullOrEmpty(RonFFile)) {
                Notify("SetStatus", "Applying female headmorph");
                await Task.Delay(1000);
                ApplyMorph(Gender.Female, RonFFile);
            }

            FSvBSDirectories.TOCMod((MEGame)TargetGame);
        }

        private async void ApplyMorph(Gender gender, string ronFile) {
            MorphWriter writer = new(ronFile, (MEGame)TargetGame, gender, ApplyToActor);
            bool res = writer.ApplyMorph();
            if (res) {
                if (ApplyToActor) {
                    Notify("SetStatus", "Cloning and linking the headmorph to the clone's files");
                    await Task.Delay(1000);
                    MorphRelinker relinker = new((MEGame)TargetGame, gender);
                    relinker.RelinkMorph();
                }

                Notify("Apply", $"{(gender.IsFemale() ? "F" : "M")}");
                await windowManager.ShowWindowAsync(new CustomMessageBoxViewModel(
                        $"The {(gender.IsFemale() ? "female" : "male")} headmorph was applied succesfully.", "Success", "OK"),
                    null, null);
                Notify("SetStatus", $"Applied {(gender.IsFemale() ? "female" : "male")} headmorph");
            } else {
                Notify("SetStatus", "Aborted morph application");
            }
        }

        public override void Update<Type>(string name, Type value) {
            switch (name) {
                case "TargetGame":
                    TargetGame = (MEGame) Convert.ChangeType(value, typeof(MEGame));
                    CheckIfApply();
                    break;
            }
        }

        private void Notify<Type>(string name, Type value) {
            foreach(ObserverControl observer in _observers) {
                observer.Update(name, value);
            }
        }
    }
}
