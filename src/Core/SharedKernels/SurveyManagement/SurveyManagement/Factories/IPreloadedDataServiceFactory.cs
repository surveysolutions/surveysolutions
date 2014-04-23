using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    internal interface IPreloadedDataServiceFactory
    {
        IPreloadedDataService CreatePreloadedDataService(QuestionnaireExportStructure exportStructure,
            QuestionnaireRosterStructure questionnaireRosterStructure, QuestionnaireDocument questionnaireDocument);
    }
}
