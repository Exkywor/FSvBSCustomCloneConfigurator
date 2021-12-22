using FSvBSCustomCloneUtility.ViewModels;
using LegendaryExplorerCore;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Packages.CloningImportingAndRelinking;
using LegendaryExplorerCore.Unreal;
using LegendaryExplorerCore.Unreal.BinaryConverters;
using MassEffectModManagerCore.modmanager.save.game3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vector = MassEffectModManagerCore.modmanager.save.game3.Vector;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Writer to apply a headmorph to the dummies file
    /// </summary>
    public class MorphWriter {
        private string pccPath; // Found path, for ease of use
        private IMEPackage pcc; // Opened package
        private MEGame targetGame; // Target game
        private Gender gender; // Target gender
        private MorphHead morphSource; // Parsed headmorph
        private ExportEntry morphTarget; // Target morph export

        /// <summary> Global file name , Global file path </summary>
        private Dictionary<string, string> globalResources = new(StringComparer.OrdinalIgnoreCase);

        private List<string> resourcesNotFound = new();  // Resources not found.
        /// <summary> Resource's instanced name, paths to duplicates </summary>
        private Dictionary<string, IEnumerable<String>> resourceDuplicates = new(); // Resources in multiple files.

        /// <summary>
        /// Create an instance of MorphWriter
        /// </summary>
        /// <param name="ronFile">Path to headmorph</param>
        /// <param name="game">Target game</param>
        /// <param name="gender">Target Shepard</param>
        public MorphWriter(string ronFile, MEGame game, Gender gender) {
            Load(ronFile, game, gender);
        }

        /// <summary>
        /// Apply the headmorph 
        /// </summary>
        /// <returns>True if there were no errors</returns>
        public bool ApplyMorph() {
            EditMorphFeatures();
            EditBones();
            EditLODVertices();
            EditHair();
            EditMatOverrides();

            if (resourcesNotFound.Count > 0) { DisplayErrors("resourcesNotFound"); }
            else if (resourceDuplicates.Count > 0) { DisplayErrors("resourceDuplicates"); }
            else {
                MessageBox.Show($"The {(gender == Gender.Male ? "male" : "female")} headmorph was applied succesfully.",
                    "Success", MessageBoxButton.OK);
                pcc.Save();
            }

            pcc.Release();
            pcc.Dispose();
            return (resourcesNotFound.Count == 0 && resourceDuplicates.Count == 0);
        }

        /// <summary>
        /// Load the files and variables required by the methods
        /// </summary>
        /// <param name="ronFile">Path to headmorph</param>
        /// <param name="game">Target game</param>
        /// <param name="gender">Target Shepard</param>
        private void Load(string ronFile, MEGame game, Gender gender) {
            pccPath = FSvBSDirectories.GetDummiesPath(MEGame.ME3);
            pcc = MEPackageHandler.OpenMEPackage(pccPath);
            targetGame = game;
            this.gender = gender;

            morphSource = RONConverter.ConvertRON(ronFile);
            LoadMorphExport(gender);

            SetGlobalPaths();
        }

        /// <summary>
        /// Find and load the morph target in the dummies file.
        /// INVARIANT: The pcc does contain a dummy_custom export with an assigned BioMorphFace.
        /// </summary>
        /// <param name="gender">Target Shepard</param>
        private void LoadMorphExport(Gender gender) {
            IEnumerable<ExportEntry> stuntActors = pcc.Exports.Where(e => e.ClassName == "SFXStuntActor");
            foreach (ExportEntry stuntActor in stuntActors) {
                string targetTag = gender is Gender.Female ? "dummy_custom_female" : "dummy_custom_male";
                NameProperty tag = stuntActor.GetProperty<NameProperty>("Tag");

                if (tag != null && tag.Value == targetTag) {
                    ExportEntry archetype = (ExportEntry)stuntActor.Archetype;
                    morphTarget = (ExportEntry)pcc.GetEntry(archetype.GetProperty<ObjectProperty>("MorphHead").Value);
                }
            }
        }

        /// <summary>
        /// Set the list of global files
        /// </summary>
        private void SetGlobalPaths() {
            // Code thanks to Noira Fayn
            IEnumerable<string> globalFiles = Directory.EnumerateFiles
                (MEDirectories.GetCookedPath(targetGame), "*.pcc", SearchOption.AllDirectories)
                    .Where(file => Path.GetFileName(file).Contains("BIOG_HM"));
            
            foreach (string file in globalFiles) {
                globalResources.Add(Path.GetFileNameWithoutExtension(file), file);
            }
        }

        /// <summary>
        /// Get the export of the input name either from the current pcc, mod files, or global files
        /// </summary>
        /// <param name="name">
        /// Instantiated name of the resource
        /// Example: BIOG_HMF_HIR_PRO_HAIRMOD.Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff
        /// </param>
        /// <returns>The export of the resource in the dummies file; null if not found</returns>
        private IEntry GetResource(string name) {
            string fileName = name.Substring(0, name.IndexOf('.')); // BIOG_HMF_HIR_PRO_HAIRMOD
            string instancedName = name.Substring(name.IndexOf('.') + 1); // Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff

            // Check if resource is already in the file 
            IEntry res = pcc.FindExport(name);

            if (res != null) { return res; }

            ExportEntry extRes = null;

            // Get a list of mod files that contain the export
            // If export was found, also store it, but continue iterating to find if more than one file have it
            List<string> resourcePccs = GetModdedResourcePaths(fileName)
                .Where(file => {
                    using IMEPackage resourcePcc = MEPackageHandler.OpenMEPackage(file);
                    IEnumerable<ExportEntry> exports = resourcePcc.Exports.Where(e => e.InstancedFullPath == instancedName);
                    if (exports.Any()) { extRes = exports.First(); };
                    return exports.Any();
                }).ToList();

            // The resource was not in a mod, so we check in the global files if the fileName is that of a global file
            // We don't check for duplicates between mod files and global files, since mod files will override anyway
            if (!resourcePccs.Any() && globalResources.ContainsKey(fileName)) {
                using IMEPackage resourcePcc = MEPackageHandler.OpenMEPackage(globalResources[fileName]);
                IEnumerable<ExportEntry> exports = resourcePcc.Exports.Where(e => e.InstancedFullPath == instancedName);
                if (exports.Any()) {
                    extRes = exports.First();
                    resourcePccs.Add(globalResources[fileName]);
                }
            }

            if (!resourcePccs.Any()) { // Resource not found
                return null;
            } else if (resourcePccs.Count() > 1) { // Multiple files contain the resource
                // Store the instancedName and the name of the mods that contain it
                // Don't return null, since the resource was found
                if (!resourceDuplicates.ContainsKey(instancedName)) {
                    resourceDuplicates.Add(instancedName,
                        resourcePccs.Select(file => Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file)))));
                }
            }

            EntryExporter.ExportExportToPackage(extRes, pcc, out res);

            return res;
        }

        /// <summary>
        /// Get a list of modded files that match the input fileName name
        /// </summary>
        /// <param name="fileName">Filename to find</param>
        /// <returns>List of matching files</returns>
        private IEnumerable<string> GetModdedResourcePaths(string fileName) {
            return Directory.EnumerateFiles (MEDirectories.GetDLCPath(targetGame), "*.pcc", SearchOption.AllDirectories)
                .Where(file => Path.GetFileNameWithoutExtension(file).Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Apply the bones property from the headmorph to the morphTarget
        /// </summary>
        private void EditBones() {
            morphTarget.RemoveProperty("m_aFinalSkeleton");

            ArrayProperty<StructProperty> m_aFinalSkeleton = new("m_aFinalSkeleton");
            List<MorphHead.OffsetBone> offsetBones = morphSource.OffsetBones;

            foreach (MorphHead.OffsetBone bone in offsetBones) {
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

        /// <summary>
        /// Apply the morh features property from the headmorph to the morphTarget
        /// </summary>
        private void EditMorphFeatures() {
            morphTarget.RemoveProperty("m_aMorphFeatures");

            ArrayProperty<StructProperty> m_aMorphFeatures = new("m_aMorphFeatures");
            List<MorphHead.MorphFeature> sourceFeatures = morphSource.MorphFeatures;

            foreach (MorphHead.MorphFeature feature in sourceFeatures) {
                PropertyCollection props = new();
                props.Add(new NameProperty(feature.Name, "sFeatureName"));
                props.Add(new FloatProperty(feature.Offset, "Offset"));

                m_aMorphFeatures.Add(new StructProperty("MorphFeature", props));
            }

            morphTarget.WriteProperty(m_aMorphFeatures);
        }

        /// <summary>
        /// Apply the vertices binary data from the headmorph to the morphTarget
        /// </summary>
        private void EditLODVertices() {
            BioMorphFace head = ObjectBinary.From<BioMorphFace>(morphTarget);
            List<Vector>[] lods = { morphSource.Lod0Vertices, morphSource.Lod1Vertices, morphSource.Lod2Vertices, morphSource.Lod3Vertices };

            for (int lod = 0; lod < lods.Length; lod++) {
                for (int v = 0; v < lods[lod].Count; v++) {
                    head.LODs[lod][v].X = lods[lod][v].X;
                    head.LODs[lod][v].Y = lods[lod][v].Y;
                    head.LODs[lod][v].Z = lods[lod][v].Z;
                }
            }

            morphTarget.WriteBinary(head);
        }

        /// <summary>
        /// Apply the hair property from the headmorph to the morphTarget
        /// </summary>
        private void EditHair() {
            string hairName = morphSource.HairMesh.ToString();

            if (hairName is "None" or "") {
                // Remove the hair property in case it exists
                if (morphTarget.GetProperty<ObjectProperty>("m_oHairMesh") != null) {
                    morphTarget.RemoveProperty("m_oHairMesh");
                }
                return;
            }

            ExportEntry hairMesh = (ExportEntry) GetResource(morphSource.HairMesh);
            if (hairMesh == null) {
                resourcesNotFound.Add($" - HairMesh: {morphSource.HairMesh}");
                return;
            }

            ObjectProperty hairProp = morphTarget.GetProperty<ObjectProperty>("m_oHairMesh");
            if (hairProp != null) {
                hairProp.Value = hairMesh.UIndex;
            } else {
                hairProp = new ObjectProperty(hairMesh.UIndex, "m_oHairMesh");
            }
            morphTarget.WriteProperty(hairProp);
        }

        /// <summary>
        /// Apply the material overrides property from the headmorph to the morphTarget
        /// </summary>
        private void EditMatOverrides() {
            ExportEntry matOverride = (ExportEntry) pcc.GetEntry(morphTarget.GetProperty<ObjectProperty>("m_oMaterialOverrides").Value);

            matOverride.RemoveProperty("m_aScalarOverrides");
            matOverride.WriteProperty(GenerateScalarOverrides());
            matOverride.RemoveProperty("m_aColorOverrides");
            matOverride.WriteProperty(GenerateColorOverrides());
            matOverride.RemoveProperty("m_aTextureOverrides");
            matOverride.WriteProperty(GenerateTextureOverride());
        }

        /// <summary>
        /// Generate a scalar overrides property with the morphSource values
        /// </summary>
        /// <returns>A scalar overrides property</returns>
        private ArrayProperty<StructProperty> GenerateScalarOverrides() {
            ArrayProperty<StructProperty> m_aScalarOverrides = new("m_aScalarOverrides");
            foreach (MorphHead.ScalarParameter parameter in morphSource.ScalarParameters) {
                PropertyCollection props = new PropertyCollection();

                props.Add(new NameProperty(parameter.Name, "nName"));
                props.Add(new FloatProperty(parameter.Value, "sValue"));

                m_aScalarOverrides.Add(new StructProperty("ScalarParameter", props));
            }
            return m_aScalarOverrides;
        }

        /// <summary>
        /// Generate a color overrides property with the morphSource values
        /// </summary>
        /// <returns>A color overrides property</returns>
        private ArrayProperty<StructProperty> GenerateColorOverrides() {
            ArrayProperty<StructProperty> m_aColorOverrides = new("m_aColorOverrides");
            foreach (MorphHead.VectorParameter parameter in morphSource.VectorParameters) {
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

        /// <summary>
        /// Generate a texture overrides property with the morphSource values
        /// </summary>
        /// <returns>A texture overrides property</returns>
        private ArrayProperty<StructProperty> GenerateTextureOverride() {
            ArrayProperty<StructProperty> m_aTextureOverrides = new("m_aTextureOverrides");

            foreach (MorphHead.TextureParameter parameter in morphSource.TextureParameters) {
                string textureName = parameter.Value.Remove(parameter.Value.Length - 1).Substring(1);

                if (textureName is "None" or "") { continue; }

                PropertyCollection props = new PropertyCollection();
                IEntry texture = GetResource(textureName);

                if (texture == null) {
                    resourcesNotFound.Add($" - {parameter.Name}: {textureName}");
                    continue;
                }
                
                props.Add(new NameProperty(parameter.Name, "nName"));
                props.Add(new ObjectProperty(texture.UIndex, "m_pTexture"));


                m_aTextureOverrides.Add(new StructProperty("TextureParameter", props));
            }

            return m_aTextureOverrides;
        }

        /// <summary>
        /// Show a message box for the input error type
        /// </summary>
        /// <param name="type">The error type</param>
        private void DisplayErrors(string type) {
            switch(type) {
                case "resourcesNotFound":
                    string errMsg = string.Join(Environment.NewLine, resourcesNotFound.ToArray());
                    MessageBox.Show($"The following textures/hair could not be found for the {(gender == Gender.Male ? "male" : "female")} headmorph:" +
                        Environment.NewLine + Environment.NewLine +
                        $"{errMsg}"
                        + Environment.NewLine + Environment.NewLine +
                        $"Make sure that any modded texture/hairs are installed, and that the names are spelled correctly in the headmorph file." +
                        Environment.NewLine +
                        $"If you cannot install the modded resources, you can remove the lines from the headmorph file.",
                        "Error", MessageBoxButton.OK);
                    break;
                case "resourceDuplicates":
                    string dupMsg = "";
                    foreach (string key in resourceDuplicates.Keys) {
                        // Example:
                        //
                        // Hair_Pulled02.HMF_HIR_SCP_Pll02_Diff:
                        // - D:\Games\Origin\ME3\BioGame\CookedPCConsole\DLC\DLC_MOD_HAIR\CookedPCConsole\BioD_MOD_HAIR1.pcc
                        // - D:\Games\Origin\ME3\BioGame\CookedPCConsole\DLC\DLC_MOD_HAIRS\CookedPCConsole\BioD_MOD_HAIR2.pcc
                        dupMsg += $"{key}:" + Environment.NewLine + string.Join(Environment.NewLine, resourceDuplicates[key].ToArray()) + Environment.NewLine + Environment.NewLine;
                    }
                    MessageBox.Show($"The following textures/hair were found in more than one mod for the {(gender == Gender.Male ? "male" : "female")} headmorph:" +
                        Environment.NewLine + Environment.NewLine +
                        $"{dupMsg}" +
                        $"Make sure to only have one mod containing the resource." +
                        Environment.NewLine +
                        $"You can disable conflicting mods while using this tool and enable them afterwards.",
                        "Duplicate resources found", MessageBoxButton.OK);
                    break;
                default:
                    break;
            }
        }
    }
}
