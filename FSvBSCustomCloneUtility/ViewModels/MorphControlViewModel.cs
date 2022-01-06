using Caliburn.Micro;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.InterfacesAndClasses;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class MorphControlViewModel : PropertyChangedBase {
        private IWindowManager windowManager = new WindowManager();
        private StatusBar statusBar;

        private bool _isBusy = false;
        protected bool IsBusy {
            get { return _isBusy; }
            set {
                _isBusy = value;

                NotifyOfPropertyChange(() => CanApply);
                NotifyOfPropertyChange(() => CanSetDefaultAppearance);
                NotifyOfPropertyChange(() => CanSetCustomAppearance);
                NotifyOfPropertyChange(() => MaleBoxEnabled);
                NotifyOfPropertyChange(() => FemaleBoxEnabled);
            }
        }

        // GAME TARGET AND PATH PROPERTIES
        // Prevent us from trying to set the path if it's already set
        private bool ME3PathChecked = false;
        private bool LE3PathChecked = false;


        private string _targetPath = "";
        public string TargetPath {
            get { return _targetPath; }
            set {
                _targetPath = value;
                NotifyOfPropertyChange(() => TargetPath);
                NotifyOfPropertyChange(() => TargetPathEnabled);
            }
        }
        public bool TargetPathEnabled {
            get { return !string.IsNullOrEmpty(TargetPath); }
        }

        private MEGame? _targetGame = null;
        public MEGame? TargetGame {
            get { return _targetGame; }
            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
                NotifyOfPropertyChange(() => CanApply);
                NotifyOfPropertyChange(() => CanSetDefaultAppearance);
                NotifyOfPropertyChange(() => CanSetCustomAppearance);
            }
        }

        // CONDITIONALS PROPERTIES
        private bool _setMaleCustom = false;
        public bool SetMaleCustom {
            get { return _setMaleCustom; }
            set {
                _setMaleCustom = value;
                NotifyOfPropertyChange(() => SetMaleCustom);
                NotifyOfPropertyChange(() => MaleBoxEnabled);
            }
        }
        private bool _setFemaleCustom = false;
        public bool SetFemaleCustom {
            get { return _setFemaleCustom; }
            set {
                _setFemaleCustom = value;
                NotifyOfPropertyChange(() => SetFemaleCustom);
                NotifyOfPropertyChange(() => FemaleBoxEnabled);
            }
        }

        // MORPH PROPERTIES
        private bool _applyToActor = true;
        
        private string _ronMFile = "";
        public string RonMFile {
            get { return _ronMFile; }
            set {
                _ronMFile = value;
                NotifyOfPropertyChange(() => RonMFile);
                NotifyOfPropertyChange(() => CanApply);
            }
        }
        
        private string _ronFFile = "";
        public string RonFFile {
            get { return _ronFFile; }
            set {
                _ronFFile = value;
                NotifyOfPropertyChange(() => RonFFile);
                NotifyOfPropertyChange(() => CanApply);
            }
        }

        public bool MaleBoxEnabled {
            get { return SetMaleCustom && !IsBusy; }
        }
        public bool FemaleBoxEnabled {
            get { return SetFemaleCustom && !IsBusy; }
        }

        public MorphControlViewModel(StatusBar statusBar) {
            this.statusBar = statusBar;
        }

        // GAME TARGET METHODS
        public void SetGameTarget(MEGame game) {
            // Get game path only if we haven't gotten it already
            if (game.IsOTGame()) {
                if (!ME3PathChecked) {
                    if (!GetGamePath(game)) {
                        statusBar.UpdateStatus("Error: ME3 game path not set");
                        TargetGame = null;
                        TargetPath = "";
                        return;
                    } else { ME3PathChecked = true; }
                }
            } else {
                if (!LE3PathChecked) {
                    if (!GetGamePath(game)) {
                        statusBar.UpdateStatus("Error: LE3 game path not set");
                        TargetGame = null;
                        TargetPath = "";
                        return;
                    } else { LE3PathChecked = true; }
                }
            }

            // Verify mod installation
            if (!VerifyMod(game)) {
                statusBar.UpdateStatus("Error: Mod not installed or invalid");
                TargetGame = null;
                TargetPath = "";
                return;
            }
            
            TargetPath = MEDirectories.GetExecutablePath(game);
            TargetGame = game;
            statusBar.UpdateStatus($"Selected Mass Effect 3 {(game.IsOTGame() ? "" : "LE ")}as the game target");
            return;
        }

        /// <summary>
        /// Get the input game path, or prompt the user to point to the input game executable if it wasn't found
        /// </summary>
        /// <param name="game">Target game</param>
        /// <returns></returns>
        private bool GetGamePath(MEGame game) {
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
                windowManager.ShowDialogAsync(new CustomMessageBoxViewModel(
                        "The FemShep v BroShep mod was not found. Make sure to install the mod before running this tool.", "Error", "OK"),
                    null, null);

                return false;
            } else if (!FSvBSDirectories.IsValidDummies(game, FSvBSDirectories.GetDummiesPath(game))) {
                windowManager.ShowDialogAsync(new CustomMessageBoxViewModel(
                        "The FemShep v BroShep mod version is incompatible. Make sure to have version 1.1.0 or higher installed.", "Error", "OK"),
                    null, null);

                return false;
            } else {
                return true;
            }
        }

        // CONDITIONALS METHODS
        public bool CanSetDefaultAppearance {
            get { return TargetGame != null && !IsBusy; }
        }
        public bool CanSetCustomAppearance {
            get { return TargetGame != null && !IsBusy; }
        }

        public void SetDefaultAppearance(Gender gender) {
            if (gender.IsFemale()) { SetFemaleCustom = false; }
            else { SetMaleCustom = false; }
          
            statusBar.UpdateStatus($"{(gender.IsFemale() ? "Female" : "Male")} clone will be set to default appearance");
        }

        public void SetCustomAppearance(Gender gender) {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", $"Select the {(gender.IsFemale() ? "female" : "male")} headmorph");

            if (string.IsNullOrEmpty(file)) { 
                if (gender.IsFemale()) { SetFemaleCustom = false; }
                else { SetMaleCustom = true; }
                return;
            }

            if (gender.IsFemale()) {
                RonFFile = file;
                SetFemaleCustom = true;
            } else {
                RonMFile = file;
                SetMaleCustom = true;
            }

            statusBar.UpdateStatus($"Added {(gender.IsFemale() ? "Female" : "Male")} headmorph file. The clone will be set to custom appearance");
        }

        public bool CanApply {
            get { return TargetGame != null && (RonMFile != "" || RonFFile != "") && !IsBusy; }
        }

        public void Apply() {
            List<Gender> targets = new(); // Aggregation of targets
            IsBusy = true;
            Thread.Sleep(2000);

            if (SetMaleCustom) {
                targets.Add(Gender.Male);
            } else {
                ConditionalsManager.SetConditional(Gender.Male, false, (MEGame) TargetGame);
                statusBar.UpdateStatus("Set default appearance for the male clone");
            }

            if (SetFemaleCustom) {
                targets.Add(Gender.Female);
            } else {
                ConditionalsManager.SetConditional(Gender.Female, false, (MEGame) TargetGame);
                statusBar.UpdateStatus("Set default appearance for the female clone");
            }

            if (targets.Count > 0) {
                ApplyCustomApperance(targets);
            }
            IsBusy = false;
        }

        private void ApplyCustomApperance(List<Gender> targets) {
            FSvBSDirectories.ApplyCleanFiles((MEGame) TargetGame);
            // statusBar.UpdateStatus("Cleared clone files");
            foreach (Gender gender in targets) {
                ApplyMorph(gender, gender.IsFemale() ? RonFFile : RonMFile);
                ConditionalsManager.SetConditional(gender, true, (MEGame) TargetGame);
            }
        }

        private void ApplyMorph(Gender gender, string ronFile) {
            statusBar.UpdateStatus($"Applying {(gender.IsFemale() ? "female" : "male")} headmorph");
            MorphWriter writer = new(ronFile, (MEGame)TargetGame, gender);
            bool res = writer.ApplyMorph();
            if (res) {
                if (_applyToActor) {
                    statusBar.UpdateStatus("Cloning and linking the headmorph to the clone's files");
                    MorphRelinker relinker = new((MEGame)TargetGame, gender);
                    relinker.RelinkMorph();
                }

                MessageBox.Show($"The {(gender.IsFemale() ? "female" : "male")} headmorph was applied succesfully.",
                                "Success", MessageBoxButton.OK);
                statusBar.UpdateStatus($"Applied {(gender.IsFemale() ? "female" : "male")} headmorph");
            } else {
                statusBar.UpdateStatus($"Aborted {(gender.IsFemale() ? "female" : "male")} morph application");
            }
        }
    }
}
