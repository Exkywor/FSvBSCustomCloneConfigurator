using Caliburn.Micro;
using FSvBSCustomCloneUtility.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FSvBSCustomCloneUtility
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(new ExceptionHandlerViewModel(e.Exception), null, null);;
            e.Handled = true;
        }
    }
}
