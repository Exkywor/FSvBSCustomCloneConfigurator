using Caliburn.Micro;
using FSvBSCustomCloneUtility.InterfacesAndClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class StatusBarViewModel : StatusBar {
        private string _statusMessage = "";
        public string StatusMessage {
            get { return _statusMessage; }
            set {
                _statusMessage = value;
                NotifyOfPropertyChange(() => StatusMessage);
            }
        }

        public override void UpdateStatus(string status) {
            StatusMessage = status;
        }
    }
}
