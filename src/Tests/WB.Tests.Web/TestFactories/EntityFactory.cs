using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using ReflectionMagic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.Tests.Web.TestFactories
{
    public class EntityFactory
    {
        public Identity Identity(string id, RosterVector rosterVector)
            => Create.Entity.Identity(Guid.Parse(id), rosterVector);

        public Identity Identity(Guid? id = null, RosterVector rosterVector = null)
            => new Identity(
                id ?? Guid.NewGuid(),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty);

        public Identity Identity(int id, RosterVector rosterVector = null)
            => new Identity(
                Guid.Parse(this.SpamIntToStringOfLength(id)),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty);

        private string SpamIntToStringOfLength(int number, int length = 32)
        {
            var result = number.ToString();
            return string.Concat(Enumerable.Repeat(result, (int)Math.Ceiling(length / (double)result.Length))).Substring(0, 32);
        }

        public RosterVector RosterVector(params decimal[] coordinates)
            => new RosterVector(coordinates ?? Enumerable.Empty<decimal>());

        public InterviewSummary InterviewSummary()
            => new InterviewSummary();


        public InterviewSummary InterviewSummary(
            Guid? interviewId = null,
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            InterviewStatus? status = null,
            Guid? responsibleId = null,
            Guid? teamLeadId = null,
            string responsibleName = null,
            string teamLeadName = null,
            UserRoles role = UserRoles.Interviewer,
            string key = null,
            DateTime? updateDate = null,
            bool? wasCreatedOnClient = null,
            DateTime? receivedByInterviewerAtUtc = null,
            int? assignmentId = null,
            bool wasCompleted = false,
            int? errorsCount = 0,
            TimeSpan? interviewingTotalTime = null,
            IEnumerable<InterviewCommentedStatus> statuses = null,
            IEnumerable<TimeSpanBetweenStatuses> timeSpans = null)
        {
            var qId = questionnaireId ?? Guid.NewGuid();
            var qVersion = questionnaireVersion ?? 1;
            return new InterviewSummary
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                QuestionnaireId = qId,
                QuestionnaireVersion = qVersion,
                Status = status.GetValueOrDefault(),
                ResponsibleId = responsibleId.GetValueOrDefault(),
                ResponsibleName = string.IsNullOrWhiteSpace(responsibleName) ? responsibleId?.ToString("N") : responsibleName,
                SupervisorId = teamLeadId.GetValueOrDefault(),
                SupervisorName = string.IsNullOrWhiteSpace(teamLeadName) ? teamLeadId?.ToString("N") : teamLeadName,
                ResponsibleRole = role,
                Key = key,
                UpdateDate = updateDate ?? new DateTime(2017, 3, 23),
                WasCreatedOnClient = wasCreatedOnClient ?? false,
                ReceivedByInterviewerAtUtc = receivedByInterviewerAtUtc,
                AssignmentId = assignmentId,
                QuestionnaireIdentity = new QuestionnaireIdentity(qId, qVersion).ToString(),
                WasCompleted = wasCompleted,
                InterviewDuration = interviewingTotalTime,
                InterviewCommentedStatuses = statuses?.ToList() ?? new List<InterviewCommentedStatus>(),
                TimeSpansBetweenStatuses = timeSpans != null ?  timeSpans.ToHashSet() : new HashSet<TimeSpanBetweenStatuses>()
            };
        }

        public HqUser HqUser(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null,
            string userName = "name", bool isLockedByHQ = false, UserRoles role = UserRoles.Interviewer,
            string deviceId = null, string passwordHash = null, string passwordHashSha1 = null, string interviewerVersion = null,
            int? interviewerBuild = null,
            bool lockedBySupervisor = false)
        {
            var user = new HqUser
            {
                Id = userId ?? Guid.NewGuid(),
                IsArchived = isArchived ?? false,
                UserName = userName,
                IsLockedByHeadquaters = isLockedByHQ,
                IsLockedBySupervisor = lockedBySupervisor,
                Profile = new WorkspaceUserProfile(),
                PasswordHash = passwordHash,
                PasswordHashSha1 = passwordHashSha1
            };
            user.Profile.AsDynamic().SupervisorId = supervisorId;
            user.Profile.AsDynamic().DeviceId = deviceId;
            user.Profile.AsDynamic().DeviceAppBuildVersion = interviewerBuild;
            user.Profile.AsDynamic().DeviceAppVersion = interviewerVersion;

            user.Roles.Add(new HqRole { Id = role.ToUserId() });

            return user;
        }

        public CompanyLogo HqCompanyLogo()
        {
            var hqCompanyLogo = new CompanyLogo();
            hqCompanyLogo.Logo = new byte[]{1, 2,12, 45, 15};
            return hqCompanyLogo;
        }
    }
}
