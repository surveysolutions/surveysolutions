using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class TranslationInstance : TranslationDto, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string QuestionnaireId { get; set; }
    }
}
