// Code adaptded from
// https://github.com/ME3Tweaks/LegendaryExplorer/blob/Beta/LegendaryExplorer/LegendaryExplorer/Dialogs/ExceptionHandlerDialog.xaml.cs

using Caliburn.Micro;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FSvBSCustomCloneUtility.ViewModels {
    /// <summary>
    /// Interaction logic for ExceptionHandlerDialogWPF.xaml
    /// </summary>
    public class ExceptionHandlerViewModel : Screen {
        public string ExceptionStackTrace { get; set; }
        public string ExceptionMessage { get; set; }
        public string WindowHeight { get; set; }
        public string ExceptionHeight { get; set; }

        public ExceptionHandlerViewModel(Exception exception) {
            string flattened = exception.FlattenException();
            ExceptionStackTrace = flattened;
            ExceptionMessage = exception.Message;

            Size errSize = Misc.MeasureString(ExceptionStackTrace, "Consolas", 12);
            ExceptionHeight = Math.Min(450, errSize.Height + 100).ToString();
            WindowHeight = (Convert.ToDouble(ExceptionHeight) + 200).ToString();
        }

        public void Quit() {
            Environment.Exit(1);
        }

        public void Continue() {
            TryCloseAsync();
        }

        public void Copy() {
            try { Clipboard.SetText(ExceptionStackTrace); }
            catch (Exception) { }
        }

        public void NewIssue() {
            Process.Start(new ProcessStartInfo {
                FileName = $"https://github.com/Exkywor/FSvBSCustomCloneUtility/issues/new",
                UseShellExecute = true
            });
        }

    }
}

