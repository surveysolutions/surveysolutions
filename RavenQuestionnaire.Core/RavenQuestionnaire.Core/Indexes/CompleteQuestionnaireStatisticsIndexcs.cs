using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Indexes
{
    public class CompleteQuestionnaireStatisticsIndexcs : AbstractIndexCreationTask<CompleteQuestionnaireDocument, CompleteQuestionnaireDocument>
    {
        public CompleteQuestionnaireStatisticsIndexcs()
        {
        
        }
    }
}
