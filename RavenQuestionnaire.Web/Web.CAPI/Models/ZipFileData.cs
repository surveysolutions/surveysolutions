using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Event;

namespace Web.CAPI.Models
{
    public class ZipFileData
    {
        public Guid ClientGuid { get; set; }

        public List<EventBrowseItem> Events { get; set; }

        public DateTime ImportDate { get; set; }

        public ZipFileData()
        {
            ImportDate = DateTime.Now;
        }
    }
}