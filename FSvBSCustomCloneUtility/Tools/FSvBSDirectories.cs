using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Static methods to get mod directory information from an MEGame
    /// </summary>
    public static class FSvBSDirectories {
        /// <summary>
        /// Gets the path to the FSvBS folder for ME3
        /// </summary>
        public static string ME3ModPath => GetModPath(MEGame.ME3);
        /// <summary>
        /// Gets the path to the FSvBS folder for LE3
        /// </summary>
        public static string LE3ModPath => GetModPath(MEGame.LE3);

        /// <summary>
        /// Gets the path to the FSvBS folder for the input game
        /// </summary>
        /// <param name="game">Game to get the path for</param>
        /// <returns>Path to the FSvBS folder, null if the mod is not installed</returns>
        public static string GetModPath(MEGame game) {
            if (!IsModInstalled(game)) { return null; }
            else { return Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\DLC_MOD_FSvBS{(game == MEGame.ME3 ? "" : "LE")}"); }
        }

        /// <summary>
        /// Gets the path to the dummies file for ME3
        /// </summary>
        public static string ME3DummiesPath => GetDummiesPath(MEGame.ME3);
        /// <summary>
        /// Gets the path to the dummies file for LE3
        /// </summary>
        public static string LE3DummiesPath => GetDummiesPath(MEGame.LE3);

        /// <summary>
        /// Gets the path to the dummies file for the input game
        /// </summary>
        /// <param name="game">Game to get the path for</param>
        /// <returns>Path to the dummies file, null if the file version is not valid</returns>
        public static string GetDummiesPath(MEGame game) {
            string dummies = Path.Combine(GetModPath(game), @"CookedPCConsole\BioD_FSvBS_Dummies.pcc");
            if (!IsValidDummies(game, dummies)) { return null; }
            else { return dummies; }
        }

        /// <summary>
        /// Gets the path to the coalesced binary file for the ME3 mod
        /// </summary>
        public static string ME3BinPath => GetBinPath(MEGame.ME3);
        /// <summary>
        /// Gets the path to the coalesced binary file for the LE3 mod
        /// </summary>
        public static string LE3BinPath => GetBinPath(MEGame.LE3);

        /// <summary>
        /// Gets the path to the coalesced binary file for the input game mod
        /// </summary>
        /// <param name="game">Game to get the path for</param>
        /// <returns>Path to the coalesced binary file, null if the file is not found</returns>
        public static string GetBinPath(MEGame game) {
            string file = Path.Combine(GetModPath(game), $@"Default_DLC_MOD_FSvBS{(game == MEGame.ME3 ? "" : "LE")}.bin");
            if (!File.Exists(file)) { return null; }
            else { return file; }
        }

        /// <summary>
        /// Checks if the mod is installed for the input game
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <returns>True if the mod is installed</returns>
        public static bool IsModInstalled(MEGame game) {
            if (!Directory.Exists(Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\DLC_MOD_FSvBS{(game == MEGame.ME3 ? "" : "LE")}"))) {
                return false;
            } else { return true; }
        }

        /// <summary>
        /// Checks if the dummies file is valid for the input game
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <param name="dummies">Dummies file path</param>
        /// <returns>True if the dummies is valid</returns>
        public static bool IsValidDummies(MEGame game, string dummies) {
            using IMEPackage file = MEPackageHandler.OpenMEPackage(dummies);
            // File does not contain the identifier, meaning it's from an older version of the mod
            if (!file.Names.Contains($"FSvBS_{(game == MEGame.ME3 ? "ME3" : "LE3")}_DummiesFile")) {
                return false;
            } else { return true; }
        }
    }
}
