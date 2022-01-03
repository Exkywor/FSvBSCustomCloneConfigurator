using FSvBSCustomCloneUtility.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class StatusBarViewModel : ObserverControl {
        private string _statusMessage = "This is a test message";
        public string StatusMessage {
            get { return _statusMessage; }
            set {
                _statusMessage = value;
                NotifyOfPropertyChange(() => StatusMessage);
            }
        }

        public override void Update<Type>(string name, Type value) {
            switch (name) {
                case "SetStatus":
                    StatusMessage = (string) Convert.ChangeType(value, typeof(string));
                    break;
            }
        }
    }
}
