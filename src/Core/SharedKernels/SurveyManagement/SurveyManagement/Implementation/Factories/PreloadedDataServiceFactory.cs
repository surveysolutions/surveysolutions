using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class PreloadedDataServiceFactory : IPreloadedDataServiceFactory
    {
        private readonly IQuestionDataParser dataParser;
        public PreloadedDataServiceFactory(IQuestionDataParser dataParser)
        {
            this.dataParser = dataParser;
        }

        public IPreloadedDataService CreatePreloadedDataService(QuestionnaireExportStructure exportStructure,
            QuestionnaireRosterStructure questionnaireRosterStructure, QuestionnaireDocument questionnaireDocument)
        {
            return new PreloadedDataService(exportStructure, questionnaireRosterStructure, questionnaireDocument, dataParser);
        }
    }
}
