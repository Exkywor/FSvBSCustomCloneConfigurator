using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FSvBSCustomCloneUtility.Controls {
    public abstract class ObserverControl : PropertyChangedBase {
        public abstract void Update(string property, string value);
    }
}
