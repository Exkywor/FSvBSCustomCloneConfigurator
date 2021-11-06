using LegendaryExplorerCore;
using LegendaryExplorerCore.Packages;
using MassEffectModManagerCore.modmanager.save.game3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools
{
    public class MorphWriter
    {
        private string ronFile;
        private string targetFile;
        private MorphHead morphHead;

        public MorphWriter()
        {
        }

        private void LoadCommands()
        {

        }

        public void ApplyMorph(string ronFile, string targetFile)
        {
            LoadPCC(targetFile);
            LoadMorph(ronFile);
        }

        private void LoadPCC(string targetFile)
        {
            // need to run some validation checks on the file first
            this.targetFile = targetFile;
            // TODO: Add check for ME3 or LE3 file
            using IMEPackage pcc = MEPackageHandler.OpenMEPackage(targetFile);
        }

        private void LoadMorph(string ronFile)
        {
            // need to run some validation checks on the file first
            morphHead = RONConverter.ConvertRON(ronFile);
        }
    }
}
