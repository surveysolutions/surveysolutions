using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
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
            return new PreloadedDataService(exportStructure, questionnaireRosterStructure, questionnaireDocument, dataParser, userViewFactory);
        }
    }
}
