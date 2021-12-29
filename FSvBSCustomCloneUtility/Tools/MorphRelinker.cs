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
        private int GLOBAL_MAT_INDEX = 1000;
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
            this.archetype = pcc.FindExport($"BioChar_CustomDummy.Archetypes.fsvbs_dummy_custom_{(gender.IsMale() ? "male" : "female")}_Con");
        }

        public void RelinkMorph() {
            foreach (string file in targetFiles) {
                using IMEPackage pcc = MEPackageHandler.OpenMEPackage(file);

                // Skip gender specific files if gender doesn't match
                if (pcc.FileNameNoExtension == "BioD_Cit004_273FemClone" && gender.IsMale()) { continue; }
                else if (pcc.FileNameNoExtension == "BioD_Cit004_272MaleClone" && gender.IsFemale()) { continue; }

                ExportEntry clonedArchetype = CloneDummyArchetype(archetype, file);

                List<ExportEntry> targetExports = GetTargetExports(pcc); // Some files contain multiple exports to relink
                if (!targetExports.Any()) { return; } // No exports found

                foreach(ExportEntry export in targetExports) {
                    // TODO: Add check in case any get returns null
                    ExportEntry sourceHairSMC = SMCTools.GetHairSMC(clonedArchetype, pcc);
                    ExportEntry targetHairSMC = SMCTools.GetHairSMC(export, pcc);
                    SMCTools.SetHairToSMC(sourceHairSMC, targetHairSMC);

                    ExportEntry sourceHeadSMC = SMCTools.GetHeadSMC(clonedArchetype, pcc);
                    ExportEntry targetHeadSMC = SMCTools.GetHeadSMC(export, pcc);
                    SMCTools.SetHeadToSMC(sourceHeadSMC, targetHeadSMC);

                    ArrayProperty<ObjectProperty> hairMaterials = sourceHairSMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
                    if (hairMaterials != null) {
                        GLOBAL_MAT_INDEX = SMCTools.CloneMaterialsToSMC(hairMaterials, targetHairSMC, pcc, GLOBAL_MAT_INDEX);
                    }

                    ArrayProperty<ObjectProperty> headMaterials = sourceHeadSMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
                    if (headMaterials != null) {
                        GLOBAL_MAT_INDEX = SMCTools.CloneMaterialsToSMC(headMaterials, targetHeadSMC, pcc, GLOBAL_MAT_INDEX);
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
        private List<ExportEntry> GetTargetExports(IMEPackage pcc) {
            switch (pcc.FileNameNoExtension) {
                case "BioD_Cit002_700Exit":
                    List<ExportEntry> exports700 = new();
                    ExportEntry export700 = pcc.FindExport($"TheWorld.PersistentLevel.SFXStuntActor_{(gender.IsFemale() ? 73 : 82)}");
                    if (export700 != null) { exports700.Add(export700); }
                    return exports700;
                case "BioD_Cit004_210CICIntro":
                    List<ExportEntry> exports210 = new();
                    ExportEntry export210 = pcc.FindExport($"BioChar_CitGlobal.Archetypes.cit_evilclone_{(gender.IsFemale() ? "female" : "male")}_fatigue_Con");
                    if (export210 != null) { exports210.Add(export210); }
                    return exports210;
                case "BioD_Cit004_272MaleClone": {
                        List<ExportEntry> exports272 = new();
                        ExportEntry exportA = pcc.FindExport($"Char_Enemies_Citadel.Bosses.CloneShepardMale_Conv");
                        if (exportA != null) { exports272.Add(exportA); }
                        ExportEntry exportB = pcc.FindExport($"Char_Enemies_Citadel.Bosses.CloneShepardMale");
                        if (exportB != null) { exports272.Add(exportB); }
                        ExportEntry exportC = pcc.FindExport($"Char_Henchmen.Archetypes.CloneM.VariantA.CloneM_Combat");
                        if (exportC != null) { exports272.Add(exportC); }
                        ExportEntry exportD = pcc.FindExport($"Char_Henchmen.Archetypes.CloneM.VariantA.CloneM_Conversation");
                        if (exportD != null) { exports272.Add(exportD); }
                        return exports272;
                    }
                case "BioD_Cit004_273FemClone": {
                        List<ExportEntry> exports273 = new();
                        ExportEntry exportA = pcc.FindExport($"Char_Enemies_Citadel.Bosses.CloneShepardFemale_Conv");
                        if (exportA != null) { exports273.Add(exportA); }
                        ExportEntry exportB = pcc.FindExport($"Char_Enemies_Citadel.Bosses.CloneShepardFemale");
                        if (exportB != null) { exports273.Add(exportB); }
                        ExportEntry exportC = pcc.FindExport($"Char_Henchmen.Archetypes.CloneM.VariantA.CloneF_Combat");
                        if (exportC != null) { exports273.Add(exportC); }
                        ExportEntry exportD = pcc.FindExport($"Char_Henchmen.Archetypes.CloneM.VariantA.CloneF_Conversation");
                        if (exportD != null) { exports273.Add(exportD); }
                        return exports273;
                    }
                default:
                    List<ExportEntry> exports = new();
                    ExportEntry export = pcc.FindExport($"BioChar_CitGlobal.Archetypes.cit_evilclone_{(gender.IsFemale() ? "female" : "male")}_Con");
                    if (export != null) { exports.Add(export); }
                    return exports;
            }
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
