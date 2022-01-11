using Caliburn.Micro;
using FSvBSCustomCloneUtility.ViewModels;
using Serilog;
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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("FSvBSC3_log_.txt", rollingInterval: RollingInterval.Hour)
                .CreateLogger();
            
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            Log.Error(e.Exception.ToString());
            
            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(new ExceptionHandlerViewModel(null), null, null);;
            e.Handled = true;
        }
    }
}
