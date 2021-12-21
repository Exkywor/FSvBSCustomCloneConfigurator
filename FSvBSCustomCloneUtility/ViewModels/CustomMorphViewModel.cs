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
        }

        private bool _isRonFileValid = false;
        private string _warningRonFile = "";
        public string WarningRonFile {
            get { return _warningRonFile; }
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

        public void RonMFileButton() {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", "Select male headmorph");

            if (!string.IsNullOrEmpty(file)) {
                _ronMFile = file;
                NotifyOfPropertyChange(() => RonMFile);
                _isRonFileValid = true;
                CheckIfApply();
            }
        }
        public void RonFFileButton() {
            string file = Misc.PromptForFile("Ron files (.ron)|*.ron", "Select female headmorph");

            if (!string.IsNullOrEmpty(file)) {
                _ronFFile = file;
                NotifyOfPropertyChange(() => RonFFile);
                _isRonFileValid = true;
                CheckIfApply();
            }
        }

        private void CheckIfApply() {
            _isValid = (_isRonFileValid);
            NotifyOfPropertyChange(() => IsValid);
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
