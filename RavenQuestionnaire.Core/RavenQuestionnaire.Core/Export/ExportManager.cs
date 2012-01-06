using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Export
{
    public class ExportManager
    {
        private IExportProvider _provider;

        public ExportManager( IExportProvider provider)
        {
            _provider = provider;
        }

        public bool Export(Dictionary<Guid, string> template, CompleteQuestionnaireBrowseView items, string fileName)
        {
            _provider.DoExport(template, items, fileName);
            return true;
        }


    }
}
