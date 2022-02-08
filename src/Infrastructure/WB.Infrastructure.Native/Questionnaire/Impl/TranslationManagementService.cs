using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Enumerator.Native.Questionnaire.Impl
{
    public class TranslationManagementService : ITranslationManagementService
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public TranslationManagementService(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        public List<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId, Guid translationId)
        {
            var translationInstances =
                this.translations.Query(_ =>
                    _.Where(x =>
                        x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                        x.QuestionnaireId.Version == questionnaireId.Version &&
                        x.TranslationId == translationId).ToList());
            return translationInstances;
        }

        public List<TranslationInstance> GetAll(QuestionnaireIdentity questionnaireId)
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
                x.QuestionnaireId.Version == questionnaireId.Version).ToList())
                .ToList();

            this.translations.Remove(translationInstances);
        }

        public void Store(IEnumerable<TranslationInstance> translationInstances)
        {
            var enumerable = translationInstances.Select(x => Tuple.Create(x, (object)x));
            this.translations.Store(enumerable);
        }
    }
}
