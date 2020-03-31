using System.Linq;
using HotChocolate.Types;
using HotChocolate.Types.Filters;
using HotChocolate.Types.Relay;
using HotChocolate.Types.Sorting;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class InterviewSummaryObject : ObjectType<InterviewSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            
            descriptor.Field(x => x.SummaryId)
                .Name("id")
                .Type<NonNullType<IdType>>();
            
            descriptor.Field(x => x.Status)
                .Type<NonNullType<EnumType<InterviewStatus>>>();
            
            descriptor.Field(x => x.ResponsibleName)
                .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.ResponsibleRole)
                .Type<NonNullType<EnumType<UserRoles>>>();

            descriptor.Field(x => x.AssignmentId)
                .Type<NonNullType<IntType>>();

            descriptor.Field(x => x.CreatedDate)
                .Description("Utc creation date");

            descriptor.Field(x => x.Key)
                .Type<NonNullType<StringType>>();
            
            descriptor.Field(x => x.UpdateDate)
                .Description("Utc last updated date");
            
            descriptor.Field(x => x.ReceivedByInterviewer);
            
            descriptor.Field(x => x.ErrorsCount)
                .Name("invalidAnswersCount");

            descriptor.Field(x => x.QuestionnaireId);
            descriptor.Field(x => x.QuestionnaireVersion);
            
            descriptor.Field(x => x.AnswersToFeaturedQuestions)
                .Name("questions")
                .Resolver(x =>
                {
                    var interviewSummary = x.Parent<InterviewSummary>();
                    var unitOfWork = x.Service<IUnitOfWork>();
                    var answers = unitOfWork.Session.Query<QuestionAnswer>()
                        .Where(a => a.InterviewSummary == interviewSummary)
                        .OrderBy(a => a.Position)
                        .ToList();
                    return answers;
                })
                .Type<NonNullType<ListType<NonNullType<AnswerObject>>>>();
        }
    }

    public class AnswerObject : ObjectType<QuestionAnswer>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionAnswer> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            
            descriptor.Field(x => x.Answer)
                .Type<StringType>();
            
            descriptor.Field(x => x.Variable)
                .Type<NonNullType<StringType>>();
        }
    }

    public class InterviewsQueryType : ObjectType<InterviewsQuery>
    {
        protected override void Configure(IObjectTypeDescriptor<InterviewsQuery> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Field(x => x.GetInterviews(default, default))
                .Authorize()
                .Type<NonNullType<ListType<InterviewSummaryObject>>>()
                .UsePaging<InterviewSummaryObject>()
                .UseFiltering<InterviewsQueryFilerType>()
                .UseSorting<InterviewsQuerySortType>();
        }
    }

    public class InterviewsQuerySortType : SortInputType<InterviewSummary>
    {
        protected override void Configure(ISortInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Sortable(x => x.Key);
            descriptor.Sortable(x => x.CreatedDate);
            descriptor.Sortable(x => x.UpdateDate);
            descriptor.Sortable(x => x.SummaryId).Name("id");
        }
    }

    public class InterviewsQueryFilerType : FilterInputType<InterviewSummary>
    {
        protected override void Configure(IFilterInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Filter(x => x.Status)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowNotEquals().And().AllowIn();
            
            descriptor.Filter(x => x.Key)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowStartsWith().And().AllowContains().And().AllowIn();
            descriptor.Filter(x => x.AssignmentId)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowIn().And().AllowNotEquals();
            descriptor.Filter(x => x.CreatedDate)
                .BindFiltersExplicitly()
                .AllowGreaterThan()
                .And().AllowGreaterThanOrEquals()
                .And().AllowLowerThan()
                .And().AllowLowerThanOrEquals()
                .And().AllowNotGreaterThan()
                .And().AllowNotGreaterThanOrEquals()
                .And().AllowNotLowerThan()
                .And().AllowNotLowerThanOrEquals();
            descriptor.Filter(x => x.ResponsibleName)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowStartsWith().And().AllowIn().And().AllowNotIn();
            descriptor.Filter(x => x.ResponsibleRole)
                .BindFiltersExplicitly()
                .AllowEquals();
            
            descriptor.Filter(x => x.UpdateDate).BindFiltersExplicitly()
                .AllowGreaterThan()
                .And().AllowGreaterThanOrEquals()
                .And().AllowLowerThan()
                .And().AllowLowerThanOrEquals()
                .And().AllowNotGreaterThan()
                .And().AllowNotGreaterThanOrEquals()
                .And().AllowNotLowerThan()
                .And().AllowNotLowerThanOrEquals();
            
            descriptor.Filter(x => x.ReceivedByInterviewer).BindFiltersExplicitly().AllowEquals();
            descriptor.Filter(x => x.ErrorsCount).BindFiltersExplicitly()
                .AllowEquals().And().AllowGreaterThan();
            
            descriptor.List(x => x.AnswersToFeaturedQuestions)
                .BindExplicitly()
                .AllowSome(y =>
                {
                    y.BindFieldsExplicitly();
                    
                    y.Filter(f => f.Answer)
                        .BindFiltersImplicitly();
                    y.Filter(x => x.Variable)
                        .BindFiltersExplicitly()
                        .AllowEquals();
                }).Name("answers_some");
        }
    }
}
