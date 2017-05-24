using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentViewFactory
    {
        AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input);
    }

    internal class AssignmentViewFactory : IAssignmentViewFactory
    {
        private readonly IPlainStorageAccessor<Assignment> asssignmentsStorage;

        public AssignmentViewFactory(IPlainStorageAccessor<Assignment> asssignmentsStorage)
        {
            this.asssignmentsStorage = asssignmentsStorage;
        }

        public AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input)
        {
            var assignments = asssignmentsStorage.Query(_ =>
            {
                var items = ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                return items.Fetch(x => x.Responsible).Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
            });

            var totalCount = asssignmentsStorage.Query(_ => ApplyFilter(input, _).Count());
            //var requiredInterviews = assignments.Select(y => y.Id).ToList();

            var result = new AssignmentsWithoutIdentifingData
            {
                TotalCount = totalCount,
                Page = input.Page,
                PageSize = input.PageSize,
                Items = assignments.Select(x => new AssignmentWithoutIdentifingData
                {
                    CreatedAtUtc = x.CreatedAtUtc,
                    ResponsibleId = x.ResponsibleId,
                    UpdatedAtUtc = x.UpdatedAtUtc,
                    Capacity = x.Capacity,
                    InterviewsCount = 5,
                    Id = x.Id,
                    Responsible = x.Responsible.Name
                })
            };

            return result;
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
                items = items.Where(x => x.IdentifyingData.Any(a => a.Answer.StartsWith(input.SearchBy)));
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

            return items;
        }
    }

    public class AssignmentsInputModel : ListViewModelBase
    {
        public string SearchBy { get; set; }
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }

    public class AssignmentsWithoutIdentifingData : IListView<AssignmentWithoutIdentifingData>
    {
        public int TotalCount { get; set; }
        public IEnumerable<AssignmentWithoutIdentifingData> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class AssignmentWithoutIdentifingData
    {
        public DateTime CreatedAtUtc { get; set; }
        public Guid ResponsibleId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int? Capacity { get; set; }
        public int InterviewsCount { get; set; }
        public int Id { get; set; }
        public string Responsible { get; set; }
    }
}