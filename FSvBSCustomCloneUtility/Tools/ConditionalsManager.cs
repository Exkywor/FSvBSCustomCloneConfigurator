using FSvBSCustomCloneUtility.Tools;
using FSvBSCustomCloneUtility.ViewModels;
using LegendaryExplorerCore.Coalesced;
using LegendaryExplorerCore.Misc;
using LegendaryExplorerCore.Packages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Static methods to control the custom clone conditionals
    /// </summary>
    public static class ConditionalsManager {
        private static CaseInsensitiveDictionary<string> coal = new(); // Decompiled coalesced file
        private static XmlDocument BioGame = new(); // Parsed coalesced file
        private static XmlNode Property; // Property to add to coalesced

        /// <summary>
        /// Sets the state to toggle the custom clone appearance for the input game
        /// </summary>
        /// <param name="gender">Target Shepard</param>
        /// <param name="state">True for custom conditional, false to remove it</param>
        /// <param name="game">Target game</param>
        public static void SetConditional(Gender gender, bool state, MEGame game) {
            LoadResources(game);

            bool exists = CheckConditional(gender);
            if (exists && !state) {
                RemoveConditional(gender);
                coal["BioGame.xml"] = BioGame.OuterXml;
                WriteCoal(game);
            } else if (!exists && state) {
                AddConditional(gender);
                coal["BioGame.xml"] = BioGame.OuterXml;
                WriteCoal(game);
            }
        }

        /// <summary>
        /// Loads and assigns the variables that the methods will need
        /// </summary>
        /// <param name="game">Game to load for</param>
        private static void LoadResources(MEGame game) {
            using FileStream fs = File.OpenRead(FSvBSDirectories.GetBinPath(game));
            coal = CoalescedConverter.DecompileGame3ToMemory(fs);

            BioGame.LoadXml(coal["BioGame.xml"]);
            Property = BioGame.SelectSingleNode("//Property[@name=\"timedplotunlocks\"]");
        }

        /// <summary>
        /// Checks the state of the conditional for the input gender
        /// </summary>
        /// <param name="gender">Gender to check for</param>
        /// <returns>True if the custom condition is set for the input gender</returns>
        private static bool CheckConditional(Gender gender) {
            return gender is Gender.Male ? Property.InnerXml.Contains("71174570")
                                         : Property.InnerXml.Contains("71174571");
        }
        /// <summary>
        /// Checks the state of the conditional for the input gender and the selected game
        /// </summary>
        /// <param name="game">Game to check for</param>
        /// <param name="gender">Gender to check for</param>
        /// <returns>True if the custom condition is set for the input gender</returns>
        public static bool CheckConditional(Gender gender, MEGame game) {
            LoadResources(game);
            return gender is Gender.Male ? Property.InnerXml.Contains("71174570")
                                         : Property.InnerXml.Contains("71174571");

        }

        /// <summary>
        /// Add the custom condition for the input gender
        /// </summary>
        /// <param name="gender">Gender to check for</param>
        private static void AddConditional(Gender gender) {
            string plotBool = gender is Gender.Male ? "71174570" : "71174571";
            if (Property.InnerXml.Contains("Value")) {
                Property.AppendChild(CreateVal(plotBool));
            } else {
                Property.InnerXml = "";
                Property.InnerText = "";
                Property.AppendChild(CreateVal("71174566"));
                Property.AppendChild(CreateVal(plotBool));
            }
        }

        /// <summary>
        /// Remove the custom condition for the input gender
        /// </summary>
        /// <param name="gender">Gender to check for</param>
        private static void RemoveConditional(Gender gender) {
            string targetBool = gender is Gender.Male ? "71174570" : "71174571";
            foreach (XmlElement child in Property.ChildNodes) {
                if (child.InnerXml.ToString().Contains(targetBool)) {
                    Property.RemoveChild(child);
                }
            }
        }

        /// <summary>
        /// Creates the value with the input plotBool to add to the XML
        /// </summary>
        /// <param name="plotBool">Bool to add</param>
        /// <returns>The XML element to add to the coalesced</returns>
        private static XmlElement CreateVal(string plotBool) {
            XmlElement val = BioGame.CreateElement("Value");
            XmlAttribute type = BioGame.CreateAttribute("type");
            type.Value = "3";
            val.Attributes.Append(type);

            val.InnerText = $"(PlotBool = {plotBool}, UnlockDay = 0)";

            return val;
        }

        private static void WriteCoal(MEGame game) {
            MemoryStream ms = CoalescedConverter.CompileFromMemory(coal);
            File.WriteAllBytes(FSvBSDirectories.GetBinPath(game), ms.ToArray());
        }
    }
}
