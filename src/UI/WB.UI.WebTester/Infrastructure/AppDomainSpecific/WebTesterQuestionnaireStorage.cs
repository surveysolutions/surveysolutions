﻿using System;
using System.Collections.Concurrent;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly IWebTesterTranslationService translationService;

        public WebTesterQuestionnaireStorage(IWebTesterTranslationService translationService, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService,
            IReusableCategoriesFillerIntoQuestionnaire categoriesFillerIntoQuestionnaire)
        {
            this.translationService = translationService;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
            this.categoriesFillerIntoQuestionnaire = categoriesFillerIntoQuestionnaire;
        }

        private readonly ConcurrentDictionary<string, PlainQuestionnaire> plainQuestionnairesCache 
            = new ConcurrentDictionary<string, PlainQuestionnaire>();

        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IReusableCategoriesFillerIntoQuestionnaire categoriesFillerIntoQuestionnaire;

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            if (this.plainQuestionnairesCache.TryGetValue(identity.ToString(), out PlainQuestionnaire q))
            {
                if (language == null)
                {
                    return q;
                }
            }

            return this.translationService.Translate(q, identity.Version, language);
        }
        
        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var questionnaireMergedWithCategories =
                this.categoriesFillerIntoQuestionnaire.FillCategoriesIntoQuestionnaireDocument(questionnaireIdentity,
                    questionnaireDocument);

            this.plainQuestionnairesCache[questionnaireIdentity.ToString()] = 
                 new PlainQuestionnaire(questionnaireMergedWithCategories, version, questionOptionsRepository, substitutionService);
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            if (this.plainQuestionnairesCache.TryGetValue(identity.ToString(), out PlainQuestionnaire q))
            {
                return q.QuestionnaireDocument;
            }

            return null;
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return this.GetQuestionnaireDocument(new QuestionnaireIdentity(id, version));
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            this.plainQuestionnairesCache.TryRemove(new QuestionnaireIdentity(id, version).ToString(), out _);
        }
    }
}
