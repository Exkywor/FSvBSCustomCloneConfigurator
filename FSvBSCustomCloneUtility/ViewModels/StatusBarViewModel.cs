using FSvBSCustomCloneUtility.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class StatusBarViewModel : ObserverControl {
        private string _statusMessage = "";
        public string StatusMessage {
            get { return _statusMessage; }
            set {
                _statusMessage = value;
                NotifyOfPropertyChange(() => StatusMessage);
            }
        }

        protected override void SetButtonsState() {} // Yes, I know this is refused bequest

        public override void Update<Type>(string name, Type value) {
            switch (name) {
                case "SetStatus":
                    StatusMessage = (string) Convert.ChangeType(value, typeof(string));
                    break;
            }
        }
    }
}
