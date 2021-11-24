using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools
{
    public static class ConditionalsManager
    {
        private static string path;

        public static bool SetConditional(Gender gender, bool state, string path)
        {
            ConditionalsManager.path = path;
            return false;
        }


        public static bool CheckConditional(Gender gender)
        {
            return false;
        }

        private static bool AddConditional(Gender gender)
        {
            return false;
        }

        private static bool RemoveConditional(Gender gender)
        {
            return false;
        }
    }

}
