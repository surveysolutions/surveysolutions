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
            PublicKey = Guid.NewGuid();
            Roles= new List<UserRoles>();
            Location= new LocationDocument();
        }

        public string Id { get; set; }

        public Guid PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                this.Id = value.ToString();
            }
        }

        private Guid publicKey;

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
