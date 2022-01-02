using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Enums {
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
        /// <returns>True if gender is male</returns>
        public static bool IsMale(this Gender gender) => gender is Gender.Male;

        /// <summary>
        /// Is gender female
        /// </summary>
        /// <param name="gender">Input gender</param>
        /// <returns>True if gender is female</returns>
        public static bool IsFemale(this Gender gender) => gender is Gender.Female;
    }
}
