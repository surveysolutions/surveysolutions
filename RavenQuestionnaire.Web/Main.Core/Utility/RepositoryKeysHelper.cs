using System;

namespace Main.Core.Utility
{
    public static class RepositoryKeysHelper
    {
        public static string GetVersionedKey(Guid id, long version)
        {
            return GetVersionedKey(id.ToString(), version);
        }

        public static string GetVersionedKey(string id, long version)
        {
            return string.Format("{0}-{1}", id, version);
        }

        public static string GetVariableByQuestionnaireKey(string variableName, string questionnaireVersiondKey)
        {
            return string.Format("{0}-{1}", variableName, questionnaireVersiondKey);
        }
    }
}