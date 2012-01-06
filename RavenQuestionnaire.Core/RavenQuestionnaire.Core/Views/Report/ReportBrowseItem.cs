using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportBrowseItem
    {
        private string _id;
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            set { _id = value; }
        }

        public DateTime CreationDate { set; get; }

        public string Title { get; set; }

        public string Description { get; set; }

        
        public ReportBrowseItem(string id, string title, string description)
        {
            this.Title = title;
            this.Id = id;
            this.Description = description;
        }

        public static ReportBrowseItem New()
        {
            return new ReportBrowseItem(null, null, null);
        }

    }
}
