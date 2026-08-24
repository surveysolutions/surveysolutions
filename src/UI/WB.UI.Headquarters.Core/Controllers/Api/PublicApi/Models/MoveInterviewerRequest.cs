using System;
using System.ComponentModel.DataAnnotations;
using WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class MoveInterviewerRequest
    {
        /// <summary>
        /// Id of the new supervisor
        /// </summary>
        [Required]
        public Guid SupervisorId { get; set; }

        /// <summary>
        /// Determines how existing interviews and assignments are handled when moving the interviewer.
        /// ReassignToOriginalSupervisor (1) - reassign all data to the original supervisor.
        /// MoveAllToNewTeam (2) - move all data to the new supervisor's team.
        /// </summary>
        [Required]
        public MoveUserToAnotherTeamMode Mode { get; set; }
    }
}
