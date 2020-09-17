using System.Linq;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.UI.Headquarters.Services.Impl
{
    public class HqWebInterviewInterviewEntityFactory : WebInterviewInterviewEntityFactory
    {
        private readonly IAuthorizedUser authorizedUser;

        public HqWebInterviewInterviewEntityFactory(IMapper autoMapper,
            IAuthorizedUser authorizedUser,
            IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy,
            ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy,
            IWebNavigationService webNavigationService,
            ISubstitutionTextFactory substitutionTextFactory) : base(autoMapper,
            enumeratorGroupStateCalculationStrategy, supervisorGroupStateCalculationStrategy, webNavigationService,
            substitutionTextFactory)
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
                                     ((this.authorizedUser.IsSupervisor || this.authorizedUser.IsHeadquarter) && !this.authorizedUser.IsObserving);
            result.AcceptAnswer = result.IsForSupervisor;

            if (callerInterview.Status < InterviewStatus.ApprovedByHeadquarters
                && question.AnswerComments.Any(x => !x.Resolved && x.Id.HasValue))
            {
                if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
                {
                    result.AllowResolveComments = true;
                }
                else if (this.authorizedUser.IsSupervisor)
                {
                    result.AllowResolveComments = 
                        question.AnswerComments
                            .Where(x => !x.Resolved)
                            .All(x => x.UserRole == UserRoles.Supervisor || x.UserRole == UserRoles.Interviewer);
                }
            }
        }

        protected override Comment[] GetComments(InterviewTreeQuestion question)
        {
            var result = question.AnswerComments.Select(
                    ac =>
                    {
                        var comment = new Comment
                        {
                            Text = ac.Comment,
                            IsOwnComment = ac.UserId == this.authorizedUser.Id,
                            UserRole = ac.UserRole,
                            CommentTimeUtc = ac.CommentTime.UtcDateTime,
                            Id = ac.Id,
                            Resolved = ac.Resolved,
                            CommentOnPreviousAnswer = ac.CommentOnPreviousAnswer
                        };

                        return comment;
                    })
                .ToArray();
            return result;
        }

        protected override string WebLinksVirtualDirectory(bool isReview) => isReview ? @"Interview/Review" : @"WebInterview";
    }
}
