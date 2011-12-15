using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Location
{
    public class LocationBrowseItem
    {
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            set { _id = value; }
        }

        private string _id;
        public string Location
        {
            get;
            set;
        }

        public int UserCount { get; private set; }
        /*  public DateTime CreationDate
        {
            get;
            private set;
        }*/
        public LocationBrowseItem()
        {
        }

        public LocationBrowseItem(string id, string location, int userCount)
        {
            this.Id = id;
            this.Location = location;
            this.UserCount = userCount;
        }

        public static  LocationBrowseItem New()
        {
            return  new LocationBrowseItem();
        }
    }
}
