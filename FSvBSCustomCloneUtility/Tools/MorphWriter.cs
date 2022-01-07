using Caliburn.Micro;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.Tools;
using FSvBSCustomCloneUtility.ViewModels;
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
using System.Windows;
using Vector = MassEffectModManagerCore.modmanager.save.game3.Vector;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Writer to apply a headmorph to the dummies file
    /// </summary>
    public class MorphWriter {
        private IWindowManager windowManager = new WindowManager();

        private readonly bool applyToActor; // Whether to apply the morph as a morph or the actor directly
        private string pccPath; // Found path, for ease of use
        private IMEPackage pcc; // Opened package
        private MEGame targetGame; // Target game
        private Gender gender; // Target gender
        private MorphHead morphSource; // Parsed headmorph
        private ExportEntry morphTarget; // Target morph export
        private ExportEntry archetype; // Target archetype for hair and materials

        /// <summary> Global file name , Global file path </summary>
        private Dictionary<string, string> globalResources = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Resource's parameter, path resource</summary>
        private Dictionary<string, string> resourcesNotFound = new(); // Resources not found
        /// <summary> Resource's instanced name, paths to duplicates </summary>
        private Dictionary<string, IEnumerable<string>> resourceDuplicates = new(); // Resources in multiple files.

        /// <summary>
        /// Create an instance of MorphWriter
        /// </summary>
        /// <param name="ronFile">Path to headmorph</param>
        /// <param name="game">Target game</param>
        /// <param name="gender">Target Shepard</param>
        public MorphWriter(string ronFile, MEGame game, Gender gender, bool applyToActor = true) {
            this.applyToActor = applyToActor;
            Load(ronFile, game, gender);
        }

        /// <summary>
        /// Apply the headmorph 
        /// </summary>
        /// <returns>A tuple containing resourcesNotFound and resourceDuplicates</returns>
        public (Dictionary<string, string>, Dictionary<string, IEnumerable<string>>) ApplyMorph() {
            EditHead();
            EditHair();

            if (!(resourcesNotFound.Count > 0 || resourceDuplicates.Count > 0)) {
                pcc.Save();
            }

            pcc.Release();
            pcc.Dispose();

            return (resourcesNotFound, resourceDuplicates);
        }

        /// <summary>
        /// Load the files and variables required by the methods
        /// </summary>
        /// <param name="ronFile">Path to headmorph</param>
        /// <param name="game">Target game</param>
        /// <param name="gender">Target Shepard</param>
        private void Load(string ronFile, MEGame game, Gender gender) {
            pccPath = FSvBSDirectories.GetDummiesPath(game);
            pcc = MEPackageHandler.OpenMEPackage(pccPath);
            targetGame = game;
            this.gender = gender;

            morphSource = RONConverter.ConvertRON(ronFile);
            HandleSpecialMorphTextureCases();
            LoadMorphAndArchetypeExports(gender);

            SetGlobalPaths();
        }

        /// <summary>
        /// Find and load the morph target in the dummies file.
        /// INVARIANT: The pcc does contain a dummy_custom export with an assigned BioMorphFace.
        /// </summary>
        /// <param name="gender">Target Shepard</param>
        private void LoadMorphAndArchetypeExports(Gender gender) {
            archetype = pcc.FindExport($"BioChar_CustomDummy.Archetypes.fsvbs_dummy_custom_{(gender.IsMale() ? "male" : "female")}_Con");
            if (archetype != null) {
                morphTarget = (ExportEntry)pcc.GetEntry(archetype.GetProperty<ObjectProperty>("MorphHead").Value);
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
                    ExportEntry export = resourcePcc.FindExport(instancedName);
                    if (export != null) { extRes = export; }
                    return export != null;
                }).ToList();

            // The resource was not in a mod, so we check in the global files if the fileName is that of a global file
            // We don't check for duplicates between mod files and global files, since mod files will override anyway
            if (!resourcePccs.Any() && globalResources.ContainsKey(fileName)) {
                using IMEPackage resourcePcc = MEPackageHandler.OpenMEPackage(globalResources[fileName]);
                ExportEntry export = resourcePcc.FindExport(instancedName);
                if (export != null) {
                    extRes = export;
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
        /// Apply all head propertys from the headmorph to the morphTarget
        /// </summary>
        private void EditHead() {
            EditBones();
            EditMorphFeatures();
            EditLODVertices();

            if (applyToActor) {
                // Remove material overrides since we'll apply those to the archetype
                if (morphTarget.GetProperty<ObjectProperty>("m_oMaterialOverrides") != null) {
                    morphTarget.RemoveProperty("m_oMaterialOverrides");
                }

                ExportEntry headSMC = SMCTools.GetHeadSMC(archetype, pcc);
                ApplyOverridesToSMC(headSMC);
            } else {
                ApplyOverridesToMorph();
            }
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
        /// Apply the morph features property from the headmorph to the morphTarget
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
        /// Apply the hair property from the headmorph to the archetype or morphTarget
        /// </summary>
        private void EditHair() {
            string hairName = morphSource.HairMesh.ToString();
            ExportEntry hairSMC = SMCTools.GetHairSMC(archetype, pcc);

            if (applyToActor) {
                // Remove the hair property in case it exists, since it'll be set to the hairSMC directly
                if (morphTarget.GetProperty<ObjectProperty>("m_oHairMesh") != null) {
                    morphTarget.RemoveProperty("m_oHairMesh");
                }
            }

            // Stop if no hair is set in the headmorph, and remove the skeletal mesh from the smc in case it contains one
            if (hairName.Equals("None", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(hairName.Trim())) {
                if (applyToActor) {
                    if (hairSMC.GetProperty<ObjectProperty>("SkeletalMesh") != null) {
                        hairSMC.RemoveProperty("SkeletalMesh");
                    }
                } else {
                    // Remove the hair property in case it exists
                    if (morphTarget.GetProperty<ObjectProperty>("m_oHairMesh") != null) {
                        morphTarget.RemoveProperty("m_oHairMesh");
                    }
                    return;
                }
                return;
            }

            // Try to get the resource. Add it to not found if null
            ExportEntry hairMesh = (ExportEntry) GetResource(morphSource.HairMesh);
            if (hairMesh == null) {
                if (!resourcesNotFound.ContainsKey("HairMesh")) {
                    resourcesNotFound.Add("HairMesh", $"HairMesh: {morphSource.HairMesh}");
                }
                return;
            }

            if (applyToActor) {
                ObjectProperty hairSMCMesh = hairSMC.GetProperty<ObjectProperty>("SkeletalMesh");
                if (hairSMCMesh == null) {
                    hairSMCMesh = new ObjectProperty(hairMesh.UIndex, "SkeletalMesh");
                } else {
                    hairSMCMesh.Value = hairMesh.UIndex;
                }

                hairSMC.WriteProperty(hairSMCMesh);
                ApplyOverridesToSMC(hairSMC, true, pcc.GetUExport(hairMesh.UIndex));
            } else {
                ObjectProperty hairProp = morphTarget.GetProperty<ObjectProperty>("m_oHairMesh");
                if (hairProp != null) {
                    hairProp.Value = hairMesh.UIndex;
                } else {
                    hairProp = new ObjectProperty(hairMesh.UIndex, "m_oHairMesh");
                }
                morphTarget.WriteProperty(hairProp);
            }
        }

        /// <summary>
        /// Apply the material overrides from the ron file to the materials of the input SkeletalMeshComponent
        /// </summary>
        /// <param name="SMC">SkeletalMeshComponent containing the materials</param>
        /// <param name="isHair">Is hair material</param>
        /// <param name="sourceHairMesh">Optional hair mesh to get material parent from</param>
        private void ApplyOverridesToSMC(ExportEntry SMC, bool isHair = false, ExportEntry sourceHairMesh = null) {
            ArrayProperty<ObjectProperty> materials = SMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
            if (materials == null) { return; }

            foreach (ObjectProperty material in materials) {
                if (isHair) {
                    ApplyOverridesToMatInstance(pcc.GetUExport(material.Value), true, sourceHairMesh);
                } else {
                    ApplyOverridesToMatInstance(pcc.GetUExport(material.Value));
                }
            }

            SMC.WriteProperty(materials);
        }

        /// <summary>
        /// Apply the material overrides from the ron file to the input material instance 
        /// </summary>
        /// <param name="matInstance">Material instance to apply parameters to</param>
        /// <param name="isHair">Is hair material</param>
        /// <param name="sourceHairMesh">Optional hair mesh to get material parent from</param>
        private void ApplyOverridesToMatInstance(ExportEntry matInstance, bool isHair = false, ExportEntry sourceHairMesh = null) {
            matInstance.RemoveProperty("VectorParameterValues");
            matInstance.RemoveProperty("ScalarParameterValues");
            matInstance.RemoveProperty("TextureParameterValues");

            matInstance.WriteProperty(GenerateVectorValues());
            matInstance.WriteProperty(GenerateScalarValues());
            matInstance.WriteProperty(GenerateTextureValues());

            // Change the material parent of the matInstance
            if (isHair) {
                if (sourceHairMesh == null) { return; }
                ObjectProperty parentProp = matInstance.GetProperty<ObjectProperty>("Parent");
                SkeletalMesh sourceHairBin = ObjectBinary.From<SkeletalMesh>(sourceHairMesh);
                int parentMatID = sourceHairBin.Materials[0];
                parentProp.Value = parentMatID;
                matInstance.WriteProperty(parentProp);
            }
        }

        /// <summary>
        /// Apply the material overrides form the ron file to the morphTarget
        /// </summary>
        private void ApplyOverridesToMorph() {
            ExportEntry matOverride = (ExportEntry)pcc.GetEntry(morphTarget.GetProperty<ObjectProperty>("m_oMaterialOverrides").Value);

            matOverride.RemoveProperty("m_aScalarOverrides");
            matOverride.WriteProperty(GenerateScalarValues());
            matOverride.RemoveProperty("m_aColorOverrides");
            matOverride.WriteProperty(GenerateVectorValues());
            matOverride.RemoveProperty("m_aTextureOverrides");
            matOverride.WriteProperty(GenerateTextureValues());
        }

        /// <summary>
        /// Generate a Scalar values property with the morphSource values
        /// </summary>
        /// <returns>A Scalar values property</returns>
        private ArrayProperty<StructProperty> GenerateScalarValues() {
            ArrayProperty<StructProperty> ScalarValues = new($"{(applyToActor ? "ScalarParameterValues" : "m_aScalarOverrides")}");
            foreach (MorphHead.ScalarParameter parameter in morphSource.ScalarParameters) {
                PropertyCollection props = new();

                if (applyToActor) {
                    props.Add(GenerateExpressionGUID());
                }
                props.Add(new NameProperty(parameter.Name, $"{(applyToActor ? "ParameterName" : "nName")}"));
                props.Add(new FloatProperty(parameter.Value, $"{(applyToActor ? "ParameterValue" : "sValue")}"));

                ScalarValues.Add(new StructProperty("ScalarParameter", props));
            }
            return ScalarValues;
        }

        /// <summary>
        /// Generate a Vector values property with the morphSource values
        /// </summary>
        /// <returns>A Vector values property</returns>
        private ArrayProperty<StructProperty> GenerateVectorValues() {
            ArrayProperty<StructProperty> VectorValues = new($"{(applyToActor ? "VectorParameterValues" : "m_aColorOverrides")}");
            foreach (MorphHead.VectorParameter parameter in morphSource.VectorParameters) {
                PropertyCollection props = new();

                PropertyCollection color = new();
                color.Add(new FloatProperty(parameter.Value.R, "R"));
                color.Add(new FloatProperty(parameter.Value.G, "G"));
                color.Add(new FloatProperty(parameter.Value.B, "B"));
                color.Add(new FloatProperty(parameter.Value.A, "A"));

                StructProperty ParameterValue = new("LinearColor", color, $"{(applyToActor ? "ParameterValue" : "cValue")}", true);

                if (applyToActor) {
                    props.Add(GenerateExpressionGUID());
                }
                props.Add(ParameterValue);
                props.Add(new NameProperty(parameter.Name, $"{(applyToActor ? "ParameterName" : "nName")}"));

                VectorValues.Add(new StructProperty($"{(applyToActor ? "VectorParameterValue" : "ColorParameter")}", props));
            }

            return VectorValues;
        }

        /// <summary>
        /// Generate a Texture values property with the morphSource values
        /// </summary>
        /// <returns>A Texture values property</returns>
        private ArrayProperty<StructProperty> GenerateTextureValues() {
            ArrayProperty<StructProperty> TextureValues = new($"{(applyToActor ? "TextureParameterValues" : "m_aTextureOverrides")}");
            foreach (MorphHead.TextureParameter parameter in morphSource.TextureParameters) {
                string textureName = parameter.Value.Remove(parameter.Value.Length - 1).Substring(1);

                if (textureName.Equals("None", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(textureName.Trim())) {
                    continue;
                }

                PropertyCollection props = new PropertyCollection();
                IEntry texture = GetResource(textureName);

                if (texture == null) {
                    if (!resourcesNotFound.ContainsKey(parameter.Name)) {
                        resourcesNotFound.Add(parameter.Name, $"{parameter.Name}: {textureName}");
                    }
                    continue;
                }

                if (applyToActor) {
                    props.Add(GenerateExpressionGUID());
                }
                props.Add(new NameProperty(parameter.Name, $"{(applyToActor ? "ParameterName" : "nName")}"));
                props.Add(new ObjectProperty(texture.UIndex, $"{(applyToActor ? "ParameterValue" : "m_pTexture")}"));

                TextureValues.Add(new StructProperty($"{(applyToActor ? "TextureParameterValue" : "TextureParameter")}", props));
            }

            return TextureValues;
        }

        /// <summary>
        /// Handle special morph texture cases, such as HED_Addn which overrides HED_Brow
        /// </summary>
        private void HandleSpecialMorphTextureCases() {
            HandleNoScarsCase();
            HandleAddBrowCase();
            if (targetGame.IsLEGame()) {
                HandleEyeEmisScalarLECase();
            }
        }

        /// <summary>
        /// Handle there case where there are no chose scars, setting blank scars to avoid the default Scars_03 used by the material
        /// </summary>
        private void HandleNoScarsCase() {
            int alignmentEmisIndex = morphSource.TextureParameters.FindIndex(tp => tp.Name == "HED_Face_Alignment_Emis");
            int alignmentNormIndex = morphSource.TextureParameters.FindIndex(tp => tp.Name == "HED_Face_Alignment_Norm");

            if (alignmentEmisIndex == -1) {
                MorphHead.TextureParameter defaultEmis = new();
                defaultEmis.Name = "HED_Face_Alignment_Emis";
                defaultEmis.Value = @"\BIOG_Humanoid_MASTER_MTR_R.GBL_ARM_ALL_Black\";
                morphSource.TextureParameters.Add(defaultEmis);
            } else if (alignmentEmisIndex >= 0) {
                string alignmentEmisValue = morphSource.TextureParameters[alignmentEmisIndex].Value;
                alignmentEmisValue = alignmentEmisValue.Remove(alignmentEmisValue.Length - 1).Substring(1);
                if (alignmentEmisValue.Equals("None", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(alignmentEmisValue.Trim())) {
                    morphSource.TextureParameters[alignmentEmisIndex].Value = @"\BIOG_Humanoid_MASTER_MTR_R.GBL_ARM_ALL_Black\";
                }
            }

            if (alignmentNormIndex == -1) {
                MorphHead.TextureParameter defaultNorm = new();
                defaultNorm.Name = "HED_Face_Alignment_Norm";
                defaultNorm.Value = @"\BIOG_Humanoid_MASTER_MTR_R.GBL_Norm_Alpha\";
                morphSource.TextureParameters.Add(defaultNorm);

            } else if (alignmentNormIndex >= 0) {
                string alignmentNormValue = morphSource.TextureParameters[alignmentNormIndex].Value;
                alignmentNormValue = alignmentNormValue.Remove(alignmentNormValue.Length - 1).Substring(1);
                if (alignmentNormValue.Equals("None", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(alignmentNormValue.Trim())) {
                    morphSource.TextureParameters[alignmentNormIndex].Value = @"\BIOG_Humanoid_MASTER_MTR_R.GBL_Norm_Alpha\";
                }
            }
        }

        /// <summary>
        /// Handle case where HED_Addn and HED_Brow are both in the morph
        /// </summary>
        private void HandleAddBrowCase() {
            int addnIndex = morphSource.TextureParameters.FindIndex(tp => tp.Name == "HED_Addn");
            int browIndex = morphSource.TextureParameters.FindIndex(tp => tp.Name == "HED_Brow");
            if (addnIndex >= 0 && browIndex >= 0) {
                morphSource.TextureParameters[addnIndex].Value = morphSource.TextureParameters[browIndex].Value;
            }
        }

        /// <summary>
        /// Handle case where eye emis scalar is named after the OT convention, which won't work in the LE
        /// </summary>
        private void HandleEyeEmisScalarLECase() {
            int emisScalar = morphSource.ScalarParameters.FindIndex(sp => sp.Name == "HED_Emis_Intensity");
            if (emisScalar >= 0) {
                morphSource.ScalarParameters[emisScalar].Name = "Emis_Scalar";
            }
        }
        
        /// <summary>
        /// Generate a default ExpressionGUID
        /// </summary>
        /// <returns>ExpressionGUID StructProperty</returns>
        private StructProperty GenerateExpressionGUID() {
            PropertyCollection props = new PropertyCollection();
            props.Add(new IntProperty(0, "A"));
            props.Add(new IntProperty(0, "B"));
            props.Add(new IntProperty(0, "C"));
            props.Add(new IntProperty(0, "D"));

            return new StructProperty("Guid", props, "ExpressionGUID", true);
        }
    }
}
