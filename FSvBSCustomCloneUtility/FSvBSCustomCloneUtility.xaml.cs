using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore;
using LegendaryExplorerCore.Packages;
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

namespace FSvBSCustomCloneUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ronFile = @"C:\Users\ferna\Desktop\morph.ron";
        string targetFile = @"E:\Origin\Mass Effect 3\BIOGame\DLC\DLC_MOD_FSvBS\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
        string customHair = @"C:\Users\ferna\Desktop\hair.pcc";

        public MainWindow()
        {
            InitializeComponent();
            initCoreLib();

            var resources = new List<string>();
            resources.Add(customHair);
            var writer = new MorphWriter(ronFile,targetFile,resources);
            writer.ApplyMorph();
        }

        /// <summary>
        /// Initialize Legendary Explorer Core Library
        /// </summary>
        private static void initCoreLib()
        {
            static void packageSaveFailed(string message)
            {
                // I'm not sure if this requires ui thread since it's win32 but i'll just make sure
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(message);
                });
            }

            LegendaryExplorerCoreLib.InitLib(TaskScheduler.FromCurrentSynchronizationContext(), packageSaveFailed);
        }
    }
}
