using Caliburn.Micro;
using FSvBSCustomCloneUtility.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FSvBSCustomCloneUtility.ViewModels {
    public class CustomMessageBoxViewModel : Screen {
        public string WindowTitle { get; set; }
        public string WindowHeight { get; set; }
        public string Message { get; set; }
        public string ButtonText { get; set; }

        /// <summary>
        /// Display a MessageBox with the given message 
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="windowTitle">Window title</param>
        /// <param name="buttonText">BUtton text</param>
        public CustomMessageBoxViewModel(string message, string windowTitle, string buttonText) {
            WindowTitle = windowTitle;
            Message = message;
            ButtonText = buttonText;

            Size errSize = Misc.MeasureString(message, "Segoe UI", 12);
            WindowHeight = (Convert.ToDouble(errSize.Height) + 75).ToString();
        }

        public void Button_Clicked() {
            TryCloseAsync();
        }
    }
}
