using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class ConditionalsControlViewModel : ObserverControl {
        private const string BUTTONSELECTEDCOLOR = "#5c5f72";
        private const string BUTTONDEFAULTCOLOR = "#3e3d4b";

        private MEGame? _targetGame = null;
        public MEGame? TargetGame {
            get { return _targetGame; }
            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }

        private bool _isTargetSet = false;
        public bool IsTargetSet {
            get { return _isTargetSet; }
            set {
                _isTargetSet = value;
                NotifyOfPropertyChange(() => IsTargetSet);
            }
        }

        // BUTTON COLOR CONTROLS
        private string _maleDefaultColor = BUTTONDEFAULTCOLOR;
        private string _maleCustomColor = BUTTONDEFAULTCOLOR;
        public string MaleDefaultColor {
            get { return _maleDefaultColor; }
            set {
                _maleDefaultColor = value;
                NotifyOfPropertyChange(() => MaleDefaultColor);
            }
        }
        public string MaleCustomColor {
            get { return _maleCustomColor; }
            set {
                _maleCustomColor = value;
                NotifyOfPropertyChange(() => MaleCustomColor);
            }
        }

        private string _femaleDefaultColor = BUTTONDEFAULTCOLOR;
        private string _femaleCustomColor = BUTTONDEFAULTCOLOR;
        public string FemaleDefaultColor {
            get { return _femaleDefaultColor; }
            set {
                _femaleDefaultColor = value;
                NotifyOfPropertyChange(() => FemaleDefaultColor);
            }
        }
        public string FemaleCustomColor {
            get { return _femaleCustomColor; }
            set {
                _femaleCustomColor = value;
                NotifyOfPropertyChange(() => FemaleCustomColor);
            }
        }

        // BUTTON CONTROLS
        public void MaleDefault() {
            MaleDefaultColor = BUTTONSELECTEDCOLOR;
            MaleCustomColor = BUTTONDEFAULTCOLOR;
            ConditionalsManager.SetConditional(Gender.Male, false, (MEGame) TargetGame);
        }
        public void MaleCustom() {
            MaleDefaultColor = BUTTONDEFAULTCOLOR;
            MaleCustomColor = BUTTONSELECTEDCOLOR;
            ConditionalsManager.SetConditional(Gender.Male, true, (MEGame) TargetGame);
        }

        public void FemaleDefault() {
            FemaleDefaultColor = BUTTONSELECTEDCOLOR;
            FemaleCustomColor = BUTTONDEFAULTCOLOR;
            ConditionalsManager.SetConditional(Gender.Female, false, (MEGame) TargetGame);
        }
        public void FemaleCustom() {
            FemaleDefaultColor = BUTTONDEFAULTCOLOR;
            FemaleCustomColor = BUTTONSELECTEDCOLOR;
            ConditionalsManager.SetConditional(Gender.Female, true, (MEGame) TargetGame);
        }

        public override void Update<Type>(string name, Type value) {
            switch (name) {
                case "TargetGame":
                    TargetGame = (MEGame) Convert.ChangeType(value, typeof(MEGame));
                    IsTargetSet = true;

                    bool isMaleCustom = ConditionalsManager.CheckConditional(Gender.Male, (MEGame) TargetGame);
                    bool isFemaleCustom = ConditionalsManager.CheckConditional(Gender.Female, (MEGame) TargetGame);

                    if (isMaleCustom) { MaleCustom(); }
                    else { MaleDefault(); }

                    if (isFemaleCustom) { FemaleCustom(); }
                    else { FemaleDefault(); }

                    break;
                default:
                    break;
            }
        }
    }
}
