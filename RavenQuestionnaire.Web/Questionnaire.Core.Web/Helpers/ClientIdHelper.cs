namespace Questionnaire.Core.Web.Helpers
{
    public static class ClientIdHelper
    {
        public static string GetUniqueIdByEntityName(EntityNames name, string id)
        {
            return string.Format("tag{0}_{1}", name, id);
        }
    }
}