using LegendaryExplorerCore;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Unreal;
using LegendaryExplorerCore.Unreal.BinaryConverters;
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
        // TODO: This will change for other files!
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
            EditLODVertices();
            // Only call for the ones that have LODs
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
                MorphHead.OffsetBone offsetBone = offsetBones.Find(x => x.Name.ToString() == item.Properties.GetProp<NameProperty>("nName").Value);
                StructProperty vPos = item.Properties.GetProp<StructProperty>("vPos");
                vPos.GetProp<FloatProperty>("X").Value = offsetBone.Offset.X;
                vPos.GetProp<FloatProperty>("Y").Value = offsetBone.Offset.Y;
                vPos.GetProp<FloatProperty>("Z").Value = offsetBone.Offset.Z;

                morphExport.WriteProperty(m_aFinalSkeleton);
            }

            return true;
        }

        private bool EditLODVertices()
        {
            BioMorphFace head = ObjectBinary.From<BioMorphFace>(morphExport);
            List<Vector>[] lods = { morphHead.Lod0Vertices, morphHead.Lod1Vertices, morphHead.Lod2Vertices, morphHead.Lod3Vertices };

            for (int lod = 0; lod < lods.Length; lod++)
            {
                for (int v = 0; v < lods[lod].Count; v++)
                {
                    head.LODs[lod][v].X = lods[lod][v].X;
                    head.LODs[lod][v].Y = lods[lod][v].Y;
                    head.LODs[lod][v].Z = lods[lod][v].Z;
                }
            }

            morphExport.WriteBinary(head);
            return true;
        }
    }
}
