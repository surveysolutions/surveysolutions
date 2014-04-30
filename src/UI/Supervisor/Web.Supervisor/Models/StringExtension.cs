namespace Web.Supervisor.Models
{
    using System;
    using System.Linq;

    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Make acronim from sentence
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The acronim
        /// </returns>
        public static string Acronim(this string str)
        {
            return string.Join(
                string.Empty, str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]));
        }
    }
}