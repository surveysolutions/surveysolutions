using System.Linq;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class HqWebInterviewInterviewEntityFactory : WebInterviewInterviewEntityFactory
    {
        private readonly IAuthorizedUser authorizedUser;

        public HqWebInterviewInterviewEntityFactory(IMapper autoMapper, 
            IAuthorizedUser authorizedUser,
            IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy,
            ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy) : base(autoMapper, enumeratorGroupStateCalculationStrategy, supervisorGroupStateCalculationStrategy)
        {
            this.authorizedUser = authorizedUser;
        }

        protected override void ApplyReviewState(GenericQuestion result, InterviewTreeQuestion question, IStatefulInterview callerInterview, bool isReviewMode)
        {
            if (!isReviewMode)
            {
                result.AcceptAnswer = true;
                return;
            }

            result.IsForSupervisor = question.IsSupervisors &&
                                     callerInterview.Status < InterviewStatus.ApprovedByHeadquarters &&
                                     (this.authorizedUser.IsSupervisor && !this.authorizedUser.IsObserving);
            result.AcceptAnswer = result.IsForSupervisor;
        }

        protected override Comment[] GetComments(InterviewTreeQuestion question)
        {
            return question.AnswerComments.Select(
                    ac => new Comment
                    {
                        Text = ac.Comment,
                        IsOwnComment = ac.UserId == this.authorizedUser.Id,
                        UserRole = ac.UserRole,
                        CommentTimeUtc = ac.CommentTime
                    })
                .ToArray();
        }
    }
}
