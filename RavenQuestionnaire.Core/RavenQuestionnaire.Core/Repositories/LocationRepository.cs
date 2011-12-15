using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class LocationRepository: EntityRepository<Location, LocationDocument>, ILocationRepository
    {
        public LocationRepository(IDocumentSession documentSession) : base(documentSession) { }


        protected override Location Create(LocationDocument doc)
        {
            return new Location(doc);
        }
    }
}
