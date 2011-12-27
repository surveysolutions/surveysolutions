using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusBrowseItem
    {
        private string _id;
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            set { _id = value; }
        }

        public DateTime CreationDate { set; get; }

        public string Title { get; set; }

        public bool IsInitial { get; set; }
        public bool IsVisible { get; set; }

        public StatusBrowseItem()
        {
            IsInitial = false;
        }

        public StatusBrowseItem (string id, string title)
        {
            this.Title = title;
            this.Id = id;

        }

        public static StatusBrowseItem New()
        {
            return new StatusBrowseItem(null, null);
        }

    }
}
