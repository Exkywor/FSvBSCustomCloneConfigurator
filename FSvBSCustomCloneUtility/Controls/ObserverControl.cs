using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FSvBSCustomCloneUtility.Controls {
    /// <summary>
    /// Abstract class for observable controls that need to access NotifyOnPropertyChange
    /// </summary>
    public abstract class ObserverControl : PropertyChangedBase {
        /// <summary>
        /// Inform the observer that the observable has had an update
        /// </summary>
        /// <typeparam name="Type">Type of the updated property from the observable</typeparam>
        /// <param name="name">The name of the updated property</param>
        /// <param name="value">The value of the updated property</param>
        public abstract void Update<Type>(string name, Type value);
    }
}
