using Caliburn.Micro;
using FSvBSCustomCloneUtility;
using FSvBSCustomCloneUtility.SharedUI;
using FSvBSCustomCloneUtility.Controls;
using FSvBSCustomCloneUtility.ViewModels;
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
    public class CustomMorphViewModel : ObserverControl
    {
        private string _targetGame = "";
        public string TargetGame
        {
            get
            {
                return _targetGame;
            }
            set
            {
                _targetGame = value;
                NotifyOfPropertyChange(() => TargetGame);
            }
        }

        public CustomMorphViewModel()
        {
            // DataContext = this;
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

        public override void Update(string property, string value)
        {
            switch (property)
            {
                case "TargetGame":
                    TargetGame = value;
                    break;
            }
        }
    }
}
