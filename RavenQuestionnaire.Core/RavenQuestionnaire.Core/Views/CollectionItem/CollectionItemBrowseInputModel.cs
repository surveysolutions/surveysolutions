using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseInputModel
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
        
        public int TotalCount { get; private set; }

        public string CollectionId { get; set; }

        public CollectionItemBrowseInputModel(string collectionId)
        {
            this.CollectionId = IdUtil.CreateQuestionnaireId(collectionId);
        }
    }
}
