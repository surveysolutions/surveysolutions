using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes
{
    public class UserDocumentsByBriefFields : AbstractIndexCreationTask<UserDocument, UserDocumentBrief>
    {
        public UserDocumentsByBriefFields()
        {
            this.Map = users => from doc in users
                                select new UserDocumentBrief
                                {
                                    Supervisor_Id = doc.Supervisor.Id,
                                    PublicKey = doc.PublicKey,
                                    Roles = doc.Roles,
                                    Password = doc.Password,
                                    UserName = doc.UserName
                                };

            Store(x => x.Roles, FieldStorage.Yes);
            Index(x => x.Roles, FieldIndexing.Analyzed);
        }
    }

    public class UserDocumentBrief
    {
        // ReSharper disable once InconsistentNaming
        public Guid Supervisor_Id { get; set; }
        public Guid PublicKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<UserRoles> Roles { get; set; }
    }
}