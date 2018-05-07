using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public class HqQuestionnaireStorage : QuestionnaireStorage
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireCompositeItem, int> questionnaireItemsWriter;

        public HqQuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository,
            ITranslationStorage translationStorage,
            IQuestionnaireTranslator translator,
            IReadSideRepositoryWriter<QuestionnaireCompositeItem, int> questionnaireItemsWriter)
            : base(repository, translationStorage, translator)
        {
            this.questionnaireItemsWriter = questionnaireItemsWriter;
        }

        public override void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            base.StoreQuestionnaire(id, version, questionnaireDocument);

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, version).ToString();

            foreach (var composite in questionnaireDocument.Children.TreeToEnumerable(d => d.Children))
            {
                var question = composite as IQuestion;

                var compositeItem = new QuestionnaireCompositeItem
                {
                    EntityId = composite.PublicKey,
                    ParentId = composite.GetParent()?.PublicKey,
                    QuestionType = question?.QuestionType,
                    QuestionnaireIdentity = questionnaireIdentity,
                    Featured = question?.Featured,
                    QuestionScope = question?.QuestionScope,
                    EntityType = composite.GetEntityType()
                };

                questionnaireItemsWriter.Store(compositeItem);
            }
        }
    }
}
