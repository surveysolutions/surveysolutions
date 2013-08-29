namespace Questionnaire.Core.Web.Helpers
{
    /// <summary>
    /// The client id helper.
    /// </summary>
    public static class ClientIdHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get unique id by entity name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetUniqueIdByEntityName(EntityNames name, string id)
        {
            return string.Format("tag{0}_{1}", name, id);
        }

        #endregion
    }
}