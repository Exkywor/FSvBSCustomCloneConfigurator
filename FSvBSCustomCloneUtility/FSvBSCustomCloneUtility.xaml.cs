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
    public enum Gender
    {
        Male,
        Female
    }

    public partial class MainWindow : Window
    {
        string ronFile = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\ME3\VoodooLilium_Boss.ron";
        string ronFileF = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\ME3\VoodooLilium_Minmi.ron";
        string targetFile = @"E:\Origin\Mass Effect 3\BIOGame\DLC\DLC_MOD_FSvBS\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
        string targetFileLE = @"D:\Games\Origin\Mass Effect Legendary Edition\Game\ME3\BioGame\DLC\DLC_MOD_FSvBSLE\CookedPCConsole\BioD_FSvBS_Dummies.pcc";
        string customHair = @"D:\Ministerio\dev\modding\Mass Effect\mods\Counter Clone\project\ron and saves\LE3\milkykookie_DLC_MOD_FemshepHair\CookedPCConsole\BIOG_HMF_HIR_ANTO.pcc";


        public MainWindow()
        {
            InitializeComponent();
            initCoreLib();

            var resources = new List<string>();
            resources.Add(customHair);
            var writerMale = new MorphWriter(ronFile, targetFile, Gender.Male);
            writerMale.ApplyMorph();
            var writerFemale = new MorphWriter(ronFileF, targetFile, Gender.Female, new List<string>() { customHair });
            writerFemale.ApplyMorph();
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
