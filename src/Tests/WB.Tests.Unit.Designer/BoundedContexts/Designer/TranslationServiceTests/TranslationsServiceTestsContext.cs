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
        protected const int translationTypeColumn = 2;
        protected const int translationIndexColumn = 3;
        protected const int questionnaireEntityIdColumn = 1;
        protected const int originalTextColumn = 4;
        protected const int translactionColumn = 5;
    }
}