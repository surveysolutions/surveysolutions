namespace Core.Supervisor
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    using Core.Supervisor.Views.Survey;

    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Converts string into the md5 hash and coverts bytes to Guid
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The guid
        /// </returns>
        public static Guid StringToGuid(string key)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(key));
            return new Guid(data);
        }

        /// <summary>
        /// This extention generates screen key for groups
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// Group's key for cleaner navigation
        /// </returns>
        public static ScreenKey GetKey (this ICompleteGroup group)
        {
            return new ScreenKey(group);
        }
    }
}
