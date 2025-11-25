using System;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public class QuestionnaireContextProvider(IDesignerQuestionnaireStorage questionnaireStorage, 
    IImportExportQuestionnaireMapper importExportQuestionnaireMapper) 
    : IQuestionnaireContextProvider
{
    public string GetQuestionnaireContext(Guid questionnaireId)
    {
        var questionnaireDocument = questionnaireStorage.Get(questionnaireId);
        if (questionnaireDocument == null)
            throw new ArgumentException($"Questionnaire with id {questionnaireId} not found");
        
        var questionnaire = importExportQuestionnaireMapper.Map(questionnaireDocument);
        var json = JsonConvert.SerializeObject(questionnaire);

        return $"Context for questionnaire: {json}";
    }
}
