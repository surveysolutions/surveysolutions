using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Collection
{
    public class CollectionBrowseInputModel
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
    }
}
