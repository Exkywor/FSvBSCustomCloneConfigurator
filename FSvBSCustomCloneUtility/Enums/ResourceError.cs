using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Enums {
    /// <summary>
    /// Indicates a resource error type
    /// </summary>
    /// <remarks><see cref="ResourceErrorExtensions">Extension Methods</see></remarks>
    public enum ResourceError {
        Duplicates,
        NotFound
    }

    /// <summary>
    /// Extension methods for the ResourceError enum
    /// </summary>
    public static class ResourceErrorExtensions {
        /// <summary>
        /// Is duplicates error
        /// </summary>
        /// <param name="error">Input error</param>
        /// <returns>True if error is duplicates</returns>
        public static bool IsDuplicates(this ResourceError error) => error is ResourceError.Duplicates;

        /// <summary>
        /// Is not found error
        /// </summary>
        /// <param name="error">Input error</param>
        /// <returns>True if error is not found</returns>
        public static bool IsNotFound(this ResourceError error) => error is ResourceError.NotFound;
    }
}
