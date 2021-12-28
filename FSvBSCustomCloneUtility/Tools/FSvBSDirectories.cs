using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Static methods to get mod directory information from an MEGame
    /// </summary>
    public static class FSvBSDirectories {
        /// <summary>
        /// Files that contain instances of the clone
        /// </summary>
        private static readonly string[] CLONE_INSTANCES_FILENAMES = {
            "BioD_Cit002_700Exit.pcc",
            "BioD_Cit003.pcc",
            "BioD_Cit003_815Final_RR2.pcc",
            "BioD_Cit004_210CICIntro.pcc",
            "BioD_Cit004_272MaleClone.pcc",
            "BioD_Cit004_273FemClone.pcc"
        };

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
        /// Gets the path to the FSvBS CookedPCConsole folder for ME3
        /// </summary>
        public static string ME3CookedPath => GetCookedPath(MEGame.ME3);
        /// <summary>
        /// Gets the path to the FSvBS CookedPCConsole folder for LE3
        /// </summary>
        public static string LE3CookedPath => GetCookedPath(MEGame.LE3);
        /// <summary>
        /// Gets the path for the FSvBS CookedPCConsole folder for the input game
        /// </summary>
        /// <param name="game">Game to get the path for</param>
        /// <returns>Path to the FSvBS CookedPCConsole folder, null if the mod is not installed</returns>
        public static string GetCookedPath(MEGame game) {
            if (!IsModInstalled(game)) { return null; }
            else { return Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\DLC_MOD_FSvBS{(game == MEGame.ME3 ? "" : "LE")}\CookedPCConsole"); }
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
            string file = Path.Combine(GetCookedPath(game), $@"Default_DLC_MOD_FSvBS{(game == MEGame.ME3 ? "" : "LE")}.bin");
            if (!File.Exists(file)) { return null; }
            else { return file; }
        }

        /// <summary>
        /// Gets the path to the clean folder for the ME3 mod
        /// </summary>
        public static string ME3CleanPath => GetCleanPath(MEGame.ME3);
        /// <summary>
        /// Gets the path to the clean folder for the LE3 mod
        /// </summary>
        public static string LE3CleanPath => GetCleanPath(MEGame.LE3);

        /// <summary>
        /// Gets the path to the clean folder for the input game mod
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetCleanPath(MEGame game) {
            string folder = Path.Combine(GetCookedPath(game), "Clean");
            if (!Directory.Exists(folder)) { return null; }
            else { return folder; }
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
            string dummies = Path.Combine(GetCookedPath(game), @"BioD_FSvBS_Dummies.pcc");
            if (!IsValidDummies(game, dummies)) { return null; }
            else { return dummies; }
        }

        /// <summary>
        /// Gets the path to the clean dummies file for the input game
        /// </summary>
        /// <param name="game">Game to get the path for</param>
        /// <returns>Path to the clean dummies file, null if the file version is not valid</returns>
        public static string GetCleanDummiesPath(MEGame game) {
            string dummiesClean = Path.Combine(GetCleanPath(game), "BioD_FSvBS_Dummies_Clean.pcc");
            if (!IsValidDummies(game, dummiesClean)) { return null; }
            else { return dummiesClean; }
        }

        /// <summary>
        /// Gets te paths of the FSvBS files that contain instances of the clone in ME3
        /// </summary>
        public static List<string> ME3CloneInstancesPaths => GetCloneInstancesPaths(MEGame.ME3);
        /// <summary>
        /// Gets te paths of the FSvBS files that contain instances of the clone in LE3
        /// </summary>
        public static List<string> LE3CloneInstancesPaths => GetCloneInstancesPaths(MEGame.LE3);
        /// <summary>
        /// Gets the paths of the FSvBS files that contain instances of the clone for the input game
        /// </summary>
        /// <param name="game">Game to get the paths for</param>
        /// <returns>List of file paths containing instances of the clone</returns>
        public static List<string> GetCloneInstancesPaths(MEGame game) {
            List<string> paths = new();

            foreach (string file in CLONE_INSTANCES_FILENAMES) {
                paths.Add(Path.Combine(GetCookedPath(game), file));
            }

            return paths;
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

        /// <summary>
        /// Replace the dummies file with a clean version
        /// </summary>
        /// <param name="game">Game to replace for</param>
        public static void ApplyCleanDummies(MEGame game) {
            File.Copy(GetCleanDummiesPath(game), GetDummiesPath(game), true);
        }

        /// <summary>
        /// Replace the files that contain the clone with clean versions
        /// </summary>
        /// <param name="game">Game to replace for</param>
        public static void ApplyCleanClone(MEGame game) {
            foreach(string file in GetCloneInstancesPaths(game)) {
                File.Copy(Path.Combine(GetCleanPath(game), $"{Path.GetFileNameWithoutExtension(file)}_Clean.pcc"), file, true);
            }
        }

        /// <summary>
        /// Replace all files that can be cleaned with clean versions
        /// </summary>
        /// <param name="game">Game to replace for</param>
        public static void ApplyCleanFiles(MEGame game) {
            ApplyCleanDummies(game);
            ApplyCleanClone(game);
        }
    }
}
