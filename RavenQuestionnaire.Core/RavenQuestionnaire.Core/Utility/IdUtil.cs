// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdUtil.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Util class to create id
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Utility
{
    // TODO: delete all references to this class changing id to guid
    /// <summary>
    /// Util class to create id
    /// </summary>
    public static class IdUtil
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create complete questionnaire id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateCompleteQuestionnaireId(string id)
        {
            return string.Format("completequestionnairedocuments/{0}", id);
        }

        /// <summary>
        /// The create file id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateFileId(string id)
        {
            return string.Format("filedocuments/{0}", id);
        }

        /// <summary>
        /// The create flow graph id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateFlowGraphId(string id)
        {
            return string.Format("flowgraphdocuments/{0}", id);
        }

        /// <summary>
        /// The create image id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateImageId(string id)
        {
            return string.Format("images/{0}", id);
        }

        /// <summary>
        /// The create location id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateLocationId(string id)
        {
            return string.Format("locationdocuments/{0}", id);
        }

        /// <summary>
        /// The create questionnaire id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateQuestionnaireId(string id)
        {
            return id; // string.Format("questionnairedocuments/{0}", id);
        }

        /// <summary>
        /// The create report id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateReportId(string id)
        {
            return string.Format("reportdocuments/{0}", id);
        }

        /// <summary>
        /// The create statistic id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateStatisticId(string id)
        {
            return string.Format("completeqquestionnairestatisticdocument/{0}", id);
        }

        /// <summary>
        /// The create status id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateStatusId(string id)
        {
            return string.Format("statusdocuments/{0}", id);
        }

        /// <summary>
        /// The create user id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string CreateUserId(string id)
        {
            return id;
        }

        /// <summary>
        /// The parse id.
        /// </summary>
        /// <param name="ravenId">
        /// The raven id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string ParseId(string ravenId)
        {
            if (string.IsNullOrEmpty(ravenId))
            {
                return ravenId;
            }

            return ravenId.Substring(ravenId.LastIndexOf('/') + 1);
        }

        #endregion
    }
}