using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class CQStatusReportView
    {
        public string QuestionnaireId { get; set; }
        public string Title { get; set; }
        public List<CQStatusReportItemView> Items { get; set; }
        public string StatusName { get; set; }

        public CQStatusReportView(StatusItem status, IEnumerable<CompleteQuestionnaireBrowseItem> statisticDocuments)
        {
            StatusName = status.Title;
            
            Items = new List<CQStatusReportItemView>();
            foreach (var statisticDocument in statisticDocuments)
            {
                var view = new CQStatusReportItemView()
                               {
                                   AssignToUser = statisticDocument.Responsible,
                                   LastChangeDate = statisticDocument.LastEntryDate,
                                   LastSyncDate = DateTime.Now,
                                   Description = string.Join(", ", statisticDocument.FeaturedQuestions.Select(f => f.QuestionText + ": " + f.AnswerText))
                               };
                Items.Add(view);
            }
        }
    }
}
