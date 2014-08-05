using System.Collections.Generic;
using System.Linq;

namespace WB.Tools.ConsoleTranslator
{
    internal static class EntryExtenstions
    {
        public static bool HasTranslation(this Entry entry)
        {
            return entry != null && !string.IsNullOrWhiteSpace(entry.TargetLanguage);
        }

        public static Entry GetQuestionEntry(this IEnumerable<Entry> entries, string questionVariable)
        {
            return entries.SingleOrDefault(entry => entry.Variable == questionVariable && entry.Code == string.Empty);
        }

        public static Entry GetAnswerOptionEntry(this IEnumerable<Entry> entries, string questionVariable, string answerOptionCode)
        {
            return entries.SingleOrDefault(entry => entry.Variable == questionVariable && entry.Code == answerOptionCode);
        }
    }
}