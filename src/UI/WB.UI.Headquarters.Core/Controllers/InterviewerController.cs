#nullable enable
using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewerController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserRepository userRepository;
        private readonly IInterviewerProfileFactory interviewerProfileFactory;

        public InterviewerController(IAuthorizedUser authorizedUser,
                            IUserRepository userRepository, 
                              IInterviewerProfileFactory interviewerProfileFactory)
        {
            this.authorizedUser = authorizedUser;
            this.userRepository = userRepository;
            this.interviewerProfileFactory = interviewerProfileFactory;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        [ActionName("Profile")]
        [ActivePage(MenuItem.Interviewers)]
        [ExtraHeaderPermissions(HeaderPermissionType.Google)]
        public async Task<ActionResult> InterviewerProfile(string? id)
        {
            if (id == null || !Guid.TryParse(id, out var userId))
            {
                userId = this.authorizedUser.Id;
            }

            var interviewer = await this.userRepository.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) 
                return NotFound();

            InterviewerProfileModel interviewerProfileModel = await interviewerProfileFactory.GetInterviewerProfileAsync(userId);

            if (interviewerProfileModel == null)
                return NotFound();

            return this.View(interviewerProfileModel);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        public async Task<ActionResult<InterviewerPoints>> InterviewerPoints(Guid? id)
        {
            var userId = id ?? this.authorizedUser.Id;
            var interviewer = await this.userRepository.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) 
                return NotFound();

            InterviewerPoints points = interviewerProfileFactory.GetInterviewerCheckInPoints(userId);

            if (points.CheckInPoints.Count == 0 && points.TargetLocations.Count == 0) 
                return this.NotFound("No points");

            return points;
        }

        
        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        [ActionName("TrafficUsage")]
        public async Task<ActionResult<InterviewerTrafficUsage>> InterviewerTrafficUsage(Guid? id)
        {
            var userId = id ?? this.authorizedUser.Id;
            var interviewer = await this.userRepository.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) return this.NotFound();

            InterviewerTrafficUsage trafficUsage = await interviewerProfileFactory.GetInterviewerTrafficUsageAsync(userId);

            if (trafficUsage == null)
            {
                return new InterviewerTrafficUsage();
            }
    
            return trafficUsage;
        }
    }
}
