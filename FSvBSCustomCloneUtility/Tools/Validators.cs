using Caliburn.Micro;
using FSvBSCustomCloneUtility.ViewModels;
using FSvBSCustomCloneUtility.Tools;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Static methods to validate paths, mods, and files
    /// </summary>
    public static class Validators {
        private static IWindowManager windowManager = new WindowManager();

        /// <summary>
        /// Validate that the path points to a full game installation
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <param name="path">Path to validate</param>
        /// <returns>Null if no errors were found, else the error message</returns>
        /// Adapted from https://github.com/ME3Tweaks/ME3TweaksModManager/blob/master/MassEffectModManagerCore/modmanager/objects/GameTarget.cs
        public static string ValidatePath(MEGame game, string path) {
            List<string> validationFiles = new();

            if (game.IsOTGame()) {
                validationFiles.AddRange(new List<string> {
                        Path.Combine(path, @"Binaries", @"Win32", @"MassEffect3.exe"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"Textures.tfc"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"Startup.pcc"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"Coalesced.bin"),
                        Path.Combine(path, @"BioGame", @"Patches", @"PCConsole", @"Patch_001.sfar"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"Textures.tfc"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"citwrd_rp1_bailey_m_D_Int.afc")
                    });
            } else if (game.IsLEGame()){
                validationFiles.AddRange(new List<string> {
                        Path.Combine(path, @"Binaries", @"Win64", @"MassEffect3.exe"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"Textures1.tfc"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"Startup.pcc"),
                        Path.Combine(path, @"BioGame", @"DLC", @"DLC_CON_PRO3", @"CookedPCConsole", @"DLC_CON_PRO3_INT.tlk"),
                        Path.Combine(path, @"BioGame", @"DLC", @"DLC_CON_END", @"CookedPCConsole", @"BioD_End001_910RaceToConduit.pcc"),
                        Path.Combine(path, @"BioGame", @"CookedPCConsole", @"citwrd_rp1_bailey_m_D_Int.afc")
                    });
            } else {
                return "The selected game is invalid";
            }

            foreach (string f in validationFiles) {
                if (!File.Exists(f)) { return $"Invalid target. Missing file {Path.GetFileName(f)}"; }
            }

            return null;
        }

        /// <summary>
        /// Validate that the mod is installed in its compatible version for the input game
        /// </summary>
        /// <param name="game">Target game</param>
        /// <returns>Null if no errors were found, else the error message</returns>
        public static string ValidateMod(MEGame game) {
            if (!FSvBSDirectories.IsModInstalled(game)) {
                return "The FemShep v BroShep mod was not found. Make sure to install the mod before running this tool.";
            } else if (!FSvBSDirectories.IsValidDummies(game, FSvBSDirectories.GetDummiesPath(game))) {
                return "The FemShep v BroShep mod version is incompatible. Make sure to have version 1.1.0 or higher installed.";
            } else {
                return null;
            }
        }
    }
}
