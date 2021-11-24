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
using System.Windows;
using Vector = MassEffectModManagerCore.modmanager.save.game3.Vector;

namespace FSvBSCustomCloneUtility.Tools
{
    public class MorphWriter
    {
        private string pccTargetFile;
        private IMEPackage pccTarget;
        private MorphHead morphSource;
        private ExportEntry morphTarget;
        // resources and globalResources only contain paths, to avoid opening unnecessary pccs
        private Dictionary<string, string> resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> globalResources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string[] globalNames = {"BIOG_HMM_HED_Alignment", "BIOG_HMF_HED_Alignment",
            "BIOG_HMM_HED_PROMorph", "BIOG_HMF_HED_PROMorph_R", "BIOG_HMM_HIR_PRO_R", "BIOG_HMF_HIR_PRO"};

        // TODO: Validation of incoming stuff will be handled by Controller, not Model
        public MorphWriter(string ronFile, string targetFile, Gender gender)
        {
            Load(ronFile, targetFile, gender);
        }

        public MorphWriter(string ronFile, string targetFile, Gender gender, List<string> resources)
        {

            Load(ronFile, targetFile, gender, resources);
        }

        private void LoadCommands()
        {

        }

        public void ApplyMorph()
        {
            try
            {
                EditMorphFeatures();
                EditBones();
                EditLODVertices();
                EditHair();
                EditMatOverrides();

                pccTarget.Save();
                return;
            } catch (ArgumentNullException e)
            {
                MessageBox.Show($"{e.Message}." +
                    $"Check that the texture/hair is spelled correctly and that you have provided a valid resource pcc if it's not in the basegame." +
                    $"If you don't have access to the modded resource, you can remove the entry from the .ron",
                    "Error", MessageBoxButton.OK);
                return;
            }

        }

        private void Load(string ronFile, string targetFile, Gender gender, List<string>? resourcePaths = null)
        {
            pccTargetFile = targetFile;
            pccTarget = MEPackageHandler.OpenMEPackage(targetFile);

            morphSource = RONConverter.ConvertRON(ronFile);
            LoadMorphExport(gender);

            if (resourcePaths != null && resourcePaths.Count > 0)
            {
                SetResourcePaths(resourcePaths);
            }
            SetGlobalPaths();

            return;
        }
        private IEntry LoadMorphExport(Gender gender)
        {
            // INVARIANT: The pcc does contain a dummy_custom export with an assigned BioMorphFace.
            IEnumerable<ExportEntry> stuntActors = pccTarget.Exports.Where(e => e.ClassName == "SFXStuntActor");
            foreach (ExportEntry stuntActor in stuntActors)
            {
                string targetTag = gender is Gender.Female ? "dummy_custom_female" : "dummy_custom_male";
                NameProperty tag = stuntActor.GetProperty<NameProperty>("Tag");

                if (tag != null && tag.Value == targetTag)
                {
                    ExportEntry archetype = (ExportEntry)stuntActor.Archetype;
                    morphTarget = (ExportEntry)pccTarget.GetEntry(archetype.GetProperty<ObjectProperty>("MorphHead").Value);
                }
            }
            return null;
        }

        private void SetResourcePaths(List<string> resourcePaths)
        {
            foreach (string file in resourcePaths)
            {
                string resourceName = file.Substring(file.LastIndexOf(@"\") + 1); // Get only the file name
                resourceName = resourceName.Remove(resourceName.Length - 4); // Remove the .pcc part
                resources.Add(resourceName, file);
            }
        }

        private void SetGlobalPaths()
        {
            string prefix = @$"{pccTargetFile.Substring(0, pccTargetFile.IndexOf("BIOGame", StringComparison.OrdinalIgnoreCase) + 7)}\CookedPCConsole";
            foreach (string name in globalNames)
            {
                globalResources.Add(name, $@"{prefix}\{name}.pcc");
            }
        }

        // Name Example: BIOG_HMF_HIR_PRO_HAIRMOD.Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff
        private IEntry GetResource(string name)
        {
            string fileName = name.Substring(0, name.IndexOf('.'));

            if (globalResources.ContainsKey(fileName)) {
                return GetOrCloneResource(name, globalResources);
            } else if (resources.ContainsKey(fileName))
            {
                return GetOrCloneResource(name, resources);
            } else
            {
                return null;
            }
        }

        private IEntry GetOrCloneResource(string name, Dictionary<string, string> resourcePaths)
        {
            string fileName = name.Substring(0, name.IndexOf('.')); // BIOG_HMF_HIR_PRO_HAIRMOD
            string instancedName = name.Substring(name.IndexOf('.') + 1); // Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff

            // Check if resource is alrady in the file 
            IEntry res = pccTarget.FindExport(name);

            if (res != null)
            {
                return res;

            }

            using IMEPackage resourcePcc = MEPackageHandler.OpenMEPackage(resourcePaths[fileName]);

            ExportEntry extRes = resourcePcc.FindExport(instancedName);
            if (extRes == null) // Resource not found
            {
                return null;
            }

            EntryExporter.ExportExportToPackage(extRes, pccTarget, out res);

            return res;
        }

        private void EditBones()
        {
            morphTarget.RemoveProperty("m_aFinalSkeleton");

            ArrayProperty<StructProperty> m_aFinalSkeleton = new("m_aFinalSkeleton");
            List<MorphHead.OffsetBone> offsetBones = morphSource.OffsetBones;
            foreach (MorphHead.OffsetBone bone in offsetBones)
            {
                PropertyCollection props = new();
                
                PropertyCollection vals = new();
                vals.Add(new FloatProperty(bone.Offset.X, "X"));
                vals.Add(new FloatProperty(bone.Offset.Y, "Y"));
                vals.Add(new FloatProperty(bone.Offset.Z, "Z"));

                props.Add(new StructProperty("Vector", vals, "vPos", true));
                props.Add(new NameProperty(bone.Name, "nName"));

                m_aFinalSkeleton.Add(new StructProperty("OffsetBonePos", props));
            }

            morphTarget.WriteProperty(m_aFinalSkeleton);
        }

        private void EditMorphFeatures()
        {
            morphTarget.RemoveProperty("m_aMorphFeatures");

            ArrayProperty<StructProperty> m_aMorphFeatures = new("m_aMorphFeatures");
            List<MorphHead.MorphFeature> sourceFeatures = morphSource.MorphFeatures;
            foreach (MorphHead.MorphFeature feature in sourceFeatures)
            {
                PropertyCollection props = new();
                props.Add(new NameProperty(feature.Name, "sFeatureName"));
                props.Add(new FloatProperty(feature.Offset, "Offset"));

                m_aMorphFeatures.Add(new StructProperty("MorphFeature", props));
            }

            morphTarget.WriteProperty(m_aMorphFeatures);
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
            string hairName = morphSource.HairMesh.ToString();

            if (hairName is "None" or "")
            {
                // Remove the hair property in case it exists
                if (morphTarget.GetProperty<ObjectProperty>("m_oHairMesh") != null)
                {
                    morphTarget.RemoveProperty("m_oHairMesh");
                }
                return;
            }

            ExportEntry hairMesh = (ExportEntry)GetResource(morphSource.HairMesh);
            if (hairMesh == null)
            {
                throw new ArgumentNullException("HairMesh", $"Could not find {hairName}.");
            }

            ObjectProperty hairProp = morphTarget.GetProperty<ObjectProperty>("m_oHairMesh");
            if (hairProp != null)
            {
                hairProp.Value = hairMesh.UIndex;
            } else
            {
                hairProp = new ObjectProperty(hairMesh.UIndex, "m_oHairMesh");
            }
            morphTarget.WriteProperty(hairProp);
        }

        private void EditMatOverrides()
        {
            ExportEntry matOverride = (ExportEntry)pccTarget.GetEntry(morphTarget.GetProperty<ObjectProperty>("m_oMaterialOverrides").Value);

            matOverride.RemoveProperty("m_aScalarOverrides");
            matOverride.WriteProperty(GenerateScalarOverrides());
            matOverride.RemoveProperty("m_aColorOverrides");
            matOverride.WriteProperty(GenerateColorOverrides());
            matOverride.RemoveProperty("m_aTextureOverrides");
            matOverride.WriteProperty(GenerateTextureOverride());
        }

        private ArrayProperty<StructProperty> GenerateScalarOverrides()
        {
            var m_aScalarOverrides = new ArrayProperty<StructProperty>("m_aScalarOverrides");
            foreach (MorphHead.ScalarParameter parameter in morphSource.ScalarParameters)
            {
                PropertyCollection props = new PropertyCollection();

                props.Add(new NameProperty(parameter.Name, "nName"));
                props.Add(new FloatProperty(parameter.Value, "sValue"));

                m_aScalarOverrides.Add(new StructProperty("ScalarParameter", props));
            }
            return m_aScalarOverrides;
        }

        private ArrayProperty<StructProperty> GenerateColorOverrides()
        {
            var m_aColorOverrides = new ArrayProperty<StructProperty>("m_aColorOverrides");
            foreach (MorphHead.VectorParameter parameter in morphSource.VectorParameters)
            {
                PropertyCollection props = new();

                PropertyCollection color = new();
                color.Add(new FloatProperty(parameter.Value.R, "R"));
                color.Add(new FloatProperty(parameter.Value.G, "G"));
                color.Add(new FloatProperty(parameter.Value.B, "B"));
                color.Add(new FloatProperty(parameter.Value.A, "A"));

                StructProperty cValue = new("LinearColor", color, "cValue", true);

                props.Add(cValue);
                props.Add(new NameProperty(parameter.Name, "nName"));

                m_aColorOverrides.Add(new StructProperty("ColorParameter", props));
            }

            return m_aColorOverrides;
        }

        private ArrayProperty<StructProperty> GenerateTextureOverride()
        {
            var m_aTextureOverrides = new ArrayProperty<StructProperty>("m_aTextureOverrides");
            foreach (MorphHead.TextureParameter parameter in morphSource.TextureParameters)
            {
                string textureName = parameter.Value.Remove(parameter.Value.Length - 1).Substring(1);

                if (textureName is "None" or "")
                {
                    continue;
                }

                PropertyCollection props = new PropertyCollection();
                IEntry texture = GetResource(textureName);

                if (texture == null)
                {
                    throw new ArgumentNullException(parameter.Name, $"Could not find texture {textureName}.");
                }
                
                props.Add(new NameProperty(parameter.Name, "nName"));
                props.Add(new ObjectProperty(texture.UIndex, "m_pTexture"));


                m_aTextureOverrides.Add(new StructProperty("TextureParameter", props));
            }
            return m_aTextureOverrides;
        }
    }
}
