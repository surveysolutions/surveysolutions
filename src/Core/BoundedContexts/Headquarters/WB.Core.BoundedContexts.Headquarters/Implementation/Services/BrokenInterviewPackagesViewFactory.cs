﻿using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class BrokenInterviewPackagesViewFactory : IBrokenInterviewPackagesViewFactory
    {
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> plainStorageAccessor;
        public BrokenInterviewPackagesViewFactory(IPlainStorageAccessor<BrokenInterviewPackage> plainStorageAccessor)
        {
            this.plainStorageAccessor = plainStorageAccessor;
        }

        public BrokenInterviewPackagesView GetFilteredItems(BrokenInterviewPackageFilter filter)
        {
            return this.plainStorageAccessor.Query(queryable =>
            {
                IQueryable<BrokenInterviewPackage> query = queryable;

                if (filter.ResponsibleId.HasValue)
                {
                    query = query.Where(x => x.ResponsibleId == filter.ResponsibleId.Value);
                }

                if (!string.IsNullOrEmpty(filter.QuestionnaireIdentity))
                {
                    var questionnaireIdentity = QuestionnaireIdentity.Parse(filter.QuestionnaireIdentity);
                    query = query.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId && x.QuestionnaireVersion == questionnaireIdentity.Version);
                }

                if (!string.IsNullOrEmpty(filter.ExceptionType))
                {
                    query = query.Where(x => x.ExceptionType == filter.ExceptionType);
                }

                if (filter.ToProcessingDateTime.HasValue)
                {
                    query = query.Where(x => x.ProcessingDate <= filter.ToProcessingDateTime.Value.AddDays(1));
                }

                if (filter.FromProcessingDateTime.HasValue)
                {
                    query = query.Where(x => x.ProcessingDate >= filter.FromProcessingDateTime);
                }

                if (filter.SortOrder == null)
                {
                    filter.SortOrder = new[] { new OrderRequestItem() { Field = "ProcessingDate", Direction = OrderDirection.Desc } };
                }

                var totalCount = query.LongCount();

                var items = query.OrderUsingSortExpression(filter.SortOrder.GetOrderRequestString())
                    .Skip((filter.PageIndex - 1)*filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(package => new BrokenInterviewPackageView
                    {
                        Id = package.Id,
                        InterviewId = package.InterviewId,
                        IncomingDate = package.IncomingDate,
                        ProcessingDate = package.ProcessingDate,
                        ExceptionType = package.ExceptionType,
                        ExceptionMessage = package.ExceptionMessage,
                        ExceptionStackTrace = package.ExceptionStackTrace,
                        PackageSize = package.PackageSize
                    })
                    .ToList();

                return new BrokenInterviewPackagesView() { Items = items, TotalCount = totalCount };
            });
        }

        public BrokenInterviewPackageExceptionTypesView GetExceptionTypes(int pageSize, string searchBy)
        {
            return this.plainStorageAccessor.Query(queryable =>
            {
                var query = queryable;

                if (!string.IsNullOrEmpty(searchBy))
                    query = queryable.Where(x => x.ExceptionType.ToLower().Contains(searchBy.ToLower()));

                var exceptionTypesByQuery = query.GroupBy(x => x.ExceptionType).Where(x => x.Count() > 0).Select(x => x.Key).ToList();

                return new BrokenInterviewPackageExceptionTypesView
                {
                    ExceptionTypes = exceptionTypesByQuery.Take(pageSize),
                    TotalCountByQuery = exceptionTypesByQuery.Count()
                };
            });
        }

        public BrokenInterviewPackage GetLastInterviewBrokenPackage(Guid interviewId)
        {
            return this.plainStorageAccessor
                .Query(queryable => queryable.Where(x => x.InterviewId == interviewId)
                    .OrderByDescending(x => x.IncomingDate)
                    .Take(1)
                    .ToList())
                .SingleOrDefault();
        }
    }
}