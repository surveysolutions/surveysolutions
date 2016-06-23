using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IAllInterviewsFactory
    {
        AllInterviewsView Load(AllInterviewsInputModel input);
    }

    public class AllInterviewsFactory : IAllInterviewsFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IQueryableReadSideRepositoryReader<QuestionAnswer> answersReader;

        public AllInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader,
            IQueryableReadSideRepositoryReader<QuestionAnswer> answersReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
            this.answersReader = answersReader;
        }

        public AllInterviewsView Load(AllInterviewsInputModel input)
        {
            var interviews = this.interviewSummaryReader.Query(_ =>
            {
                var items = ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                return items.Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
            });

            var totalCount = this.interviewSummaryReader.Query(_ => ApplyFilter(input, _).Count());
            var requiredInterviews = interviews.Select(y => y.SummaryId).ToList();
            var featuredQuestionAnswers = this.answersReader.Query(_ => _.Where(x => requiredInterviews.Contains(x.InterviewSummary.SummaryId)).ToList());

            var result = new AllInterviewsView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = interviews.Select(x => ToAllInterviewsViewItem(x, featuredQuestionAnswers)).ToList()
            };

            return result;
        }

        private static AllInterviewsViewItem ToAllInterviewsViewItem(InterviewSummary interviewSummary, IReadOnlyCollection<QuestionAnswer> allFeaturedQuestionAnswers)
        {
            var interviewFeaturedQuestionAnswers = allFeaturedQuestionAnswers
                .Where(f => f.InterviewSummary.SummaryId == interviewSummary.SummaryId)
                .Select(a => new InterviewFeaturedQuestion
                {
                    Id = a.Questionid,
                    Answer = a.Answer,
                    Question = a.Title,
                    Type = a.Type
                })
                .GroupBy(question => question.Id)
                .Select(grouping => grouping.First())
                .OrderBy(question => question.Id)
                .ToList();

            return new AllInterviewsViewItem
            {
                FeaturedQuestions = interviewFeaturedQuestionAnswers,
                InterviewId = interviewSummary.InterviewId,
                LastEntryDate = interviewSummary.UpdateDate.ToShortDateString(),
                ResponsibleId = interviewSummary.ResponsibleId,
                ResponsibleName = interviewSummary.ResponsibleName,
                ResponsibleRole = interviewSummary.ResponsibleRole,
                HasErrors = interviewSummary.HasErrors,
                Status = interviewSummary.Status.ToString(),
                CanDelete =    interviewSummary.Status == InterviewStatus.Created
                               || interviewSummary.Status == InterviewStatus.SupervisorAssigned
                               || interviewSummary.Status == InterviewStatus.InterviewerAssigned
                               || interviewSummary.Status == InterviewStatus.SentToCapi,
                CanApproveOrReject = interviewSummary.Status == InterviewStatus.ApprovedBySupervisor,
                CanUnapprove = interviewSummary.Status == InterviewStatus.ApprovedByHeadquarters,
                QuestionnaireId = interviewSummary.QuestionnaireId,
                QuestionnaireVersion = interviewSummary.QuestionnaireVersion,
                CreatedOnClient = interviewSummary.WasCreatedOnClient,
                ReceivedByInterviewer = interviewSummary.ReceivedByInterviewer
            };
        }

        private static IQueryable<InterviewSummary> ApplyFilter(AllInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.AnswersToFeaturedQuestions.Any(a => a.Answer.StartsWith(input.SearchBy)));
            }

            if (input.Status.HasValue)
            {
                items = items.Where(x => x.Status == input.Status);
            }

            if (!string.IsNullOrWhiteSpace(input.TeamLeadName))
            {
                items = items.Where(x => x.TeamLeadName == input.TeamLeadName);
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId == input.QuestionnaireId);
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireVersion == input.QuestionnaireVersion);
            }
            return items;
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query,
                                                        AllInterviewsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x=>x.UpdateDate);
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }
    }
}
