using FSvBSCustomCloneUtility.Tools;
using FSvBSCustomCloneUtility.ViewModels;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Packages.CloningImportingAndRelinking;
using LegendaryExplorerCore.Unreal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Clones and relinks the morph into target files and archetypes
    /// </summary>
    public class MorphRelinker {
        private Gender gender;
        private ExportEntry archetype;
        private List<string> targetFiles = new();
        /// <summary>
        /// Instantiate the Morph Relinker
        /// </summary>
        /// <param name="game">Game to relink to</param>
        /// <param name="gender">Gender of the morph</param>
        public MorphRelinker(MEGame game, Gender gender) {
            Load(game, gender);
        }

        private void Load(MEGame game, Gender gender) {
            this.gender = gender;
            this.targetFiles = FSvBSDirectories.GetCloneInstancesPaths(game);

            using IMEPackage pcc = MEPackageHandler.OpenMEPackage(FSvBSDirectories.GetDummiesPath(game));
            this.archetype = pcc.FindExport($"BioChar_CustomDummy.Archetypes.fsvbs_dummy_custom_{(gender == Gender.Male ? "male" : "female")}_Con");
        }

        public void RelinkMorph() {
            foreach (string file in targetFiles) {
                using IMEPackage pcc = MEPackageHandler.OpenMEPackage(file);

                // Skip gender specific files if gender doesn't match
                if (pcc.FileNameNoExtension == "BioD_Cit004_273FemClone" && gender == Gender.Male) { continue; }
                else if (pcc.FileNameNoExtension == "BioD_Cit004_272MaleClone" && gender == Gender.Female) { continue; }

                ExportEntry clonedArchetype = CloneDummyArchetype(archetype, file);

                IEnumerable<ExportEntry> targetExports = GetTargetExports(pcc); // Some files contain multiple exports to relink
                if (targetExports == null) { return; } // No exports found

                foreach(ExportEntry export in targetExports.ToList()) {
                    // TODO: Add check in case any get returns null
                    ExportEntry sourceHairSMC = GetHairSMC(clonedArchetype, pcc);
                    ExportEntry targetHairSMC = GetHairSMC(export, pcc);
                    SetHairToSMC(sourceHairSMC, targetHairSMC);

                    ExportEntry sourceHeadSMC = GetHeadSMC(clonedArchetype, pcc);
                    ExportEntry targetHeadSMC = GetHeadSMC(export, pcc);
                    SetHeadToSMC(sourceHeadSMC, targetHeadSMC);

                    ArrayProperty<ObjectProperty> hairMaterials = sourceHairSMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
                    if (hairMaterials != null) {
                        CloneMaterialsToSMC(hairMaterials, targetHairSMC, pcc);
                    }

                    ArrayProperty<ObjectProperty> headMaterials = sourceHeadSMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
                    if (headMaterials != null) {
                        CloneMaterialsToSMC(headMaterials, targetHeadSMC, pcc);
                    }

                    SetMorphToTarget(pcc.GetUExport(clonedArchetype.GetProperty<ObjectProperty>("MorphHead").Value), export);
                }

                pcc.Save();
            }
        }

        /// <summary>
        /// Clone the input archetype to the target file
        /// </summary>
        /// <param name="archetype">Morph to clone</param>
        /// <param name="file">Target file</param>
        /// <returns>Cloned archetype export</returns>
        private ExportEntry CloneDummyArchetype(ExportEntry archetype, string filePath) {
            EntryExporter.ExportExportToFile(archetype, filePath, out IEntry res);
            return (ExportEntry) res;
        }

        /// <summary>
        /// Get the exports to which to relink the morph to
        /// </summary>
        /// <param name="pcc">Pcc containing the target</param>
        /// <returns>Target exports; null if not found</returns>
        private IEnumerable<ExportEntry> GetTargetExports(IMEPackage pcc) {
            switch (pcc.FileNameNoExtension) {
                case "BioD_Cit002_700Exit":
                    IEnumerable<ExportEntry> exports700 = pcc.Exports.Where(e => {
                        if (e.ClassName == "SFXStuntActor") {
                            NameProperty tag = e.GetProperty<NameProperty>("Tag");
                            return (tag != null && tag.Value == $"fakeclone_{(gender == Gender.Female ? "female" : "male")}");
                        } else { return false; }
                    });
                    if (exports700.Any()) { return exports700; }
                    break;
                case "BioD_Cit004_210CICIntro":
                    IEnumerable<ExportEntry> exports210 = pcc.Exports.Where(e =>
                        e.ClassName == "SFXStuntActor" && e.ObjectName == $"cit_evilclone_{(gender == Gender.Male ? "male" : "female")}_fatigue_Con");
                    if (exports210.Any()) { return exports210; }
                    break;
                case "BioD_Cit004_272MaleClone":
                    IEnumerable<ExportEntry> exports272 = pcc.Exports.Where(e =>
                           e.ObjectName == "CloneShepardMale_Conv" || e.ObjectName == "CloneShepardMale"
                        || e.ObjectName == "CloneM_Combat" || e.ObjectName == "CloneM_Conversation");
                    if (exports272.Any()) { return exports272; }
                    break;
                case "BioD_Cit004_273FemClone":
                    IEnumerable<ExportEntry> exports273 = pcc.Exports.Where(e =>
                           e.ObjectName == "CloneShepardFemale_Conv" || e.ObjectName == "CloneShepardFemale"
                        || e.ObjectName == "CloneF_Combat" || e.ObjectName == "CloneF_Conversation");
                    if (exports273.Any()) { return exports273; }
                    break;
                default:
                    IEnumerable<ExportEntry> exports = pcc.Exports.Where(e =>
                        e.ClassName == "SFXStuntActor" && e.ObjectName == $"cit_evilclone_{(gender == Gender.Male ? "male" : "female")}_Con");
                    if (exports.Any()) { return exports; }
                    break;
            }
            return null;
        }

        /// <summary>
        /// Get the hair SMC from the target
        /// </summary>
        /// <param name="target">Export to get the hair SMC from</param>
        /// <param name="pcc">Pcc to find export in</param>
        /// <returns>Hair SMC export; null if no mesh was found </returns>
        private ExportEntry GetHairSMC(ExportEntry target, IMEPackage pcc) {
            ObjectProperty hairIndex = target.GetProperty<ObjectProperty>("HairMesh");
            if (hairIndex != null) { return pcc.GetUExport(hairIndex.Value); }

            // Some of the exports use m_oHairMesh
            hairIndex = target.GetProperty<ObjectProperty>("m_oHairMesh");
            if (hairIndex != null) { return pcc.GetUExport(hairIndex.Value); }

            return null;
        }

        /// <summary>
        /// Get the head SMC from the target
        /// </summary>
        /// <param name="target">Export to get the head SMC from</param>
        /// <param name="pcc">Pcc to find export in</param>
        /// <returns>Head SMC export</returns>
        private ExportEntry GetHeadSMC(ExportEntry target, IMEPackage pcc) {
            ObjectProperty headIndex = target.GetProperty<ObjectProperty>("HeadMesh");
            if (headIndex != null) { return pcc.GetUExport(headIndex.Value); }
            return null;
        }

        /// <summary>
        /// Clone the input materials into the target SkeletalMeshComponent
        /// </summary>
        /// <param name="materials">Materials to clone</param>
        /// <param name="SMC">Target SkeletalMeshComponent</param>
        /// <param name="pcc">Pcc to clone materials from and into</param>
        private void CloneMaterialsToSMC(ArrayProperty<ObjectProperty> materials, ExportEntry SMC, IMEPackage pcc) {
            ArrayProperty<ObjectProperty> targetMaterials = SMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
            if (targetMaterials == null) { targetMaterials = new ArrayProperty<ObjectProperty>("Materials"); }

            foreach (ObjectProperty material in materials) {
                ExportEntry clonedMat = EntryCloner.CloneEntry(pcc.GetUExport(material.Value));
                clonedMat.idxLink = SMC.UIndex;
                targetMaterials.Add(new ObjectProperty(clonedMat.idxLink));
            }

            SMC.WriteProperty(targetMaterials);
        }

        /// <summary>
        /// Set the input headMesh to the target SkeletalMeshComponent
        /// </summary>
        /// <param name="sourceSMC">Source SMC containing the head to set</param>
        /// <param name="targetSMC">Target SMC</param>
        private void SetHeadToSMC(ExportEntry sourceSMC, ExportEntry targetSMC) {
            ObjectProperty sourceHead = sourceSMC.GetProperty<ObjectProperty>("SkeletalMesh");
            if (sourceHead == null) { return; } // No head mesh to reference

            ObjectProperty targetHead = targetSMC.GetProperty<ObjectProperty>("SkeletalMesh");
            if (targetHead != null) {
                targetHead.Value = sourceHead.Value; 
            } else {
                targetHead = new ObjectProperty(sourceHead.Value, "SkeletalMesh");
            }

            targetSMC.WriteProperty(targetHead);
        }

        /// <summary>
        /// Set the input hairMesh to the target SkeletalMeshComponent
        /// </summary>
        /// <param name="sourceSMC">Source SMC containing the hair to set</param>
        /// <param name="targetSMC">Target SMC</param>
        private void SetHairToSMC(ExportEntry sourceSMC, ExportEntry targetSMC) {
            ObjectProperty sourceHair = sourceSMC.GetProperty<ObjectProperty>("SkeletalMesh");
            if (sourceHair == null) { return; } // No head mesh to reference

            ObjectProperty targetHair = targetSMC.GetProperty<ObjectProperty>("SkeletalMesh");
            if (targetHair != null) {
                targetHair.Value = sourceHair.Value; 
            } else {
                targetHair = new ObjectProperty(sourceHair.Value, "SkeletalMesh");
            }

            targetSMC.WriteProperty(targetHair);
        }

        /// <summary>
        /// Set the morph to the target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="morph">Morph to link</param>
        private void SetMorphToTarget(ExportEntry morph, ExportEntry target) {
            ObjectProperty targetMorph = target.GetProperty<ObjectProperty>("MorphHead");
            if (targetMorph != null) {
                targetMorph.Value = morph.UIndex; 
            } else {
                targetMorph = new ObjectProperty(morph.UIndex, "MorphHead");
            }

            target.WriteProperty(targetMorph);
        }
    }
}
