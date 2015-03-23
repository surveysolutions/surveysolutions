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
                                    UserName = doc.UserName,
                                    IsDeleted = doc.IsDeleted,
                                    CreationDate = doc.CreationDate,
                                    Email = doc.Email,
                                    IsLockedByHQ = doc.IsLockedByHQ,
                                    IsLockedBySupervisor = doc.IsLockedBySupervisor
                                };

            Sort(x => x.UserName, SortOptions.StringVal);

            Index(x => x.UserName, FieldIndexing.Analyzed);
            Index(x => x.Email, FieldIndexing.Analyzed);

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
        public bool IsDeleted { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }

        public bool IsLockedBySupervisor { get; set; }
        public bool IsLockedByHQ { get; set; }
    }
}