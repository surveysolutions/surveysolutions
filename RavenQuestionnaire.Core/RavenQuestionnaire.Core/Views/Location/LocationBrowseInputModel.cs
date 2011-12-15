using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.Location
{
    public class LocationBrowseInputModel
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
        private int _pageSize = 5;
    }
}
