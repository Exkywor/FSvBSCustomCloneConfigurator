using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    public static class Validators {
        public static bool ValidateFSvBSFile(string fsvbsFile, MEGame game) {
            using (IMEPackage file = MEPackageHandler.OpenMEPackage(fsvbsFile)) {

                // FIle does not contain the identifier, meaning it's from an older version of the mod
                if (!file.Names.Contains($"FSvBS_{(game == MEGame.ME3 ? "ME3" : "LE3")}_DummiesFile")) {
                    return false;
                } 
            }
            return true;
        }
    }
}
