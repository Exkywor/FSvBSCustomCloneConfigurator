using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class ConditionalsControlViewModel : ObserverControl {
        private List<ObserverControl> _observers = new();

        private MEGame? _targetGame = null;
        public MEGame? TargetGame {
            get { return _targetGame; }
            set {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }

        private bool _isMaleDefault = false;
        public bool IsMaleDefault {
            get { return _isMaleDefault; }
            set {
                _isMaleDefault = value;
                NotifyOfPropertyChange(() => IsMaleDefault);
            }
        }
        private bool _isMaleCustom = false;
        public bool IsMaleCustom {
            get { return _isMaleCustom; }
            set {
                _isMaleCustom = value;
                NotifyOfPropertyChange(() => IsMaleCustom);
            }
        }

        private bool _isFemaleDefault = false;
        public bool IsFemaleDefault {
            get { return _isFemaleDefault; }
            set {
                _isFemaleDefault = value;
                NotifyOfPropertyChange(() => IsFemaleDefault);
            }
        }
        private bool _isFemaleCustom = false;
        public bool IsFemaleCustom {
            get { return _isFemaleCustom; }
            set {
                _isFemaleCustom = value;
                NotifyOfPropertyChange(() => IsFemaleCustom);
            }
        }

        public ConditionalsControlViewModel() { }

        public ConditionalsControlViewModel(List<ObserverControl> observers) {
            _observers = observers;
        }

        public void SetMaleDefault() {
            IsMaleDefault = true;
            IsMaleCustom = false;
            Notify("ProcessUpdate", true);
            ConditionalsManager.SetConditional(Gender.Male, false, (MEGame) TargetGame);
            Notify("SetStatus", "Set male clone to default appearance");
            Notify("ProcessUpdate", false);
        }
        public void SetMaleCustom() {
            IsMaleDefault = false;
            IsMaleCustom = true;
            Notify("ProcessUpdate", true);
            ConditionalsManager.SetConditional(Gender.Male, true, (MEGame) TargetGame);
            Notify("SetStatus", "Set male clone to custom appearance");
            Notify("ProcessUpdate", false);
        }

        public void SetFemaleDefault() {
            IsFemaleDefault = true;
            IsFemaleCustom = false;
            Notify("ProcessUpdate", true);
            ConditionalsManager.SetConditional(Gender.Female, false, (MEGame) TargetGame);
            Notify("SetStatus", "Set female clone to default appearance");
            Notify("ProcessUpdate", false);
        }
        public void SetFemaleCustom() {
            IsFemaleDefault = false;
            IsFemaleCustom = true;
            Notify("ProcessUpdate", true);
            ConditionalsManager.SetConditional(Gender.Female, true, (MEGame) TargetGame);
            Notify("SetStatus", "Set female clone to custom appearance");
            Notify("ProcessUpdate", false);
        }

        /// <summary>
        /// Update the state of the UI to match the coalesced
        /// </summary>
        private void UpdateState() {
            if (TargetGame == null) { return; }

            bool isMaleCustom = ConditionalsManager.CheckConditional(Gender.Male, (MEGame) TargetGame);
            bool isFemaleCustom = ConditionalsManager.CheckConditional(Gender.Female, (MEGame) TargetGame);

            if (isMaleCustom) { SetMaleCustom(); }
            else { SetMaleDefault(); }

            if (isFemaleCustom) { SetFemaleCustom(); }
            else { SetFemaleDefault(); }
        }

        protected override void SetButtonsState() {
            ButtonsEnabled = TargetGame != null && !IsBusy;
        }

        private void Notify<Type>(string name, Type value) {
            foreach(ObserverControl observer in _observers) {
                observer.Update(name, value);
            }
        }

        public override void Update<Type>(string name, Type value) {
            switch (name) {
                case "TargetGame":
                    TargetGame = (MEGame) Convert.ChangeType(value, typeof(MEGame));
                    UpdateState();
                    SetButtonsState();
                    break;
                case "ClearConds":
                    SetMaleDefault();
                    SetFemaleDefault();
                    break;
                case "Apply":
                    string gender = (string) Convert.ChangeType(value, typeof(string));
                    if (gender == "M") { SetMaleCustom(); }
                    else if (gender == "F") { SetFemaleCustom(); }
                    break;
                case "ProcessUpdate":
                    IsBusy = (bool) Convert.ChangeType(value, typeof(bool));
                    break;
            }
        }
    }
}
