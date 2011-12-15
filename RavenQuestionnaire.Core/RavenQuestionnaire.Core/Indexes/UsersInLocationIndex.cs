using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Indexes
{
    public class UsersInLocationIndex : AbstractIndexCreationTask<UserDocument, LocationWithUserStatistic>
    {
        public UsersInLocationIndex()
        {
            Map = docs => from doc in docs
                          select new
                                     {
                                         Id = doc.Location.Id,
                                         UserCount = 1
                                     };


            Reduce = results => from result in results
                                group result by result.Id
                                into g
                                    select new 
                                           {
                                               Id = g.Key,
                                               UserCount = g.Sum(x => x.UserCount)
                                           };
           TransformResults =
                (database, users) => from user in users
                                         let alias = database.Load<LocationDocument>(user.Id)
                                     select new 
                                                    {
                                                        Id = user.Id,
                                                        Location = alias.Location,
                                                        UserCount = user.UserCount
                                                    };
        }
    }
}
