using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public class UserDocument
    {
        public UserDocument()
        {
            CreationDate = DateTime.Now;
            Roles= new List<UserRoles>();
            Location= new LocationDocument();
        }

        public string Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public DateTime CreationDate { get; set; }

        public List<UserRoles> Roles { get; set; }

        public bool IsLocked { get; set; }

        public bool IsDeleted { get; set; }

        public UserLight Supervisor { get; set; }

        public LocationDocument Location { get; set; }
    }
}
