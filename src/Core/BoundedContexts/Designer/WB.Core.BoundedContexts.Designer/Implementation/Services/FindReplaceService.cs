using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class FindReplaceService : IFindReplaceService
    {
        private readonly IPlainAggregateRootRepository<Questionnaire> questionnaires;

        internal FindReplaceService(IPlainAggregateRootRepository<Questionnaire> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public IEnumerable<string> FindAll(Guid questionnaireId, string searchFor)
        {
            //var questionnaire = this.questionnaires.Get(questionnaireId);

            return Enumerable.Empty<string>();
        }

        public void ReplaceAll(Guid questionnaireId, string searchFor, string replaceWith)
        {
            throw new System.NotImplementedException();
        }
    }
}