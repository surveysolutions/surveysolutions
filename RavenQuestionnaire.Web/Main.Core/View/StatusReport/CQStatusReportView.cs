using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.View.CompleteQuestionnaire;

namespace Main.Core.View.StatusReport
{
    public class CQStatusReportView
    {
        #region Constructors and Destructors

        public CQStatusReportView(IEnumerable<CompleteQuestionnaireBrowseItem> statisticDocuments)
        {

            
        }

        #endregion

        #region Public Properties

        public List<CQStatusReportItemView> Items { get; set; }

        public string QuestionnaireId { get; set; }


        public string Title { get; set; }

        #endregion
    }
}