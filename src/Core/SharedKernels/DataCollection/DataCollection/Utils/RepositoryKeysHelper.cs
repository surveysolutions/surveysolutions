using System;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class RepositoryKeysHelper
    {
        public static string GetVersionedKey(string id, long version)
        {
            return String.Format("{0}-{1}", id, version);
        }

        public static string GetVariableByQuestionnaireKey(string variableName, string questionnaireVersiondKey)
        {
            return String.Format("{0}-{1}", variableName, questionnaireVersiondKey);
        }

        public static string GetVersionedKey(Guid id, long version)
        {
            return GetVersionedKey(id.FormatGuid(), version);
        }
    }
}