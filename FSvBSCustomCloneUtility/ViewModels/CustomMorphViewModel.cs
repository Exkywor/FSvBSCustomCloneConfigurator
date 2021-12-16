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
        private string _targetGame = "";
        public string TargetGame {
            get { return _targetGame; }
            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }
        private string _ronFile = "";
        public string RonFile {
            get { return _ronFile; }
        }

        private string _fsvbsFile = "";
        public string FSvBSFile {
            get { return _fsvbsFile; }
        }

        private List<string> resources = new();

        string ronFile = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\ME3\CluckenDip.ron";
        string ronFileF = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\ME3\Exkywor_Natylaz.ron";
        string targetFile = @"E:\Origin\Mass Effect 3\BIOGame\DLC\DLC_MOD_FSvBS\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
        string targetFileLE = @"D:\Games\Origin\Mass Effect Legendary Edition\Game\ME3\BioGame\DLC\DLC_MOD_FSvBSLE\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
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
            OpenFileDialog dlg = new();
            dlg.Filter = "Pcc files (.pcc)|*.pcc";

            bool? result = dlg.ShowDialog();

            if (result != true) { return; }
            else {
                _fsvbsFile = dlg.FileName;
                NotifyOfPropertyChange(() => FSvBSFile);
            }
        }

        public void RonFileButton() {
            OpenFileDialog dlg = new();
            dlg.Filter = "Ron files (.ron)|*.ron";

            bool? result = dlg.ShowDialog();

            if (result != true) { return; }
            else {
                _ronFile = dlg.FileName;
                NotifyOfPropertyChange(() => RonFile);
            }
        }

        public override void Update(string property, string value) {
            switch (property) {
                case "TargetGame":
                    TargetGame = value;
                    break;
            }
        }
    }
}
