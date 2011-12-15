using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class QuestionnaireRepository : EntityRepository<Questionnaire, QuestionnaireDocument>, IQuestionnaireRepository
    {
        public QuestionnaireRepository(IDocumentSession documentSession) : base(documentSession) { }

        protected override Questionnaire Create(QuestionnaireDocument doc)
        {
            return new Questionnaire(doc);
        }
    }
}
