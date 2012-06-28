using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Indexes;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupedBrowseFactory : IViewFactory<CQGroupedBrowseInputModel, CQGroupedBrowseView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;

        public CQGroupedBrowseFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }
        #region Implementation of IViewFactory<CQGroupedBrowseInputModel,CQGroupedBrowseView>

        public CQGroupedBrowseView Load(CQGroupedBrowseInputModel input)
        {
            var questionnaires = this.documentItemSession.Query();
            var templates = this.documentGroupSession.Query();

            var retval = new CQGroupedBrowseView(0, 100, 100, templates.ToList());
            foreach (CQGroupItem cqGroupItem in retval.Groups)
            {
                CQGroupItem item = cqGroupItem;
                var complete = questionnaires.Where(q => q.TemplateId == item.SurveyId).ToList();
                cqGroupItem.Items = complete;
            }
            return retval;
            /* if (view != null)
                return view;
            return new CQGroupedBrowseView(0, 0, 0, new List<CQGroupItem>(0));*/
            /*     var query = documentSession.Query<CQGroupItem, QuestionnaireGroupedByTemplateIndex>().Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();
            
            foreach (CQGroupItem cqGroupItem in query)
            {
                var templateId = IdUtil.CreateQuestionnaireId(cqGroupItem.Id);
                var items = documentSession.Query<CompleteQuestionnaireStatisticDocument>().Where(x => x.TemplateId == templateId).ToList();
                cqGroupItem.Items = items.Select(x => new CompleteQuestionnaireBrowseItem(x));
                cqGroupItem.TotalCount = cqGroupItem.Items.Count();
            }
        
            return new CQGroupedBrowseView(input.Page, input.PageSize, 0, query);*/
        }

        #endregion
    }
}
