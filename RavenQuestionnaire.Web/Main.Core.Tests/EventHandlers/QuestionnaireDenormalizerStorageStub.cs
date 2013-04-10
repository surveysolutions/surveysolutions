using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.DenormalizerStorage;

namespace Main.Core.Tests.EventHandlers
{
    public class QuestionnaireDenormalizerStorageStub : IDenormalizerStorage<QuestionnaireDocument>
    {
        public QuestionnaireDocument Document { get; set; }

        public int Count()
        {
            return 0;
        }

        public QuestionnaireDocument GetByGuid(Guid key)
        {
            return Document;
        }

        public IQueryable<QuestionnaireDocument> Query()
        {
            return null;
        }

        public void Remove(Guid key)
        {
        }

        public void Store(QuestionnaireDocument denormalizer, Guid key)
        {
        }
    }
}
