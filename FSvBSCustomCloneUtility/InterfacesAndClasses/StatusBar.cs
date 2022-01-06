using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.InterfacesAndClasses {
    /// <summary>
    /// Interface for status bar
    /// </summary>
    public abstract class StatusBar : PropertyChangedBase {
        /// <summary>
        /// Update the status to the input status
        /// </summary>
        /// <param name="status">New status to set</param>
        public abstract void UpdateStatus(string status);
    }
}
