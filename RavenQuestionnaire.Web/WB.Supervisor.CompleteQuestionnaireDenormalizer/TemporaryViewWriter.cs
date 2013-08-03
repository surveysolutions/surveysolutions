using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Supervisor.CompleteQuestionnaireDenormalizer
{
    internal class TemporaryViewWriter : IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>, IDisposable
    {
        private readonly Guid tempViewId;
        private readonly CompleteQuestionnaireDenormalizer denormalizer;
        private CompleteQuestionnaireStoreDocument tempView;

        public TemporaryViewWriter(CompleteQuestionnaireStoreDocument view, CompleteQuestionnaireDenormalizer denormalizer)
        {
            this.tempViewId = view.PublicKey;
            this.tempView = view;
            this.denormalizer = denormalizer;
            denormalizer.SetStorage(this);
        }

        public CompleteQuestionnaireStoreDocument GetById(Guid id)
        {
            if (id != tempViewId)
                return null;
            return tempView;
        }

        public void Remove(Guid id)
        {
            tempView = null;
        }

        public void Store(CompleteQuestionnaireStoreDocument view, Guid id)
        {
            if(id!=tempViewId)
                return;
            tempView = view;
        }

        public void Dispose()
        {
            denormalizer.ClearStorage();
            tempView = null;
        }
    }
}
