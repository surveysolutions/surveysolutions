using System;
using System.Collections.Generic;
using System.IO;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

namespace RavenQuestionnaire.Core.Export
{
    public interface IExportProvider
    {
        bool DoExport(Dictionary<Guid, string> template, CompleteQuestionnaireExportView items, string fileName);
        Stream DoExportToStream(Dictionary<Guid, string> template, CompleteQuestionnaireExportView items);
    }
}
