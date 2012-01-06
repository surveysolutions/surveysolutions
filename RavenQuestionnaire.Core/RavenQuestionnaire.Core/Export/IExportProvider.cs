using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Export
{
    public interface IExportProvider
    {
        bool DoExport(Dictionary<Guid, string> template, CompleteQuestionnaireBrowseView items, string fileName);
    }
}
