using Caliburn.Micro;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.InterfacesAndClasses;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using Action = System.Action;
using Path = System.IO.Path;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class MorphControlViewModel : PropertyChangedBase {
        private IWindowManager windowManager = new WindowManager();
        private StatusBar statusBar;

        /// <summary>
        /// Whether or not the app is busy. Used to disable UI interactions while a process is happening
        /// </summary>
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

        // Checks to prevent trying to set the path if it's already set
        private bool ME3PathChecked = false;
        private bool LE3PathChecked = false;

        public bool CanSetGameTarget {
            get { return !IsBusy; }
        }

        private bool _isME3 = false;
        public bool IsME3 {
            get { return _isME3; }
            set {
                _isME3 = value;
                NotifyOfPropertyChange(() => IsME3);
            }
        }
        private bool _isLE3 = false;
        public bool IsLE3 {
            get { return _isLE3; }
            set {
                _isLE3 = value;
                NotifyOfPropertyChange(() => IsLE3);
            }
        }

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
        private bool _isMaleDefault = true;
        public bool IsMaleDefault {
            get { return _isMaleDefault; }
            set {
                _isMaleDefault = value;
                if (value) { RonMFile = ""; }
                NotifyOfPropertyChange(() => IsMaleDefault);
                NotifyOfPropertyChange(() => MaleBoxEnabled);
            }
        }
        private bool _isMaleCustom = false;
        public bool IsMaleCustom {
            get { return _isMaleCustom; }
            set {
                _isMaleCustom = value;
                NotifyOfPropertyChange(() => IsMaleCustom);
                NotifyOfPropertyChange(() => MaleBoxEnabled);
            }
        }
        private bool _isFemaleDefault = true;
        public bool IsFemaleDefault {
            get { return _isFemaleDefault; }
            set {
                _isFemaleDefault = value;
                if (value) { RonFFile = ""; }
                NotifyOfPropertyChange(() => IsFemaleDefault);
                NotifyOfPropertyChange(() => FemaleBoxEnabled);
            }
        }
        private bool _isFemaleCustom = false;
        public bool IsFemaleCustom {
            get { return _isFemaleCustom; }
            set {
                _isFemaleCustom = value;
                NotifyOfPropertyChange(() => IsFemaleCustom);
                NotifyOfPropertyChange(() => FemaleBoxEnabled);
            }
        }

        // MORPH PROPERTIES
        private bool _applyToActor = true;

        public bool CanSetDefaultAppearance {
            get { return TargetGame != null && !IsBusy; }
        }
        public bool CanSetCustomAppearance {
            get { return TargetGame != null && !IsBusy; }
        }

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
            get { return IsMaleCustom && !IsBusy; }
        }
        public bool FemaleBoxEnabled {
            get { return IsFemaleCustom && !IsBusy; }
        }

        public MorphControlViewModel(StatusBar statusBar) {
            this.statusBar = statusBar;
        }

        public MorphControlViewModel(StatusBar statusBar, MEGame game, string path) {
            this.statusBar = statusBar;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, 
                new Action(() => { SetGameTarget(game, path); }));
        }

        // GAME TARGET METHODS
        /// <summary>
        /// Sets the game target to modify
        /// </summary>
        /// <param name="game">Game target</param>
        /// <param name="path">Game path passed by the command line, empty otherwise</param>
        public void SetGameTarget(MEGame game, string path) {
            // Game path passed in the command line
            if (!string.IsNullOrEmpty(path)) {
                string pathRes = Validators.ValidatePath(game, path);
                if (!string.IsNullOrEmpty(pathRes)) {
                    UnsetGameTarget(game, pathRes);
                    return;
                } else {
                    if (game.IsOTGame()) {
                        ME3PathChecked = true;
                        ME3Directory.DefaultGamePath = path;
                    } else {
                        LE3PathChecked = true;
                        LE3Directory.DefaultGamePath = path;
                    }
                }
            } else {
                // Get game path only if we haven't gotten it already
                if (game.IsOTGame()) {
                    if (!ME3PathChecked) {
                        if (!GetGamePath(game)) {
                            UnsetGameTarget(game, "ME3 game path not set");
                            return;
                        } else { ME3PathChecked = true; }
                    }
                } else {
                    if (!LE3PathChecked) {
                        if (!GetGamePath(game)) {
                            UnsetGameTarget(game, "LE3 game path not set");
                            return;
                        } else { LE3PathChecked = true; }
                    }
                }
            }

            // Validate mod installation
            string modRes = Validators.ValidateMod(game);
            if (!string.IsNullOrEmpty(modRes)) {
                UnsetGameTarget(game, "The mod could not be found or is an incompatible version");
                windowManager.ShowDialogAsync(new CustomMessageBoxViewModel(modRes, "Error", "OK"), null, null);
                return;
            }

            TargetPath = MEDirectories.GetExecutablePath(game);
            TargetGame = game;
            statusBar.UpdateStatus($"Set Mass Effect 3 {(game.IsOTGame() ? "" : "LE ")}as the game target");
            if (game.IsOTGame()) { IsME3 = true; } else { IsLE3 = true; }
            return;
        }

        /// <summary>
        /// Get the input game path, or prompt the user to point to the input game executable if it wasn't found
        /// </summary>
        /// <param name="game">Target game</param>
        /// <returns></returns>
        private bool GetGamePath(MEGame game) {
            // Get the user to point to the game path if it's not found
            string dir = MEDirectories.GetDefaultGamePath(game);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) {
                string file = Misc.PromptForFile("MassEffect3.exe|MassEffect3.exe",
                                                 $"Select Mass Effect 3 {(game.IsOTGame() ? "" : "LE ")}executable");

                if (string.IsNullOrEmpty(file)) { return false; } // User didn't select a file

                string rootDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(file)));

                string res = Validators.ValidatePath(game, rootDir);
                if (!string.IsNullOrEmpty(res)) { // Game path not valid
                    windowManager.ShowDialogAsync(new CustomMessageBoxViewModel(res, "Error", "OK"), null, null);
                    return false;
                } 

                if (game.IsOTGame()) { ME3Directory.DefaultGamePath = rootDir; }
                else { LE3Directory.DefaultGamePath = rootDir; }
            }

            return true;
        }

        /// <summary>
        /// Unsets the game target
        /// </summary>
        /// <param name="game">Game target to unset</param>
        /// <param name="status">Status message to show</param>
        private void UnsetGameTarget(MEGame game, string status) {
            statusBar.UpdateStatus(status);
            TargetGame = null;
            TargetPath = "";
            if (game.IsOTGame()) { IsME3 = false; } else { IsLE3 = false; }
        }

        // CONDITIONALS METHODS
        public void SetDefaultAppearance(Gender gender) {
            if (gender.IsFemale()) {
                IsFemaleCustom = false;
                IsFemaleDefault = true;
            } else {
                IsMaleCustom = false;
                IsMaleDefault = true;
            }

            statusBar.UpdateStatus($"{(gender.IsFemale() ? "Female" : "Male")} clone will be set to the default appearance");
        }

        public void SetCustomAppearance(Gender gender) {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", $"Select the {(gender.IsFemale() ? "female" : "male")} headmorph");

            if (string.IsNullOrEmpty(file)) {
                if (gender.IsFemale()) { IsFemaleDefault = true; } else { IsMaleDefault = true; }
                return;
            }

            if (gender.IsFemale()) {
                RonFFile = file;
                IsFemaleCustom = true;
                IsFemaleDefault = false;
            } else {
                RonMFile = file;
                IsMaleCustom = true;
                IsMaleDefault = false;
            }

            statusBar.UpdateStatus($"Added {(gender.IsFemale() ? "female" : "male")} headmorph file");
        }

        public bool CanApplyAsync {
            get { return TargetGame != null && (RonMFile != "" || RonFFile != "" || !IsMaleCustom || !IsFemaleCustom) && !IsBusy; }
        }

        /// <summary>
        /// Starting method for the apply process. It creates and configures the background worker for multi-threading
        /// </summary>
        public void ApplyAsync() {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Apply;
            worker.ProgressChanged += Apply_ProgressChanged;
            worker.RunWorkerCompleted += Apply_RunWokerCompleted;

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// The proper apply method that will be run in a different thread than the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Apply(object sender, DoWorkEventArgs e) {
            (sender as BackgroundWorker).ReportProgress(0, "Busy");
            List<Gender> targets = new(); // Aggregation of targets

            // Since the files are reset, we set the bool for non-headmorphs to default to avoid issues
            if (IsMaleCustom) {
                targets.Add(Gender.Male);
            } else {
                ConditionalsManager.SetConditional(Gender.Male, false, (MEGame)TargetGame);
                (sender as BackgroundWorker).ReportProgress(0, "SetMaleDefault");
                Thread.Sleep(1000);
            }

            if (IsFemaleCustom) {
                targets.Add(Gender.Female);
            } else {
                ConditionalsManager.SetConditional(Gender.Female, false, (MEGame)TargetGame);
                (sender as BackgroundWorker).ReportProgress(0, "SetFemaleDefault");
                Thread.Sleep(1000);
            }

            if (targets.Count > 0) {
                ApplyCustomApperance(sender, e, targets);
            }
        }

        /// <summary>
        /// Apply any selected morphs.
        /// Morphs need to be aggregated in order to clean files once, before applying them.
        /// Cannot call this method per morph or else clean files will reset previous morphs.
        /// Cannot do this at phase where default is involved to avoid cleaning morphs when only wanting to set as default.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="targets">List of gender targets</param>
        private void ApplyCustomApperance(object sender, DoWorkEventArgs e, List<Gender> targets) {
            (sender as BackgroundWorker).ReportProgress(0, "Cleaning");
            FSvBSDirectories.ApplyCleanFiles((MEGame)TargetGame);
            (sender as BackgroundWorker).ReportProgress(0, "Cleaned");

            foreach (Gender gender in targets) {
                bool res = ApplyMorph(sender, e, gender, gender.IsFemale() ? RonFFile : RonMFile);
                if (!res) {
                    e.Cancel = true;
                    return;
                }
                ConditionalsManager.SetConditional(gender, true, (MEGame)TargetGame);
            }
        }

        /// <summary>
        /// Applies an input morph file to the input Shepard gender
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="gender">Shepard gender to apply the morph to</param>
        /// <param name="ronFile">Morph file link</param>
        /// <returns>True if the application was successful</returns>
        private bool ApplyMorph(object sender, DoWorkEventArgs e, Gender gender, string ronFile) {
            (sender as BackgroundWorker).ReportProgress(0, $"Applying;{(gender.IsFemale() ? "female" : "male")}");
            MorphWriter writer = new(ronFile, (MEGame)TargetGame, gender);

            (Dictionary<string, string> resourcesNotFound, Dictionary<string, IEnumerable<string>> resourceDuplicates) = writer.ApplyMorph();

            // Success if all resources were found and there are no duplicates
            if (resourcesNotFound.Count == 0 && resourceDuplicates.Count == 0) {
                if (_applyToActor) {
                    (sender as BackgroundWorker).ReportProgress(0, "Linking");
                    MorphRelinker relinker = new((MEGame)TargetGame, gender);
                    relinker.RelinkMorph();
                }

                (sender as BackgroundWorker).ReportProgress(0, $"Applied;{(gender.IsFemale() ? "Female" : "Male")}");
                return true;
            } else {
                if (resourcesNotFound.Count > 0) {
                    (sender as BackgroundWorker).ReportProgress(0,
                        $"ResourcesNotFound;{(gender.IsFemale() ? "female" : "male")};{ResourcesToString("resourcesNotFound", resourcesNotFound)}");
                }

                if (resourceDuplicates.Count > 0) {
                    (sender as BackgroundWorker).ReportProgress(0,
                        $"ResourceDuplicates;{(gender.IsFemale() ? "female" : "male")};{ResourcesToString("resourceDuplicates", resourceDuplicates)}");
                }
                
                return false;
            }
        }

        /// <summary>
        /// Parses a dictionary of resources to a single string 
        /// </summary>
        /// <param name="type">Error type of the resources</param>
        /// <param name="errors">Dictionary of errors, passes as an object</param>
        /// <returns></returns>
        private string ResourcesToString(string type, object errors) {
            switch(type) {
                case "resourcesNotFound":
                    Dictionary<string, string> resourcesNotFound =
                        (Dictionary<string, string>)Convert.ChangeType(errors, typeof(Dictionary<string, string>));

                    return string.Join(Environment.NewLine, resourcesNotFound.Values);
                case "resourceDuplicates":
                    Dictionary<string, IEnumerable<string>> resourceDuplicates =
                        (Dictionary<string, IEnumerable<string>>)Convert.ChangeType(errors, typeof(Dictionary<string, IEnumerable<string>>));

                    string dupMsg = "";
                    foreach (string key in resourceDuplicates.Keys) {
                        // Example:
                        //
                        // Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff:
                        // - D:\Games\Origin\ME3\BioGame\CookedPCConsole\DLC\DLC_MOD_HAIR\CookedPCConsole\BioD_MOD_HAIR1.pcc
                        // - D:\Games\Origin\ME3\BioGame\CookedPCConsole\DLC\DLC_MOD_HAIRS\CookedPCConsole\BioD_MOD_HAIR2.pcc
                        dupMsg += $"{key}:" + Environment.NewLine + string.Join(Environment.NewLine, resourceDuplicates[key]) + Environment.NewLine + Environment.NewLine;
                    }
                    return dupMsg;
                default:
                    return "";
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
                    statusBar.UpdateStatus($"{status[1]} headmorph applied");
                    windowManager.ShowDialogAsync(new CustomMessageBoxViewModel($"{status[1]} headmorph applied succesfully.",
                        "Success", "OK"), null, null);
                    break;
                case "ResourcesNotFound":
                    windowManager.ShowDialogAsync(new ResourceErrorHandlerViewModel(ResourceError.NotFound,
                        status[1] == "female" ? Gender.Female : Gender.Male, status[2]), null, null);
                    break;
                case "ResourceDuplicates":
                    windowManager.ShowDialogAsync(new ResourceErrorHandlerViewModel(ResourceError.Duplicates,
                        status[1] == "female" ? Gender.Female : Gender.Male, status[2]), null, null);
                    break;
            }
        }

        private void Apply_RunWokerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                string info = $"Error when applying to {TargetGame}.";
                if (IsMaleCustom) { info += $"\nMale Custom appearance: {RonMFile}\n{File.ReadAllText(RonMFile)}"; }
                if (IsFemaleCustom) { info += $"\nFemale Custom appearance: {RonFFile}\n{File.ReadAllText(RonFFile)}"; }
                if (!IsMaleCustom) { info += "\nMale Default appearance."; }
                if (!IsFemaleCustom) { info += "\nFemale Default appearance."; }
                Log.Error(info);
                Log.Error($"Error message:\n {e.Error}");

                windowManager.ShowDialogAsync(new ExceptionHandlerViewModel(e.Error), null, null);;
                statusBar.UpdateStatus("");
                IsBusy = false;
            } else if (e.Cancelled) {
                statusBar.UpdateStatus($"The morphs were not applied due to an error");
                IsBusy = false;
            } else {
                IsBusy = false;
            }
        }
    }
}
