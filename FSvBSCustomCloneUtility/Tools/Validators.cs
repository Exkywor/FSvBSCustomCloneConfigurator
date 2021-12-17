using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    public static class Validators {
        public static bool ValidateFSvBSFile(string fsvbsFile, string game) {
            // Check that the file has the correct name
            if (!fsvbsFile.Contains("BioD_FSvBS_Dummies.pcc")) { return false; }

            IMEPackage file = MEPackageHandler.OpenMEPackage(fsvbsFile);

            if (!file.Game.IsGame3()) { return false; }

            if (game == "ME3") {
                if (file.Game is not MEGame.ME3) { return false; }

                if (!file.Names.Contains("FSvBS_ME3_DummiesFile")) { return false; }

            } else {
                if (file.Game is not MEGame.LE3) { return false; }

                if (!file.Names.Contains("FSvBS_LE3_DummiesFile")) { return false; }
            }

            return true;
        }
    }
}
