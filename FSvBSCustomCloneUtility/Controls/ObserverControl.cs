using Caliburn.Micro;

namespace FSvBSCustomCloneUtility.Controls {
    /// <summary>
    /// Abstract class for observable controls that need to access NotifyOnPropertyChange
    /// </summary>
    public abstract class ObserverControl : PropertyChangedBase {
        protected IWindowManager windowManager = new WindowManager();

        private bool _isBusy = false;
        protected bool IsBusy {
            get { return _isBusy; }
            set {
                _isBusy = value;
                SetButtonsState();
            }
        }

        private bool _buttonsEnabled = false;
        public bool ButtonsEnabled {
            get { return _buttonsEnabled; }
            set {
                _buttonsEnabled = value;
                NotifyOfPropertyChange(() => ButtonsEnabled);
            }
        }

        /// <summary>
        /// Set state of the buttons
        /// </summary>
        protected abstract void SetButtonsState();

        /// <summary>
        /// Inform the observer that the observable has had an update
        /// </summary>
        /// <typeparam name="Type">Type of the updated property from the observable</typeparam>
        /// <param name="name">The name of the updated property</param>
        /// <param name="value">The value of the updated property</param>
        public abstract void Update<Type>(string name, Type value);
    }
}
