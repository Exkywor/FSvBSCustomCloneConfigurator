using Caliburn.Micro;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.InterfacesAndClasses;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

                NotifyOfPropertyChange(() => CanSetGameTarget);
                NotifyOfPropertyChange(() => CanApplyAsync);
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
                NotifyOfPropertyChange(() => CanApplyAsync);
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
                NotifyOfPropertyChange(() => CanApplyAsync);
            }
        }
        
        private string _ronFFile = "";
        public string RonFFile {
            get { return _ronFFile; }
            set {
                _ronFFile = value;
                NotifyOfPropertyChange(() => RonFFile);
                NotifyOfPropertyChange(() => CanApplyAsync);
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
        public bool CanSetGameTarget {
            get { return !IsBusy; }
        }

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

        public bool CanApplyAsync {
            get { return TargetGame != null && (RonMFile != "" || RonFFile != "" || !SetMaleCustom || !SetFemaleCustom) && !IsBusy; }
        }

        public void ApplyAsync() {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Apply;
            worker.ProgressChanged += Apply_ProgressChanged;
            worker.RunWorkerCompleted += Apply_RunWokerCompleted;

            worker.RunWorkerAsync();

            // IsBusy = true;

            // IsBusy = false;
        }

        private void Apply(object sender, DoWorkEventArgs e) {
            (sender as BackgroundWorker).ReportProgress(0, "Busy");
            List<Gender> targets = new(); // Aggregation of targets

            // Since the files are reset, we set the bool for non-headmorphs to default to avoid issues
            if (SetMaleCustom) {
                targets.Add(Gender.Male);
            } else {
                ConditionalsManager.SetConditional(Gender.Male, false, (MEGame) TargetGame);
                (sender as BackgroundWorker).ReportProgress(0, "SetMaleDefault");
                Thread.Sleep(1000);
            }

            if (SetFemaleCustom) {
                targets.Add(Gender.Female);
            } else {
                ConditionalsManager.SetConditional(Gender.Female, false, (MEGame) TargetGame);
                (sender as BackgroundWorker).ReportProgress(0, "SetFemaleDefault");
                Thread.Sleep(1000);
            }

            if (targets.Count > 0) {
                ApplyCustomApperance(sender, targets);
            }
        }

        private void ApplyCustomApperance(object sender, List<Gender> targets) {
            (sender as BackgroundWorker).ReportProgress(0, "Cleaning");
            FSvBSDirectories.ApplyCleanFiles((MEGame) TargetGame);
            (sender as BackgroundWorker).ReportProgress(0, "Cleaned");
            foreach (Gender gender in targets) {
                ApplyMorph(sender, gender, gender.IsFemale() ? RonFFile : RonMFile);
                ConditionalsManager.SetConditional(gender, true, (MEGame) TargetGame);
            }
        }

        private void ApplyMorph(object sender, Gender gender, string ronFile) {
            (sender as BackgroundWorker).ReportProgress(0, $"Applying;{(gender.IsFemale() ? "female" : "male")}");
            MorphWriter writer = new(ronFile, (MEGame)TargetGame, gender);
            bool res = writer.ApplyMorph();
            if (res) {
                if (_applyToActor) {
                    (sender as BackgroundWorker).ReportProgress(0, "Linking");
                    MorphRelinker relinker = new((MEGame)TargetGame, gender);
                    relinker.RelinkMorph();
                }

                (sender as BackgroundWorker).ReportProgress(0, $"Applied;{(gender.IsFemale() ? "female" : "male")}");
            } else {
                statusBar.UpdateStatus($"Aborted {(gender.IsFemale() ? "female" : "male")} morph application");
            }
        }

        private void Apply_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            string[] status = e.UserState.ToString().Split(";");

            switch (status[0]) {
                case "Busy":
                    IsBusy = true;
                    break;
                case "Cleaning":
                    statusBar.UpdateStatus("Cleaning the clone files");
                    break;
                case "Cleaned":
                    statusBar.UpdateStatus("Clone files cleaned");
                    break;
                case "Applying":
                    statusBar.UpdateStatus($"Applying {status[1]} headmorph");
                    break;
                case "Linking":
                    statusBar.UpdateStatus("Cloning and linking the headmorph to the clone's files");
                    break;
                case "SetMaleDefault":
                    statusBar.UpdateStatus("Default appearance for the male clone set");
                    break;
                case "SetFemaleDefault":
                    statusBar.UpdateStatus("Default appearance for the female clone set");
                    break;
                case "Applied":
                    statusBar.UpdateStatus($"Applied {status[1]} headmorph");
                    windowManager.ShowWindowAsync(new CustomMessageBoxViewModel($"The {status[1]} headmorph was applied succesfully.",
                        "Success", "OK"), null, null);
                    break;
            }
        }

        private void Apply_RunWokerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                // Catch error
            } else if (e.Cancelled) {
                // Cancelled

            } else {
                IsBusy = false;
            }
        }
    }
}
