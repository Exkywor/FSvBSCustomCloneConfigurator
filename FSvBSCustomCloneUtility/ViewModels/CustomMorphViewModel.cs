using Caliburn.Micro;
using FSvBSCustomCloneUtility;
using FSvBSCustomCloneUtility.SharedUI;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FSvBSCustomCloneUtility.ViewModels
{
    public class CustomMorphViewModel : UserControl
    {
        public CustomMorphViewModel()
        {
            DataContext = this;
        }

        public void SelectTarget()
        {
            OpenFileDialog dlg = new();
            dlg.Filter = "Pcc files (.pcc)|*.pcc";

            bool? result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

        }
    }
}
