using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
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
                .Type<NonNullType<ListType<AnswerObject>>>();
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
                .Type<NonNullType<IdType>>();

            descriptor.Field(x => x.InterviewSummary)
                .Name("interview");
        }
    }
    
    public class InterviewsQuery
    {
        public IQueryable<InterviewSummary> GetInterviews([Service] IUnitOfWork unitOfWork)
        {
            return unitOfWork.Session.Query<InterviewSummary>();
        }
    }

    public class InterviewsQueryType : ObjectType<InterviewsQuery>
    {
        protected override void Configure(IObjectTypeDescriptor<InterviewsQuery> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Field(x => x.GetInterviews(default))
                .Type<NonNullType<ListType<InterviewSummaryObject>>>()
                .UseFiltering<InterviewsQueryFilerType>();
        }
    }

    public class InterviewsQueryFilerType : FilterInputType<InterviewSummary>
    {
        protected override void Configure(IFilterInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly()
                .Filter(x => x.Status)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowNotEquals().And().AllowIn().And().AllowNotIn();
            descriptor.List(x => x.AnswersToFeaturedQuestions)
                .BindExplicitly()
                .AllowSome(y => y.BindFieldsExplicitly().Filter(f => f.Answer).BindFiltersImplicitly()).Name("answers_some");
        }
    }
}
