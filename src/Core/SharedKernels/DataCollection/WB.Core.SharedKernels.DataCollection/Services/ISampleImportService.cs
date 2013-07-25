using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface ISampleImportService
    {
        IEnumerable<CompleteQuestionnaireDocument> GetSampleList(Guid templateId, TextReader textReader);
    }
}