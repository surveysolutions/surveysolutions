using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation
{
    internal class VersionedQuestionnaireReader : IVersionedQuestionnaireReader
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> documentStorage;

        public VersionedQuestionnaireReader(IReadSideRepositoryReader<QuestionnaireDocument> documentStorage)
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