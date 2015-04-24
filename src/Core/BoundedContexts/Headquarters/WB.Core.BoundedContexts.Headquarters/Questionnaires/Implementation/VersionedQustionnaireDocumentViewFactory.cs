using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation
{
    internal class VersionedQustionnaireDocumentViewFactory : IVersionedQuestionnaireReader
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;

        public VersionedQustionnaireDocumentViewFactory(IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage)
        {
            if (documentStorage == null) throw new ArgumentNullException("documentStorage");
            this.documentStorage = documentStorage;
        }

        public QuestionnaireDocument Get(string id, long version)
        {
            return this.documentStorage.GetById(id + "$" + version);
        }
    }
}