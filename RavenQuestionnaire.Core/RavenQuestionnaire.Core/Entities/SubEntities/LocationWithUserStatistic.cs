using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class LocationWithUserStatistic
    {
        public string Id { get; set; }

        public string Location
        { get; set; }

        public int UserCount
        { get; set; }
    }
}
