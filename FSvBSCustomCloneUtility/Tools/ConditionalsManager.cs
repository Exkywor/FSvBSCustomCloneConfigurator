using FSvBSCustomCloneUtility.ViewModels;
using LegendaryExplorerCore.Coalesced;
using LegendaryExplorerCore.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FSvBSCustomCloneUtility.Tools
{
    public static class ConditionalsManager
    {
        private static string coalPath;
        private static CaseInsensitiveDictionary<string> coal = new();
        private static XmlDocument BioGame = new();
        private static XmlNode Property;

        public static bool SetConditional(Gender gender, bool state, string path)
        {
            LoadResources(path);

            bool exists = CheckConditional(gender);
            if (exists && !state)
            {
                bool res = RemoveConditional(gender);
                coal["BioGame.xml"] = BioGame.OuterXml;
                WriteCoal();

                return res;
            } else if (!exists && state)
            {
                bool res = AddConditional(gender);
                coal["BioGame.xml"] = BioGame.OuterXml;
                WriteCoal();

                return res;
            }

            return true;
        }

        private static void LoadResources(string path)
        {
            path = @$"{path.Substring(0, path.IndexOf("CookedPCConsole", StringComparison.OrdinalIgnoreCase) + 15)}";

            coalPath = path.Contains("DLC_MOD_FSvBSLE") ? @$"{path}\Default_DLC_MOD_FSvBSLE.bin"
                                                        : @$"{path}\Default_DLC_MOD_FSvBS.bin";

            using FileStream fs = File.OpenRead(coalPath);
            coal = CoalescedConverter.DecompileGame3ToMemory(fs);

            BioGame.LoadXml(coal["BioGame.xml"]);
            Property = BioGame.SelectSingleNode("//Property[@name=\"timedplotunlocks\"]");
        }

        public static bool CheckConditional(Gender gender)
        {
            return gender is Gender.Male ? Property.InnerXml.Contains("71174570")
                                         : Property.InnerXml.Contains("71174571");
        }

        private static bool AddConditional(Gender gender)
        {
            string plotBool = gender is Gender.Male ? "71174570" : "71174571";
            if (Property.InnerXml.Contains("Value"))
            {
                Property.AppendChild(CreateVal(plotBool));
            } else
            {
                Property.InnerXml = "";
                Property.InnerText = "";
                Property.AppendChild(CreateVal("71174566"));
                Property.AppendChild(CreateVal(plotBool));
            }
            return true;
        }

        private static bool RemoveConditional(Gender gender)
        {
            string targetBool = gender is Gender.Male ? "71174570" : "71174571";
            foreach (XmlElement child in Property.ChildNodes)
            {
                if (child.InnerXml.ToString().Contains(targetBool)) {
                    Property.RemoveChild(child);
                }
            }
            return false;
        }

        private static XmlElement CreateVal(string plotBool)
        {
            XmlElement val = BioGame.CreateElement("Value");
            XmlAttribute type = BioGame.CreateAttribute("type");
            type.Value = "3";
            val.Attributes.Append(type);

            val.InnerText = $"(PlotBool = {plotBool}, UnlockDay = 0)";

            return val;
        }

        private static void WriteCoal()
        {
            MemoryStream ms = CoalescedConverter.CompileFromMemory(coal);
            File.WriteAllBytes(coalPath, ms.ToArray());
        }
    }
}
