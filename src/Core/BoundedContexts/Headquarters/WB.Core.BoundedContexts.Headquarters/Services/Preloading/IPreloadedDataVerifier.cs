using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        IEnumerable<PanelImportVerificationError> VerifyAnswers(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file);
        IEnumerable<PanelImportVerificationError> VerifyColumnsAndRosters(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file);
    }
}
