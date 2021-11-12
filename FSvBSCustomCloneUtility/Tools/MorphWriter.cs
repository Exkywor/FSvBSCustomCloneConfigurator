using LegendaryExplorerCore;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Packages.CloningImportingAndRelinking;
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
        // Default path to clone vanilla meshes in case the user doesn't provide a hair
        // private string customHair = @"E:\Origin/Mass Effect 3/BIOGame/CookedPCConsole/BIOG_HMG_HIR_PRO.pcc";
        private string customHair = @"C:\Users\ferna\Desktop\hair.pcc";

        private string pccTargetFile;
        private IMEPackage pccTarget;
        private MorphHead morphSource;
        private ExportEntry morphTarget;
        private List<IMEPackage> resources;
        private IMEPackage hairPcc;

        // TODO: Validation of incoming stuff will be handled by Controller, not Model
        public MorphWriter(string ronFile, string targetFile)
        {
            Load(ronFile, targetFile);
        }

        public MorphWriter(string ronFile, string targetFile, List<String> resources)
        {

            Load(ronFile, targetFile, resources);
        }

        private void LoadCommands()
        {

        }

        public void ApplyMorph()
        {
            EditBones();
            EditLODVertices();
            EditHair();

            pccTarget.Save();
        }

        private void Load(string ronFile, string targetFile, List<String>? resourceFiles = null)
        {
            // TODO: Add check for ME3 or LE3 file
            pccTargetFile = targetFile;
            pccTarget = MEPackageHandler.OpenMEPackage(targetFile);

            // need to run some validation checks on the file first
            morphSource = RONConverter.ConvertRON(ronFile);
            LoadMorphExport();

            // Load pccs that contain the hairs and complexions to use
            if (resources != null && resources.Count > 0)
            {
                foreach (String file in resourceFiles)
                {
                    resources.Add(MEPackageHandler.OpenME3Package(file));
                }
            }

            // Deprecated: Will abstract to a load resources method
            // TODO: Check if is ME3 or LE3
            hairPcc = MEPackageHandler.OpenME3Package(this.customHair);

            return;
        }
        private IEntry LoadMorphExport()
        {
            // Invariant: The pcc does contain a dummy_custom export with an assigned BioMorphFace.
            var stuntActors = pccTarget.Exports.Where(e => e.ClassName == "SFXStuntActor");
            foreach (var stuntActor in stuntActors)
            {
                var tag = stuntActor.GetProperty<NameProperty>("Tag");
                if (tag != null && tag.Value == "dummy_custom")
                {
                    ExportEntry archetype = (ExportEntry)stuntActor.Archetype;
                    morphTarget = (ExportEntry)pccTarget.GetEntry(archetype.GetProperty<ObjectProperty>("MorphHead").Value);
                }
            }
            return null;
        }

        private IEntry GetResource(String name)
        {
            // Need to handle if there are no resources!
            // Remember that the resources will point to the ME3 install folder, so I can figure it out from there
            string hairName = name.Substring(name.IndexOf('.') + 1);
            var sourceHair = hairPcc.FindEntry(hairName);
            var sourcePackage = hairPcc.GetEntry(sourceHair.idxLink);

            EntryImporter.ImportAndRelinkEntries(EntryImporter.PortingOption.CloneAllDependencies, sourcePackage, pccTarget, null, true, out var result);
            var clonedPackage = (ExportEntry) result;

            return clonedPackage;
        }

        // Will need to add a parameter
        private IEntry FindResource()
        {
            return null;
        }


        private void EditBones()
        {
            ArrayProperty<StructProperty> m_aFinalSkeleton = morphTarget.GetProperty<ArrayProperty<StructProperty>>("m_aFinalSkeleton");
            var offsetBones = morphSource.OffsetBones;

            foreach (var item in m_aFinalSkeleton)
            {
                MorphHead.OffsetBone offsetBone = offsetBones.Find(x => x.Name.ToString() == item.Properties.GetProp<NameProperty>("nName").Value);
                StructProperty vPos = item.Properties.GetProp<StructProperty>("vPos");
                vPos.GetProp<FloatProperty>("X").Value = offsetBone.Offset.X;
                vPos.GetProp<FloatProperty>("Y").Value = offsetBone.Offset.Y;
                vPos.GetProp<FloatProperty>("Z").Value = offsetBone.Offset.Z;

                morphTarget.WriteProperty(m_aFinalSkeleton);
            }
        }

        private void EditLODVertices()
        {
            BioMorphFace head = ObjectBinary.From<BioMorphFace>(morphTarget);
            List<Vector>[] lods = { morphSource.Lod0Vertices, morphSource.Lod1Vertices, morphSource.Lod2Vertices, morphSource.Lod3Vertices };

            for (int lod = 0; lod < lods.Length; lod++)
            {
                for (int v = 0; v < lods[lod].Count; v++)
                {
                    head.LODs[lod][v].X = lods[lod][v].X;
                    head.LODs[lod][v].Y = lods[lod][v].Y;
                    head.LODs[lod][v].Z = lods[lod][v].Z;
                }
            }

            morphTarget.WriteBinary(head);
        }

        private void EditHair()
        {
            ExportEntry hairMesh = (ExportEntry) GetResource(morphSource.HairMesh);

            ObjectProperty hairProp = morphTarget.GetProperty<ObjectProperty>("m_oHairMesh");
            hairProp.Value = hairMesh.UIndex;
            morphTarget.WriteProperty(hairProp);
        }
    }
}
