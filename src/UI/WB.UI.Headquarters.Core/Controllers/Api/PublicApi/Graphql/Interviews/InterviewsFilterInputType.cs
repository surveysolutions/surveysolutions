using HotChocolate.Types.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewsFilterInputType : FilterInputType<InterviewSummary>
    {
        protected override void Configure(IFilterInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("InterviewFilter");
            descriptor.Filter(x => x.Status)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowNotEquals().And().AllowIn();

            descriptor.Filter(x => x.QuestionnaireId)
                .BindFiltersExplicitly()
                .AllowEquals();

            descriptor.Filter(x => x.QuestionnaireVersion)
                .BindFiltersExplicitly()
                .AllowEquals();

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
                    y.Filter(f => f.Answer).BindFiltersImplicitly();
                    y.Filter(f => f.AnswerCode).BindFiltersImplicitly();
                    
                    y.Object(x => x.Question)
                        .AllowObject<QuestionsFilterType>();
                }).Name("identifyingQuestions_some");
        }
    }
}
