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
        private string pccTargetFile;
        private IMEPackage pccTarget;
        private MorphHead morphSource;
        private ExportEntry morphTarget;
        private Dictionary<String, IMEPackage> resources = new Dictionary<String, IMEPackage>();
        private Dictionary<String, String> vanillaResources = new Dictionary<String, String>(); // Does not contain open pccs to save memory if not used
        private String[] vanillaNames = {"BIOG_HMM_HED_Alignment", "BIOG_HMF_HED_Alignment",
            "BIOG_HMM_HED_PROMorph_R", "BIOG_HMF_HED_PROMorph_R", "BIOG_HMM_HIR_PRO", "BIOG_HMF_HIR_PRO"};

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
            if (resourceFiles != null && resourceFiles.Count > 0)
            {
                foreach (String file in resourceFiles)
                {
                    String resourceName = file.Substring(file.LastIndexOf(@"\")+1);
                    resourceName = resourceName.Remove(resourceName.Length - 4);
                    resources.Add(resourceName, MEPackageHandler.OpenME3Package(file));
                }
            }

            // Generate list of vanilla files that contains resources
            SetVanillaPaths();

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

        private void SetVanillaPaths()
        {
            String prefix = @$"{pccTargetFile.Substring(0, pccTargetFile.IndexOf("BIOGame") + 7)}\CookedPCConsole";
            // String prefix = pccTargetFile.Substring(0, pccTargetFile.LastIndexOf(@"\")+1);
            foreach (String name in vanillaNames)
            {
                vanillaResources.Add(name, $@"{prefix}\{name}.pcc");
            }
        }

        // Name Example: BIOG_HMF_HIR_PRO_HAIRMOD.Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff
        private IEntry GetResource(String name)
        {
            string fileName = name.Substring(0, name.IndexOf('.')); // BIOG_HMF_HIR_PRO_HAIRMOD
            string resourceName = name.Substring(name.IndexOf('.') + 1); // Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff
            string packageName = resourceName.Substring(0, resourceName.IndexOf('.')); // Hair_Pulled02

            if (vanillaResources.ContainsKey(fileName)) {
                // If resource is vanilla, try to find it in the current file
                IEntry res = pccTarget.FindEntry(resourceName);

                if (res != null)
                {
                    return res;
                }

                // If resource not in current file, search in vanilla files
                // TODO: Check if ME3 or LE3
                IMEPackage vanillaPcc = MEPackageHandler.OpenME3Package(vanillaResources[fileName]);

                // First we check that the resource we want exists
                IEntry extRes = vanillaPcc.FindEntry(resourceName);
                if (extRes == null) // Resource not found
                {
                    return null;
                }

                // Next we get the IEntry of the package itself, so we clone the full path, rather than just the resource
                IEntry extPackage = vanillaPcc.FindEntry(packageName);
                EntryImporter.ImportAndRelinkEntries(EntryImporter.PortingOption.CloneAllDependencies, extPackage, pccTarget, null, true, out _);

                // Now that the package and resource are here, we find the ExportEntry of the resource, NOT the package, and return that
                return pccTarget.FindEntry(resourceName);
            } else if (resources.ContainsKey(fileName))
            {
                IEntry extRes = resources[fileName].FindEntry(resourceName);
                if (extRes == null) // Resource not found
                {
                    return null;
                }

                IEntry extPackage = resources[fileName].FindEntry(packageName);
                EntryImporter.ImportAndRelinkEntries(EntryImporter.PortingOption.CloneAllDependencies, extPackage, pccTarget, null, true, out _);

                return pccTarget.FindEntry(resourceName);
            } else
            {
                return null;
            }
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
            if (hairMesh == null)
            {
                // need to throw an error!
                return;
            }

            ObjectProperty hairProp = morphTarget.GetProperty<ObjectProperty>("m_oHairMesh");
            hairProp.Value = hairMesh.UIndex;
            morphTarget.WriteProperty(hairProp);
        }
    }
}
