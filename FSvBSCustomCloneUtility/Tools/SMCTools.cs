using LegendaryExplorerCore.Packages.CloningImportingAndRelinking;
using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.Unreal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Commonly used tools to operate on SkeletalMeshActors
    /// </summary>
    public static class SMCTools {
        /// <summary>
        /// Get the hair SMC from the target
        /// </summary>
        /// <param name="target">Export to get the hair SMC from</param>
        /// <param name="pcc">Pcc to find export in</param>
        /// <returns>Hair SMC export; null if no mesh was found </returns>
        public static ExportEntry GetHairSMC(ExportEntry target, IMEPackage pcc) {
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
        public static ExportEntry GetHeadSMC(ExportEntry target, IMEPackage pcc) {
            ObjectProperty headIndex = target.GetProperty<ObjectProperty>("HeadMesh");
            if (headIndex != null) { return pcc.GetUExport(headIndex.Value); }
            return null;
        }

        /// Clone the input materials into the target SkeletalMeshComponent
        /// </summary>
        /// <param name="materials">Materials to clone</param>
        /// <param name="SMC">Target SkeletalMeshComponent</param>
        /// <param name="pcc">Pcc to clone materials from and into</param>
        /// <returns>The last material index</returns>
        public static int CloneMaterialsToSMC(ArrayProperty<ObjectProperty> materials, ExportEntry SMC, IMEPackage pcc, int idx = -1) {
            ArrayProperty<ObjectProperty> targetMaterials = SMC.GetProperty<ArrayProperty<ObjectProperty>>("Materials");
            if (targetMaterials == null) { targetMaterials = new ArrayProperty<ObjectProperty>("Materials"); }

            foreach (ObjectProperty material in materials) {
                ExportEntry clonedMat = EntryCloner.CloneEntry(pcc.GetUExport(material.Value));
                clonedMat.idxLink = SMC.UIndex;
                if (idx > -1) {
                    clonedMat.indexValue = ++idx;
                }

                targetMaterials.Add(new ObjectProperty(clonedMat.UIndex));
            }

            SMC.WriteProperty(targetMaterials);
            return idx;
        }

        /// <summary>
        /// Set the input headMesh to the target SkeletalMeshComponent
        /// </summary>
        /// <param name="sourceSMC">Source SMC containing the head to set</param>
        /// <param name="targetSMC">Target SMC</param>
        public static void SetHeadToSMC(ExportEntry sourceSMC, ExportEntry targetSMC) {
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
        public static void SetHairToSMC(ExportEntry sourceSMC, ExportEntry targetSMC) {
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
    }
}
