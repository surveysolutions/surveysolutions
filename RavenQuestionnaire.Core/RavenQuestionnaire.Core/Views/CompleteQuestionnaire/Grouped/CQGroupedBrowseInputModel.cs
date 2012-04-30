using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupedBrowseInputModel
    {

        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        private int _page = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        private int _pageSize = 20;


        public string TemplateQuestionanireId
        {
            get { return _templateQuestionanireId; }
            set { _templateQuestionanireId = IdUtil.CreateQuestionnaireId(value); }
        }
        private string _templateQuestionanireId;
    }
}
