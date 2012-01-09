using System;
using System.Collections.Generic;
using System.IO;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

namespace RavenQuestionnaire.Core.Export
{
    public class ExportManager
    {
        private IExportProvider _provider;

        public ExportManager( IExportProvider provider)
        {
            _provider = provider;
        }

        public bool Export(Dictionary<Guid, string> template, CompleteQuestionnaireExportView items, string fileName)
        {
            _provider.DoExport(template, items, fileName);
            return true;
        }

        public Stream ExportToStream(Dictionary<Guid, string> template, CompleteQuestionnaireExportView items)
        {
            return _provider.DoExportToStream(template, items);
        }

    }
}
