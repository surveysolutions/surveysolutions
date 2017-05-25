using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentViewFactory : IAssignmentViewFactory
    {
        private readonly IPlainStorageAccessor<Assignment> asssignmentsStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentViewFactory(IPlainStorageAccessor<Assignment> asssignmentsStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.asssignmentsStorage = asssignmentsStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input)
        {
            var assignments = this.asssignmentsStorage.Query(_ =>
            {
                var items = this.ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                var ids = items.Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .Select(x => x.Id)
                    .ToList();

                var neededItems = _.Where(x => ids.Contains(x.Id));
                var list = this.DefineOrderBy(neededItems, input).Fetch(x =>x.IdentifyingData).Fetch(x => x.Responsible).ToList();

                return list;
            });

            var result = new AssignmentsWithoutIdentifingData
            {
                Page = input.Page,
                PageSize = input.PageSize,
                Items = assignments.Select(x => new AssignmentRow
                {
                    CreatedAtUtc = x.CreatedAtUtc,
                    ResponsibleId = x.ResponsibleId,
                    UpdatedAtUtc = x.UpdatedAtUtc,
                    Capacity = x.Capacity,
                    InterviewsCount = 5,
                    Id = x.Id,
                    Responsible = x.Responsible.Name,
                    IdentifyingQuestions = this.GetIdentifyingColumnText(x)
                }),
            };

            result.TotalCount = this.asssignmentsStorage.Query(_ => this.ApplyFilter(input, _).Count());

            return result;
        }

        private Dictionary<string, string> GetIdentifyingColumnText(Assignment assignment)
        {
            QuestionnaireIdentity assignmentQuestionnaireId = assignment.QuestionnaireId;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);

            return assignment.IdentifyingData.ToDictionary(_ => questionnaire.GetQuestionTitle(_.QuestionId), _ => _.Answer);
        }

        private IQueryable<Assignment> DefineOrderBy(IQueryable<Assignment> query, ListViewModelBase model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x => x.UpdatedAtUtc);
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

        private IQueryable<Assignment> ApplyFilter(AssignmentsInputModel input, IQueryable<Assignment> assignments)
        {
            var items = assignments;
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                int id = 0;
                if (int.TryParse(input.SearchBy, out id))
                {
                    items = items.Where(x => x.Id == id || x.Responsible.Name.Contains(input.SearchBy) || x.IdentifyingData.Any(a => a.Answer.StartsWith(input.SearchBy)));
                }
                else
                {
                    items = items.Where(x => x.Responsible.Name.Contains(input.SearchBy) || x.IdentifyingData.Any(a => a.Answer.StartsWith(input.SearchBy)));
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

            return items;
        }
    }
}