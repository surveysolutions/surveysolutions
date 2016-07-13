using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations.Impl
{
    internal class TranslationManagementService : ITranslationManagementService
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public TranslationManagementService(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        public IList<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId, string language)
        {
            var translationInstances =
                this.translations.Query(_ =>
                    _.Where(x =>
                        x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                        x.QuestionnaireId.Version == questionnaireId.Version &&
                        x.Language == language).ToList());
            return translationInstances;
        }

        public IList<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId)
        {
            var translationInstances =
                this.translations.Query(_ =>
                    _.Where(x =>
                        x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                        x.QuestionnaireId.Version == questionnaireId.Version).ToList());
            return translationInstances;
        }

        public void Delete(QuestionnaireIdentity questionnaireId)
        {
            var translationInstances = this.translations.Query(_ => _.Where(x =>
                x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                x.QuestionnaireId.Version == questionnaireId.Version).ToList());

            this.translations.Remove(translationInstances);
        }
    }
}