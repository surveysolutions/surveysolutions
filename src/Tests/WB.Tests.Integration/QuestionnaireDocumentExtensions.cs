using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Integration
{
    internal static class QuestionnaireDocumentExtensions
    {
        public static Group AddChapter(this QuestionnaireDocument document, Guid groupId)
        {
            var group = new Group(string.Format("Chapter {0}", groupId))
            {
                PublicKey = groupId
            };
            document.Add(@group, null);
            return group;
        }
    }
}