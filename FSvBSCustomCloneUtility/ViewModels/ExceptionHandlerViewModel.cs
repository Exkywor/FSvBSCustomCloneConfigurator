using System;
// Code copied from
// https://github.com/ME3Tweaks/LegendaryExplorer/blob/Beta/LegendaryExplorer/LegendaryExplorer/Dialogs/ExceptionHandlerDialog.xaml.cs

using Caliburn.Micro;
using LegendaryExplorerCore.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FSvBSCustomCloneUtility.ViewModels {
    /// <summary>
    /// Interaction logic for ExceptionHandlerDialogWPF.xaml
    /// </summary>
    public class ExceptionHandlerViewModel : Screen {
        private string _exceptionStackTrace = "Exception message goes here.";
        public string ExceptionStackTrace {
            get { return _exceptionStackTrace; }
            set {
                _exceptionStackTrace = value;
                NotifyOfPropertyChange(() => ExceptionStackTrace);
            }
        }
        private string _exceptionMessage = "Object instance not set to a reference.";
        public string ExceptionMessage {
            get { return _exceptionMessage; }
            set {
                _exceptionMessage = value;
                NotifyOfPropertyChange(() => ExceptionMessage);
            }
        }

        public ExceptionHandlerViewModel(Exception exception) {
            string flattened = exception.FlattenException();
            ExceptionStackTrace = flattened;
            ExceptionMessage = exception.Message;
        }

        public void Quit(object sender, RoutedEventArgs e) {
            Environment.Exit(1);
        }

        public void Continue(object sender, RoutedEventArgs e) {
            TryCloseAsync();
        }

        public void Copy(object sender, RoutedEventArgs e) {
            try {
                Clipboard.SetText(ExceptionStackTrace);
            } catch (Exception) {
                //what are we going to do. Crash on the error dialog?
            }
        }
    }
}

