using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Events;

namespace Questionnaire.Core.Web.Export
{
    public class ZipFileData
    {
        public Guid ClientGuid { get; set; }

        public IEnumerable<AggregateRootEventStream> Events { get; set; }

        public DateTime ImportDate { get; set; }

        public ZipFileData()
        {
            ImportDate = DateTime.Now;
        }
    }
}