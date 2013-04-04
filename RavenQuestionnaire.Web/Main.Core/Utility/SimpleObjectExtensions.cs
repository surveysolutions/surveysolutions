// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleObjectExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   The simple object extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Utility
{
    /// <summary>
    /// The simple object extensions.
    /// </summary>
    public static class SimpleObjectExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The is integer.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsInteger(this string source)
        {
            int iSource;
            return int.TryParse(source, out iSource);
        }

        #endregion
    }
}