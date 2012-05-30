namespace RavenQuestionnaire.Core.Utility
{
    public static class IdUtil
    {
        public static string CreateImageId(string id)
        {
            return string.Format("images/{0}", id);
        }
        public static string CreateFileId(string id)
        {
            return string.Format("filedocuments/{0}", id);
        }
        public static string CreateFlowGraphId(string id)
        {
            return string.Format("flowgraphdocuments/{0}", id);
        }
        public static string CreateStatisticId(string id)
        {
            return string.Format("completeqquestionnairestatisticdocument/{0}", id);
        }
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
        public static string CreateReportId(string id)
        {
            return string.Format("reportdocuments/{0}", id);
        }

        public static string CreateCollectionId(string id)
        {
            return string.Format("collectiondocuments/{0}", id);
        }

        //public static string CreateAttributeId(string id)
        //{
        //    return string.Format("attributedocuments/{0}", id);
        //}

    }
}
