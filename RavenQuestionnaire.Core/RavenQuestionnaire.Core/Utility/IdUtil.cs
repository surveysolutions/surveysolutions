namespace RavenQuestionnaire.Core.Utility
{
    public static class IdUtil
    {
        public static string CreateQuestionnaireId(string id)
        {
            return string.Format("questionnairedocuments/{0}", id);
        }
        public static string ParseId(string ravenId)
        {
            if (string.IsNullOrEmpty(ravenId))
                return ravenId;
            return ravenId.Substring(ravenId.IndexOf('/') + 1);

        }
        public static string CreateCompleteQuestionnaireId(string id)
        {
            return string.Format("completequestionnairedocuments/{0}", id);
        }
        public static string CreateUserId(string id)
        {
            return string.Format("userdocuments/{0}", id);
        }
        public static string CreateLocationId(string id)
        {
            return string.Format("locationdocuments/{0}", id);
        }

        public static string CreateStatusId(string id)
        {
            return string.Format("statusdocuments/{0}", id);
        }
    }
}
