using Machine.Specifications;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    [Subject(typeof(TranslationsService))]
    internal class TranslationsServiceTestsContext
    {
        protected const int translationTypeColumn = 1;
        protected const int translationIndexColumn = 2;
        protected const int questionnaireEntityIdColumn = 0;
        protected const int originalTextColumn = 3;
        protected const int translactionColumn = 4;

        protected static TranslationsService CreateTranslationsService(IPlainStorageAccessor<TranslationInstance> traslationsStorage = null,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage = null)
        {
            return new TranslationsService(traslationsStorage ?? new TestPlainStorage<TranslationInstance>(),
                questionnaireStorage ?? Stub<IReadSideKeyValueStorage<QuestionnaireDocument>>.Returning(Create.QuestionnaireDocument()));
        }
    }
}