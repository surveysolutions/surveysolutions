using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Indexes;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupedBrowseFactory : IViewFactory<CQGroupedBrowseInputModel, CQGroupedBrowseView>
    {
         private IDocumentSession documentSession;

         public CQGroupedBrowseFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
        #region Implementation of IViewFactory<CQGroupedBrowseInputModel,CQGroupedBrowseView>

        public CQGroupedBrowseView Load(CQGroupedBrowseInputModel input)
        {
            var query = documentSession.Query<CQGroupItem, QuestionnaireGroupedByTemplateIndex>().Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize).ToArray();
            foreach (CQGroupItem cqGroupItem in query)
            {
                var templateId = IdUtil.CreateQuestionnaireId(cqGroupItem.Id);
                var items = documentSession.Query<CompleteQuestionnaireStatisticDocument>().Where(x => x.TemplateId == templateId).ToList();
                cqGroupItem.Items = items.Select(x => new CompleteQuestionnaireBrowseItem(x));
                cqGroupItem.TotalCount = cqGroupItem.Items.Count();
            }
        
            return new CQGroupedBrowseView(input.Page, input.PageSize, 0, query);
        }

        #endregion
    }
}
