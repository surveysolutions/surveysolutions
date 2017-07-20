using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
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
        private readonly IQuestionnaireStorage questionnaireStorage;
        
        public AssignmentViewFactory(IPlainStorageAccessor<Assignment> assignmentsStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.assignmentsStorage = assignmentsStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input)
        {
            var assignments = this.assignmentsStorage.Query(_ =>
            {
                if (input.Limit == null && input.Offset == null)
                {
                    input.Limit = input.PageSize;
                    input.Offset = (input.Page - 1) * input.PageSize;
                }

                var items = this.ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                var ids = items.Skip(input.Offset.Value)
                    .Take(input.Limit.Value)
                    .Select(x => x.Id)
                    .ToList();

                var neededItems = _.Where(x => ids.Contains(x.Id));
                var list = this.DefineOrderBy(neededItems, input)
                                .Fetch(x =>x.IdentifyingData)
                                .Fetch(x => x.InterviewSummaries)
                                .Fetch(x => x.Responsible)
                                // .ThenFetch(x => x.RoleIds) throws Null reference exception, but should be here :( https://stackoverflow.com/q/21243592/72174
                                .ToList();

                return list;
            });

            var result = new AssignmentsWithoutIdentifingData
            {
                Page = input.Page,
                PageSize = input.PageSize,
                Items = assignments.Select(x => new AssignmentRow
                {
                    QuestionnaireId = x.QuestionnaireId,
                    CreatedAtUtc = x.CreatedAtUtc,
                    ResponsibleId = x.ResponsibleId,
                    UpdatedAtUtc = x.UpdatedAtUtc,
                    Quantity = x.Quantity,
                    InterviewsCount = x.InterviewSummaries.Count(s => !s.IsDeleted),
                    Id = x.Id,
                    Archived = x.Archived,
                    Responsible = x.Responsible.Name,
                    ResponsibleRole = x.Responsible.RoleIds.First().ToUserRole().ToString(),
                    IdentifyingQuestions = this.GetIdentifyingColumnText(x)
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
                assignment.IdentifyingData.Select(x => new AssignmentIdentifyingQuestionRow(questionnaire.GetQuestionTitle(x.Identity.Id).RemoveHtmlTags(), x.AnswerAsString))
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
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

        private static IQueryable<Assignment> OrderByInterviewsCount(IQueryable<Assignment> query, OrderRequestItem orderBy)
        {
            Expression<Func<Assignment, int>> orderByQuery = x => x.InterviewSummaries.Count(s => s.IsDeleted == false);

            if (orderBy.Direction == OrderDirection.Asc)
            {
                var defineOrderBy = query.OrderBy(orderByQuery);
                return defineOrderBy;
            }
            else
            {
                return query.OrderByDescending(orderByQuery);
            }
        }

        private IQueryable<Assignment> ApplyFilter(AssignmentsInputModel input, IQueryable<Assignment> assignments)
        {
            var items = assignments.Where(x => x.Archived == input.ShowArchive);
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                int id = 0;

                var lowerSearchBy = input.SearchBy.ToLower();

                Expression<Func<Assignment, bool>> textSearchExpression =
                    x => x.Responsible.Name.ToLower().Contains(lowerSearchBy) || x.IdentifyingData.Any(a => a.AnswerAsString.ToLower().Contains(lowerSearchBy));
                if (int.TryParse(input.SearchBy, out id))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Id == id);
                }

                items = items.Where(textSearchExpression);
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
                items = items.Where(x => !x.Quantity.HasValue || x.Quantity - x.InterviewSummaries.Count(c => !c.IsDeleted) > 0);
            }

            return items;
        }
    }
}