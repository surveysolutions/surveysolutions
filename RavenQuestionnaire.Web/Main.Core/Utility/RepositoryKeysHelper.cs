using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.Utility
{
    public static class RepositoryKeysHelper
    {
        public static string GetVersionedKey(Guid id, long version)
        {
            return GetVersionedKey(id.FormatGuid(), version);
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