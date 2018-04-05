using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IPreloadedDataVerifier
    {
        IEnumerable<PanelImportVerificationError> VerifySimple(PreloadedFile file,
            QuestionnaireIdentity questionnaireIdentity);

        IEnumerable<PanelImportVerificationError> VerifyPanel(PreloadedFile[] allImportedFiles,
            QuestionnaireIdentity questionnaireIdentity);
    }
}
