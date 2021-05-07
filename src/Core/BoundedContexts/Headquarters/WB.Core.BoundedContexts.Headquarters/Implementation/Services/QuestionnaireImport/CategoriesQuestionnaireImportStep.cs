using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class CategoriesQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly QuestionnaireDocument questionnaire;
        private readonly IDesignerApi designerApi;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly ILogger logger;
        private readonly Dictionary<Categories, List<CategoriesItem>> categories = new Dictionary<Categories, List<CategoriesItem>>();

        public CategoriesQuestionnaireImportStep(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire, IDesignerApi designerApi, IReusableCategoriesStorage reusableCategoriesStorage, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.questionnaire = questionnaire;
            this.designerApi = designerApi;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.logger = logger;
        }
        public bool IsNeedProcessing()
        {
            return questionnaire.Categories?.Count > 0;
        }

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            if (questionnaire.Categories != null && questionnaire.Categories.Count > 0)
            {
                int percentPerCategory = 100 / questionnaire.Categories.Count;
                int currentPercent = 0;

                foreach (var category in questionnaire.Categories)
                {
                    this.logger.Debug($"Loading reusable category for questionnaire {questionnaireIdentity}. Category id {category.Id}");
                    var reusableCategories = await designerApi.GetReusableCategories(questionnaire.PublicKey, category.Id);
                    categories.Add(category, reusableCategories);

                    currentPercent += percentPerCategory;
                    progress.Report(currentPercent);
                }
            }

            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            foreach (var category in categories)
            {
                this.logger.Debug($"Save reusable category for questionnaire {questionnaireIdentity}. Category id {category.Key.Id}");
                reusableCategoriesStorage.Store(questionnaireIdentity, category.Key.Id, category.Value);
            }
            progress.Report(100);
        }
    }
}
