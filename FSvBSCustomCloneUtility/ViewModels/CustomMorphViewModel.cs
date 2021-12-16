using Caliburn.Micro;
using FSvBSCustomCloneUtility;
using FSvBSCustomCloneUtility.SharedUI;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.ViewModels;
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

        string ronFile = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\ME3\CluckenDip.ron";
        string ronFileF = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\ME3\Exkywor_Natylaz.ron";
        string targetFile = @"E:\Origin\Mass Effect 3\BIOGame\DLC\DLC_MOD_FSvBS\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
        string targetFileLE = @"D:\Games\Origin\Mass Effect Legendary Edition\Game\ME3\BioGame\DLC\DLC_MOD_FSvBSLE\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
        string customHair = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\LE3\milkykookie_DLC_MOD_FemshepHair\CookedPCConsole\BIOG_HMF_HIR_ANTO.pcc";

        public CustomMorphViewModel() {
            // DataContext = this;

            List<string> resources = new();
            resources.Add(customHair);

            // MorphWriter writerMale = new(ronFile, targetFile, Gender.Male);
            // writerMale.ApplyMorph();
            // MorphWriter writerFemale = new(ronFileF, targetFile, Gender.Female, new List<string>() { customHair });
            // writerFemale.ApplyMorph();
        }

        public void SelectTarget() {
            OpenFileDialog dlg = new();
            dlg.Filter = "Pcc files (.pcc)|*.pcc";

            bool? result = dlg.ShowDialog();

            if (result != true) { return; }
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
