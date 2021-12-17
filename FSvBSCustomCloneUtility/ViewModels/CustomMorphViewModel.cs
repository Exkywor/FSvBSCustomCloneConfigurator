using Caliburn.Micro;
using FSvBSCustomCloneUtility;
using FSvBSCustomCloneUtility.SharedUI;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.ViewModels;
using FSvBSCustomCloneUtility.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class CustomMorphViewModel : ObserverControl {
        private bool _isValid = false;
        public bool IsValid {
            get { return _isValid; }
        }

        private bool _isFSvBSFileValid = false;
        public bool IsFSvBSFileValid {
            get { return _isFSvBSFileValid; }
        }
        private string _warningFSvBSFile = "";
        public string WarningFSvBSFile {
            get { return _warningFSvBSFile; }
        }

        private bool _isRonFileValid = false;
        public bool IsRonFileValid {
            get { return _isRonFileValid; }
        }
        private string _warningRonFile = "";
        public string WarningRonFile {
            get { return _warningRonFile; }
        }

        private string _targetGame = "";
        public string TargetGame {
            get { return _targetGame; }
            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }

        private string _fsvbsFile = "";
        public string FSvBSFile {
            get { return _fsvbsFile; }
        }

        private string _ronMFile = "";
        public string RonMFile {
            get { return _ronMFile; }
        }
        private string _ronFFile = "";
        public string RonFFile {
            get { return _ronFFile; }
        }


        private List<string> resources = new();

        string customHair = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\LE3\milkykookie_DLC_MOD_FemshepHair\CookedPCConsole\BIOG_HMF_HIR_ANTO.pcc";

        public CustomMorphViewModel() {
            // DataContext = this;

            resources.Add(customHair);

            // MorphWriter writerMale = new(ronFile, targetFile, Gender.Male);
            // writerMale.ApplyMorph();
            // MorphWriter writerFemale = new(ronFileF, targetFile, Gender.Female, new List<string>() { customHair });
            // writerFemale.ApplyMorph();
        }

        public void FSvBSFileButton() {
            OpenFileDialog dlg = PromptForFile("Pcc files (.pcc)|*.pcc");

            if (dlg != null) {
                _fsvbsFile = dlg.FileName;

                bool valid = Validators.ValidateFSvBSFile(_fsvbsFile, TargetGame);
                if (!valid) {
                    _warningFSvBSFile = "Invalid file. Select a file that matches the target game, and is the one indicated in the instructions.";
                    _isFSvBSFileValid = false;
                    NotifyOfPropertyChange(() => WarningFSvBSFile);
                    NotifyOfPropertyChange(() => FSvBSFile);
                    CheckIfApply();
                    return;
                }

                _warningFSvBSFile = "";
                _isFSvBSFileValid = true;
                NotifyOfPropertyChange(() => WarningFSvBSFile);
                NotifyOfPropertyChange(() => FSvBSFile);
                CheckIfApply();
            }
        }

        public void RonMFileButton() {
            OpenFileDialog dlg = PromptForFile("Ron files (.ron)|*.ron");

            if (dlg != null) {
                _ronMFile = dlg.FileName;
                NotifyOfPropertyChange(() => RonMFile);
                _isRonFileValid = true;
                CheckIfApply();
            }
        }
        public void RonFFileButton() {
            OpenFileDialog dlg = PromptForFile("Ron files (.ron)|*.ron");

            if (dlg != null) {
                _ronFFile = dlg.FileName;
                NotifyOfPropertyChange(() => RonFFile);
                _isRonFileValid = true;
                CheckIfApply();
            }
        }

        private void CheckIfApply() {
            _isValid = (IsFSvBSFileValid && IsRonFileValid);
            NotifyOfPropertyChange(() => IsValid);
        }

        public override void Update(string property, string value) {
            switch (property) {
                case "TargetGame":
                    TargetGame = value;
                    // Revalidate all files
                    break;
            }
        }

        private OpenFileDialog PromptForFile(string filter) {
            OpenFileDialog dlg = new();
            dlg.Filter = filter;
            bool? result = dlg.ShowDialog();

            if (result != true) { return null; }
            else { return dlg; }
        }
    }
}
