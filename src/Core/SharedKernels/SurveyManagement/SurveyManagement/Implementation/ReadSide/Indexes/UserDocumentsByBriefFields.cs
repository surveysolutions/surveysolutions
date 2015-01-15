using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Raven.Client.Indexes;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes
{
    public class UserDocumentsByBriefFields : AbstractIndexCreationTask<UserDocument, UserDocumentsByBriefFields.UserDocumentBrief>
    {
        public class UserDocumentBrief
        {
            // ReSharper disable once InconsistentNaming
            public Guid Supervisor_Id { get; set; }
            public Guid PublicKey { get; set; }
            public string UserName { get; set; }
            public List<UserRoles> Roles { get; set; }
        }

        public UserDocumentsByBriefFields()
        {
            this.Map = interviews => from doc in interviews
                select new UserDocumentBrief { Supervisor_Id = doc.Supervisor.Id, PublicKey = doc.PublicKey, Roles = doc.Roles, UserName = doc.UserName };
        }
    }
}