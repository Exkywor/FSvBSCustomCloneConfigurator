using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Tools {
    /// <summary>
    /// Indicates a character's gender in a ME3 game
    /// </summary>
    /// <remarks><see cref="GenderExtensions">Extension Methods</see></remarks>
    public enum Gender {
        Male,
        Female
    }
    /// <summary>
    /// Extension methods for the Gender enum
    /// </summary>
    public static class GenderExtensions {
        /// <summary>
        /// Is gender male
        /// </summary>
        /// <param name="gender">Input gender</param>
        /// <returns></returns>
        public static bool IsMale(this Gender gender) => gender is Gender.Male;

        /// <summary>
        /// Is gender female
        /// </summary>
        /// <param name="gender">Input gender</param>
        /// <returns></returns>
        public static bool IsFemale(this Gender gender) => gender is Gender.Female;
    }
}
