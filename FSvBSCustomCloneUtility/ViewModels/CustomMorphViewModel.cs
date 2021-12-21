using Caliburn.Micro;
using FSvBSCustomCloneUtility;
using FSvBSCustomCloneUtility.SharedUI;
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

        public CustomMorphViewModel() {
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

        public override void Update(string property, string value1, string value2 = "") {
            switch (property) {
                case "TargetGame":
                    TargetGame = value1 == "ME3" ? MEGame.ME3 : MEGame.LE3;
                    CheckIfApply();
                    break;
            }
        }
    }
}
