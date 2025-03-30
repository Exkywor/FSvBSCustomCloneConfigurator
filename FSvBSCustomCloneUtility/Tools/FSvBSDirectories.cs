using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Unreal;
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
        /// Check if the mod is the Vanilla vs version
        /// </summary>
        /// <param name="game">Game to search for</param>
        /// <returns>True if mod is the Vanilla vs version</returns>
        public static bool IsVvs(MEGame game) {
            // Check that the alternate mod dir exists and that it is not disabled
            IEnumerable<string> dirs = Directory.EnumerateDirectories(Path.Combine(MEDirectories.GetBioGamePath(game), "DLC"))
                .Where(dir => dir.Contains($"DLC_MOD_FSvBS{(game.IsOTGame() ? "" : "LE")}_V", StringComparison.OrdinalIgnoreCase)
                              && !dir.Contains($"xDLC"));
            return dirs.Any();
        }

        /// <summary>
        /// Get the name of the mod dir, accounting for game version and mod version
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <returns>Mod dir name</returns>
        public static string GetModDirName(MEGame game) {
            return $"DLC_MOD_FSvBS{(game.IsOTGame() ? "" : "LE")}{(IsVvs(game) ? "_V" : "")}";
        }

        /// <summary>
        /// Get the name of the mod's patch dir, accounting for game version and mod version
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <returns>Mod dir name</returns>
        public static string GetPatchesModDirName(MEGame game)
        {
            return $"DLC_MOD_FSvBS{(game.IsOTGame() ? "" : "LE")}P{(IsVvs(game) ? "_V" : "")}";
        }

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
            else { return Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\{GetModDirName(game)}"); }
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
            else { return Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\{GetModDirName(game)}\CookedPCConsole"); }
        }

        /// <summary>
        /// Gets the path to the FSvBS's patches CookedPCConsole folder for LE3
        /// </summary>
        public static string LE3PatchesModPath => GetPatchesModPath(MEGame.LE3);
        /// <summary>
        /// Gets the path for the FSvBS's patches CookedPCConsole folder for the input game
        /// </summary>
        /// <param name="game">Game to get the path for</param>
        /// <returns>Path to the FSvBS's patches CookedPCConsole folder, null if the mod is not installed</returns>
        public static string GetPatchesModPath(MEGame game) {
            if (!IsPatchesModInstalled(game)) { return null; }
            else { return Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\{GetPatchesModDirName(game)}\CookedPCConsole"); }
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
            string file = Path.Combine(GetCookedPath(game), $@"Default_{GetModDirName(game)}.bin");
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
        /// Gets the path to the clean folder for the input game mod's patches
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetPatchesCleanPath(MEGame game) {
            string folder = Path.Combine(GetPatchesModPath(game), "Clean");
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
        /// Gets the paths of the FSvBS's patches mod files that contain instances of the clone for the input game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="game">Game to get the paths for</param>
        /// <returns>List of file paths containing instances of the clone, in the patches mod</returns>
        public static List<string> GetPatchesInstancePaths(MEGame game)
        {
            List<string> paths = [];

            if (game is not MEGame.LE3) { return paths; }

            foreach (string file in CLONE_INSTANCES_FILENAMES) {
                string path = Path.Combine(LE3PatchesModPath, file);

                if (!Path.Exists(path)) { continue; }

                paths.Add(path);
            }

            return paths;
        }

        /// <summary>
        /// Checks if the mod is installed for the input game
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <returns>True if the mod is installed</returns>
        public static bool IsModInstalled(MEGame game) {
            if (!Directory.Exists(Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\{GetModDirName(game)}"))) {
                return false;
            } else { return true; }
        }

        /// <summary>
        /// Checks if the patches mod is installed for the input game
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <returns>True if the patches mod is installed</returns>
        public static bool IsPatchesModInstalled(MEGame game) {
            if (!Directory.Exists(Path.Combine(MEDirectories.GetBioGamePath(game), $@"DLC\{GetPatchesModDirName(game)}"))) {
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
            if (!file.Names.Contains($"FSvBS_{(game.IsOTGame() ? "ME3" : "LE3")}_DummiesFile")) {
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

            // For patches
            foreach(string file in GetPatchesInstancePaths(game)) {
                File.Copy(Path.Combine(GetPatchesCleanPath(game), $"{Path.GetFileNameWithoutExtension(file)}_Clean.pcc"), file, true);
            }
        }

        /// <summary>
        /// Replace all files that can be cleaned with clean versions
        /// </summary>
        /// <param name="game">Game to replace for</param>
        /// <param name="cleanClone">True to clean the clone files</param>
        public static void ApplyCleanFiles(MEGame game, bool cleanClone = true) {
            ApplyCleanDummies(game);
            if (cleanClone) {
                ApplyCleanClone(game);
            }
        }

        /// <summary>
        /// TOCs the mod files
        /// </summary>
        /// <param name="game">Game to toc</param>
        public static void TOCMod(MEGame game) {
            MemoryStream toc = TOCCreator.CreateDLCTOCForDirectory(GetModPath(game), game);
            File.WriteAllBytes(Path.Combine(GetModPath(game), "PCConsoleTOC.bin"), toc.ToArray());
        }
    }
}
