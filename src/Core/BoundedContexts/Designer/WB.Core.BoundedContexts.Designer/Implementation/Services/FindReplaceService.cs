using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
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

        public IEnumerable<QuestionnaireNodeReference> FindAll(Guid questionnaireId, string searchFor, bool matchCase, bool matchWholeWord)
        {
            var questionnaire = this.questionnaires.Get(questionnaireId);
            questionnaire.QuestionnaireDocument.ConnectChildrenWithParent();
            return questionnaire.FindAllTexts(searchFor, matchCase, matchWholeWord);
        }
    }
}