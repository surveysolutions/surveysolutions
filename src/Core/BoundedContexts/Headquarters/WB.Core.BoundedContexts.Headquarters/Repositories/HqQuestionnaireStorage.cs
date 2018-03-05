using System;
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

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public class HqQuestionnaireStorage : QuestionnaireStorage
    {
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;

        public HqQuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository,
                ITranslationStorage translationStorage,
                IQuestionnaireTranslator translator,
                IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems)
            : base(repository, translationStorage, translator)
        {
            this.questionnaireItems = questionnaireItems;
        }

        public override void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            base.StoreQuestionnaire(id, version, questionnaireDocument);

            foreach (var composite in questionnaireDocument.Children.TreeToEnumerable(d => d.Children))
            {
                var question = composite as IQuestion;

                questionnaireItems.Store(new QuestionnaireCompositeItem
                {
                    EntityId = composite.PublicKey,
                    ParentId = composite.GetParent()?.PublicKey,
                    QuestionType = question?.QuestionType,
                    QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, version).ToString(),
                    Featured = question?.Featured,
                    QuestionScope = question?.QuestionScope,
                    EntityType = composite.GetEntityType()
                }, composite.PublicKey);
            }
        }
    }
}
