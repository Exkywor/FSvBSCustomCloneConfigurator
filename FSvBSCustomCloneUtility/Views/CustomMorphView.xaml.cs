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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FSvBSCustomCloneUtility.Views
{
    /// <summary>
    /// Interaction logic for CustomMorphPage.xaml
    /// </summary>
    public partial class CustomMorphView : UserControl
    {
        public CustomMorphView()
        {
            LoadCommands();
            InitializeComponent();

            DataContext = this;
        }

        public ICommand SelectTargetCommand { get; set; }

        private void LoadCommands()
        {
            SelectTargetCommand = new GenericCommand(SelectTargetFile);
        }

        private void SelectTargetFile()
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
