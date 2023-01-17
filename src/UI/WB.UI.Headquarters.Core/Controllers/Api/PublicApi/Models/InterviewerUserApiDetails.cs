using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewerUserApiDetails : UserApiDetails
    {
        public InterviewerUserApiDetails(UserView userView)
            : base(userView)
        {
            this.IsLockedByHeadquarters = userView.IsLockedByHQ;
            this.IsLockedBySupervisor = userView.IsLockedBySupervisor;
            this.IsRelinkAllowed = userView.IsRelinkAllowed;
            this.SupervisorId = userView.Supervisor.Id;
            this.SupervisorName = userView.Supervisor.Name;
        }

        [DataMember]
        [Required]
        public string SupervisorName { get; }

        [DataMember]
        [Required]
        public Guid SupervisorId { get; }

        [DataMember]
        [Required]
        public bool IsLockedBySupervisor { get; }

        [DataMember]
        [Required]
        public bool IsLockedByHeadquarters { get; }
        
        [DataMember]
        [Required]
        public bool IsRelinkAllowed { get; }
    }
}