using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Sanitizer;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentViewFactory : IAssignmentViewFactory
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentViewFactory(IPlainStorageAccessor<Assignment> assignmentsStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.assignmentsStorage = assignmentsStorage;
            this.summaries = summaries;
            this.questionnaireStorage = questionnaireStorage;
        }

        public AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input)
        {
            List<int> ids = new List<int>();
            var assignments = this.assignmentsStorage.Query(_ =>
            {
                if (input.Limit == null && input.Offset == null)
                {
                    input.Limit = input.PageSize;
                    input.Offset = (input.Page - 1) * input.PageSize;
                }

                var items = this.ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                ids = items.Skip(input.Offset.Value)
                    .Take(input.Limit.Value)
                    .Select(x => x.Id)
                    .ToList();

                var neededItems = _.Where(x => ids.Contains(x.Id));

                var fetchReqests = this.DefineOrderBy(neededItems, input)
                    .Fetch(x => x.IdentifyingData)
                    .Fetch(x => x.Responsible);
                    //.Fetch(x => x.RoleIds); //throws Null reference exception, but should be here :( https://stackoverflow.com/q/21243592/72174

                List<Assignment> list;

                if (input.ShowQuestionnaireTitle)
                {
                    list = fetchReqests.Fetch(x => x.Questionnaire).ToList();
                }
                else
                {
                    list = fetchReqests.ToList();
                }

                return list;
            });

            var counts = summaries.Query(_ => 
                (from i in _
                where ids.Contains((int)i.AssignmentId)
                group i by i.AssignmentId into gr
                select new
                {
                    gr.Key,
                    InterviewsCount = gr.Count()
                }).ToList());

            var result = new AssignmentsWithoutIdentifingData
            {
                Page = input.Page,
                PageSize = input.PageSize,
                Items = assignments.Select(x =>
                {
                    var row = new AssignmentRow
                    {
                        QuestionnaireId = x.QuestionnaireId,
                        CreatedAtUtc = x.CreatedAtUtc,
                        ResponsibleId = x.ResponsibleId,
                        UpdatedAtUtc = x.UpdatedAtUtc,
                        Quantity = x.Quantity ?? -1,
                        InterviewsCount = counts.FirstOrDefault(c => c.Key == x.Id)?.InterviewsCount ?? 0,
                        Id = x.Id,
                        Archived = x.Archived,
                        Responsible = x.Responsible.Name,
                        ResponsibleRole = x.Responsible.RoleIds.First().ToUserRole().ToString(),
                        IdentifyingQuestions = this.GetIdentifyingColumnText(x)
                    };

                    if (input.ShowQuestionnaireTitle)
                    {
                        row.QuestionnaireTitle = x.Questionnaire?.Title;
                    }

                    return row;
                }).ToList(),
            };

            result.TotalCount = this.assignmentsStorage.Query(_ => this.ApplyFilter(input, _).Count());

            return result;
        }

        private List<AssignmentIdentifyingQuestionRow> GetIdentifyingColumnText(Assignment assignment)
        {
            QuestionnaireIdentity assignmentQuestionnaireId = assignment.QuestionnaireId;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);

            if (questionnaire == null) return new List<AssignmentIdentifyingQuestionRow>();

            List<AssignmentIdentifyingQuestionRow> identifyingColumnText =
                assignment.IdentifyingData
                          .Where(x => questionnaire.GetQuestionType(x.Identity.Id) != QuestionType.GpsCoordinates)
                          .Select(x => new AssignmentIdentifyingQuestionRow(questionnaire.GetQuestionTitle(x.Identity.Id).RemoveHtmlTags(), x.AnswerAsString))
                          .ToList();
            return identifyingColumnText;
        }

        private IQueryable<Assignment> DefineOrderBy(IQueryable<Assignment> query, AssignmentsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x => x.UpdatedAtUtc);
            }

            if (orderBy.Field.Contains("InterviewsCount"))
            {
                return OrderByInterviewsCount(query, orderBy);
            }

            if (orderBy.Field.Contains("QuestionnaireTitle"))
            {
                return OrderByQuestionnaire(query, orderBy);
            }

            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

        private static readonly Expression<Func<Assignment, int>> OrderByQuery = x => x.InterviewSummaries.Count;
        private static IQueryable<Assignment> OrderByInterviewsCount(IQueryable<Assignment> query, OrderRequestItem orderBy)
        {
            return orderBy.Direction == OrderDirection.Asc 
                ? query.OrderBy(OrderByQuery) 
                : query.OrderByDescending(OrderByQuery);
        }

        static readonly Expression<Func<Assignment, string>> OrderByQuestionnaireTitle = x => x.Questionnaire.Title;
        private static IQueryable<Assignment> OrderByQuestionnaire(IQueryable<Assignment> query, OrderRequestItem orderBy)
        {
            return orderBy.Direction == OrderDirection.Asc 
                ? query.OrderBy(OrderByQuestionnaireTitle) 
                : query.OrderByDescending(OrderByQuestionnaireTitle);
        }

        private IQueryable<Assignment> ApplyFilter(AssignmentsInputModel input, IQueryable<Assignment> assignments)
        {
            var items = assignments.Where(x => x.Archived == input.ShowArchive);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                int id = 0;

                var lowerSearchBy = input.SearchBy.ToLower();

                Expression<Func<Assignment, bool>> textSearchExpression = x => false;

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.IdentifyingQuestions))
                {
                    textSearchExpression = textSearchExpression
                        .OrCondition(x => x.IdentifyingData.Any(a => a.AnswerAsString.ToLower().Contains(lowerSearchBy)));
                }

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.ResponsibleId))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Responsible.Name.ToLower().Contains(lowerSearchBy));
                }

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.ResponsibleId))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Questionnaire.Title.ToLower().Contains(lowerSearchBy));
                }

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.Id) && int.TryParse(input.SearchBy, out id))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Id == id);
                }

                items = items.Where(textSearchExpression);
            }

            if (input.ReceivedByTablet != AssignmentReceivedState.All)
            {
                if (input.ReceivedByTablet == AssignmentReceivedState.NotReceived)
                {
                    items = items.Where(x => x.ReceivedByTabletAtUtc == null);
                }
                else if (input.ReceivedByTablet == AssignmentReceivedState.Received)
                {
                    items = items.Where(x => x.ReceivedByTabletAtUtc != null);
                }
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId.QuestionnaireId == input.QuestionnaireId);
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId.Version == input.QuestionnaireVersion);
            }

            if (input.ResponsibleId.HasValue)
            {
                items = items.Where(x => x.ResponsibleId == input.ResponsibleId);
            }

            if (input.SupervisorId.HasValue)
            {
                items = items.Where(x => x.Responsible.ReadonlyProfile.SupervisorId == input.SupervisorId || x.ResponsibleId == input.SupervisorId);
            }

            if (input.OnlyWithInterviewsNeeded)
            {
                items = items.Where(x => !x.Quantity.HasValue || x.Quantity - x.InterviewSummaries.Count > 0);
            }

            if (input.DateStart.HasValue || input.DateEnd.HasValue)
            {
                items = items.Where(x => 
                    x.Quantity.HasValue 
                    && (x.CreatedAtUtc >= input.DateStart || input.DateStart == null)
                    && (x.CreatedAtUtc <  input.DateEnd   || input.DateEnd == null)
                );
            }

            if (input.UserRole.HasValue)
            {
                items = items.Where(x => 
                    x.Responsible.RoleIds.Any(r => r == input.UserRole.Value.ToUserId())
                );
            }

            return items;
        }
    }
}
