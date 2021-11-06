using LegendaryExplorerCore;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Unreal;
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
        const int TargetExportIndex = 2926;
        
        private string ronFile;
        private string targetFile;
        private MorphHead morphHead;
        private IMEPackage pcc;
        private ExportEntry morphExport;

        public MorphWriter()
        {

        }

        private void LoadCommands()
        {

        }

        public void ApplyMorph(string ronFile, string targetFile)
        {
            LoadResources(ronFile, targetFile);
            EditBones();
            pcc.Save();
        }

        private bool LoadResources(string ronFile, string targetFile)
        {
            // need to run some validation checks on the file first
            morphHead = RONConverter.ConvertRON(ronFile);

            // need to run some validation checks on the file first
            this.targetFile = targetFile;

            // TODO: Add check for ME3 or LE3 file
            pcc = MEPackageHandler.OpenMEPackage(targetFile);

            morphExport = (ExportEntry) pcc.GetEntry(TargetExportIndex);

            return true;
        }

        private bool EditBones()
        {
            ArrayProperty<StructProperty> m_aFinalSkeleton = morphExport.GetProperty<ArrayProperty<StructProperty>>("m_aFinalSkeleton");
            var offsetBones = morphHead.OffsetBones;

            foreach (var item in m_aFinalSkeleton)
            {
                var offsetBone = offsetBones.Find(x => x.Name.ToString() == item.Properties.GetProp<NameProperty>("nName").Value);
                var vPos = item.Properties.GetProp<StructProperty>("vPos");
                vPos.GetProp<FloatProperty>("X").Value = offsetBone.Offset.X;
                vPos.GetProp<FloatProperty>("Y").Value = offsetBone.Offset.Y;
                vPos.GetProp<FloatProperty>("Z").Value = offsetBone.Offset.Z;

                morphExport.WriteProperty(m_aFinalSkeleton);
            }

            return true;
        }
    }
}
