using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class AllInterviewsFactory : IAllInterviewsFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;
        private readonly IQueryableReadSideRepositoryReader<QuestionAnswer> featuredQuestions;

        public AllInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> reader,
            IQueryableReadSideRepositoryReader<QuestionAnswer> featuredQuestions)
        {
            this.reader = reader;
            this.featuredQuestions = featuredQuestions;
        }

        public AllInterviewsView Load(AllInterviewsInputModel input)
        {
            var interviews = this.reader.Query(_ =>
            {
                var items = ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                return items.Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
            });


            var totalCount = this.reader.Query(_ => ApplyFilter(input, _).Count());
            var requiredInterviews = interviews.Select(y => y.SummaryId).ToList();
            var featuredQuestionAnswers = this.featuredQuestions.Query(_ => _.Where(x => requiredInterviews.Contains(x.InterviewSummary.SummaryId)).OrderBy(x => x.Position).ToList());


            var result = new AllInterviewsView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = interviews.Select(x => new AllInterviewsViewItem
                {
                    FeaturedQuestions = featuredQuestionAnswers.Where(f => f.InterviewSummary.SummaryId == x.SummaryId)
                    .Select(a => new InterviewFeaturedQuestion
                    {
                        Id = a.Questionid,
                        Answer = a.Answer,
                        Question = a.Title,
                        Type = a.Type
                    }).ToList(),
                    InterviewId = x.InterviewId,
                    LastEntryDate = x.UpdateDate.ToShortDateString(),
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    ResponsibleRole = x.ResponsibleRole,
                    HasErrors = x.HasErrors,
                    Status = x.Status.ToString(),
                    CanDelete =    x.Status == InterviewStatus.Created
                        || x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.SentToCapi,
                    CanApprove = x.Status == InterviewStatus.ApprovedBySupervisor || x.Status == InterviewStatus.Completed,
                    CanReject = x.Status == InterviewStatus.ApprovedBySupervisor,
                    CanUnapprove = x.Status == InterviewStatus.ApprovedByHeadquarters,
                    CanBeReassigned = x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.Completed
                        || x.Status == InterviewStatus.RejectedBySupervisor
                        || x.Status == InterviewStatus.RejectedByHeadquarters,
                    QuestionnaireId = x.QuestionnaireId,
                    QuestionnaireVersion = x.QuestionnaireVersion,
                    CreatedOnClient = x.WasCreatedOnClient,
                    ReceivedByInterviewer = x.ReceivedByInterviewer,
                    Key = x.Key
                }).ToList()
            };
            return result;
        }

        public InterviewsWithoutPrefilledView LoadInterviewsWithoutPrefilled(InterviewsWithoutPrefilledInputModel input)
        {
            List<InterviewSummary> interviews = new List<InterviewSummary>();
            int totalCount;
            if (input.InterviewId.HasValue)
            {
                var interviewSummary = this.reader.GetById(input.InterviewId.Value);
                if (interviewSummary!=null)
                {
                    interviews = interviewSummary.ToEnumerable().ToList();
                }
                totalCount = interviews.Count;
            }
            else
            {
                interviews = this.reader.Query(_ =>
                {
                    var items = ApplyFilter(input, _);
                    if (input.Orders != null)
                    {
                        items = this.DefineOrderBy(items, input);
                    }

                    return items.Skip((input.Page - 1)*input.PageSize)
                        .Take(input.PageSize)
                        .ToList();
                });

                totalCount = this.reader.Query(_ => ApplyFilter(input, _).Count());
            }
            var result = new InterviewsWithoutPrefilledView
            {
                TotalCount = totalCount,
                Items = interviews.Select(x => new InterviewListItem
                {
                    InterviewId = x.InterviewId,
                    Key = x.Key,
                    QuestionnaireId = new QuestionnaireIdentity(x.QuestionnaireId, x.QuestionnaireVersion).ToString(),
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    ResponsibleRole = x.ResponsibleRole,
                    TeamLeadId = x.TeamLeadId,
                    TeamLeadName = x.TeamLeadName,
                    Status = x.Status,
                    UpdateDate = x.UpdateDate.ToLocalTime().FormatDateWithTime(),
                    WasCreatedOnClient = x.WasCreatedOnClient,
                    ReceivedByInterviewer = x.ReceivedByInterviewer,
                }).ToList()
            };
            return result;
        }

        private static IQueryable<InterviewSummary> ApplyFilter(InterviewsWithoutPrefilledInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _.Where(x => !x.IsDeleted);

            if (input.SupervisorId.HasValue)
            {
                items = items.Where(x => x.TeamLeadId == input.SupervisorId);
            }

            if (!string.IsNullOrWhiteSpace(input.InterviewKey))
            {
                items = items.Where(x => x.Key == input.InterviewKey);
            }

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.Key.StartsWith(input.SearchBy));
            }

            if (input.CensusOnly)
            {
                items = items.Where(x => x.WasCreatedOnClient);
            }

            if (input.InterviewerId.HasValue)
            {
                items = items.Where(x => x.ResponsibleId == input.InterviewerId);
            }

            if (input.QuestionnaireId!=null)
            {
                items = items.Where(x => x.QuestionnaireId == input.QuestionnaireId.QuestionnaireId && x.QuestionnaireVersion == input.QuestionnaireId.Version);
            }

            if (input.ChangedFrom.HasValue)
            {
                items = items.Where(x => x.UpdateDate >= input.ChangedFrom.Value);
            }

            if (input.ChangedTo.HasValue)
            {
                items = items.Where(x => x.UpdateDate <= input.ChangedTo.Value);
            }

            return items;
        }

        private static IQueryable<InterviewSummary> ApplyFilter(AllInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.Key.StartsWith(input.SearchBy) || x.AnswersToFeaturedQuestions.Any(a => a.Answer.StartsWith(input.SearchBy)));
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

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query, ListViewModelBase model)
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
