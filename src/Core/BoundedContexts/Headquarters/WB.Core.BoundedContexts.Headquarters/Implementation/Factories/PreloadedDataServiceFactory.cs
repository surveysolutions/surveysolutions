using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    internal class PreloadedDataServiceFactory : IPreloadedDataServiceFactory
    {
        private readonly IQuestionDataParser dataParser;
        private readonly IUserViewFactory userViewFactory;

        public PreloadedDataServiceFactory(IQuestionDataParser dataParser, IUserViewFactory userViewFactory)
        {
            this.dataParser = dataParser;
            this.userViewFactory = userViewFactory;
        }

        public IPreloadedDataService CreatePreloadedDataService(QuestionnaireExportStructure exportStructure,
            QuestionnaireRosterStructure questionnaireRosterStructure, QuestionnaireDocument questionnaireDocument)
        {
            return new PreloadedDataService(exportStructure, questionnaireRosterStructure, questionnaireDocument, this.dataParser, this.userViewFactory);
        }
    }
}
