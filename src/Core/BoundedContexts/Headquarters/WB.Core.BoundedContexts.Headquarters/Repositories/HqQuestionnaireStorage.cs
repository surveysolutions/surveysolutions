﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.ReusableCategories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public class HqQuestionnaireStorage : QuestionnaireStorage
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireCompositeItem, int> questionnaireItemsWriter;
        private readonly INativeReadSideStorage<QuestionnaireCompositeItem, int> questionnaireItemsReader;
        private readonly IReusableCategoriesFillerIntoQuestionnaire categoriesFillerIntoQuestionnaire;

        public HqQuestionnaireStorage(IPlainKeyValueStorage<QuestionnaireDocument> repository,
            ITranslationStorage translationStorage,
            IQuestionnaireTranslator translator,
            IReadSideRepositoryWriter<QuestionnaireCompositeItem, int> questionnaireItemsWriter,
            INativeReadSideStorage<QuestionnaireCompositeItem, int> questionnaireItemsReader,
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService,
            IInterviewExpressionStatePrototypeProvider expressionStatePrototypeProvider,
            IReusableCategoriesFillerIntoQuestionnaire categoriesFillerIntoQuestionnaire)
            : base(repository, translationStorage, translator, questionOptionsRepository, substitutionService, expressionStatePrototypeProvider)
        {
            this.questionnaireItemsWriter = questionnaireItemsWriter;
            this.questionnaireItemsReader = questionnaireItemsReader;
            this.categoriesFillerIntoQuestionnaire = categoriesFillerIntoQuestionnaire;
        }

        public override void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            if (!questionnaireDocument.IsDeleted)
            {
                ExtractQuestionnaireEntities(version, questionnaireDocument);
            }

            base.StoreQuestionnaire(id, version, questionnaireDocument);
        }

        private void ExtractQuestionnaireEntities(long version, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, version).ToString();
            questionnaireDocument.EntitiesIdMap = new Dictionary<Guid, int>();

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

                if (question is AbstractQuestion abstractQuestion)
                {
                    compositeItem.LinkedToQuestionId = abstractQuestion.LinkedToQuestionId;
                    compositeItem.LinkedToRosterId = abstractQuestion.LinkedToRosterId;
                    compositeItem.CascadeFromQuestionId = abstractQuestion.CascadeFromQuestionId;
                    compositeItem.IsFilteredCombobox = abstractQuestion.IsFilteredCombobox ?? false;
                    compositeItem.QuestionText = abstractQuestion.QuestionText;
                    compositeItem.VariableLabel = abstractQuestion.VariableLabel;
                    compositeItem.StatExportCaption = abstractQuestion.StataExportCaption;
                }

                questionnaireItemsWriter.Store(compositeItem);
                questionnaireDocument.EntitiesIdMap.Add(compositeItem.EntityId, compositeItem.Id);
            }
        }

        public override QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);
            string repositoryId = GetRepositoryId(questionnaireIdentity);

            return questionnaireDocumentsCache.GetOrAdd(repositoryId, key =>
            {
                var questionnaire = this.repository.GetById(key);

                if (questionnaire == null)
                {
                    return null;
                }

                var entities = this.questionnaireItemsReader.Query(_ => _.Where(x => x.QuestionnaireIdentity == questionnaireIdentity.Id)
                    .Select(x => new { x.Id, x.EntityId }).ToList());

                var entitiesMap = entities.ToDictionary(q => q.EntityId, q => q.Id);
                questionnaire.EntitiesIdMap = entitiesMap;

                return questionnaire;
            });
        }

        protected override QuestionnaireDocument FillPlainQuestionnaireDataOnCreate(QuestionnaireIdentity identity, QuestionnaireDocument questionnaireDocument)
        {
            return categoriesFillerIntoQuestionnaire.FillCategoriesIntoQuestionnaireDocument(identity, questionnaireDocument);
        }
    }
}
