using AngleSharp.Common;
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
            
            descriptor.Filter(x => x.QuestionnaireVariable)
                .BindFiltersExplicitly()
                .AllowEquals();

            descriptor.Filter(x => x.QuestionnaireVersion)
                .BindFiltersExplicitly()
                .AllowEquals();

            descriptor.Filter(x => x.Key)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowStartsWith().And().AllowContains().And().AllowIn();
            
            descriptor.Filter(x => x.NotAnsweredCount);
            
            descriptor.Filter(x => x.ClientKey)
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

            descriptor.Filter(x => x.ResponsibleNameLowerCase)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowStartsWith().And().AllowIn().And().AllowNotIn();

            descriptor.Filter(x => x.SupervisorName)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowStartsWith().And().AllowIn().And().AllowNotIn();

            descriptor.Filter(x => x.SupervisorNameLowerCase)
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
            
            descriptor.Filter(x => x.ReceivedByInterviewerAtUtc).BindFiltersExplicitly()
                .AllowEquals().And().AllowGreaterThan().And().AllowLowerThan();
            
            descriptor.Filter(x => x.ErrorsCount).BindFiltersExplicitly()
                .AllowEquals().And().AllowGreaterThan();
            
            descriptor.List(x => x.AnswersToFeaturedQuestions)
                .BindExplicitly()
                .AllowSome(y =>
                {
                    y.BindFieldsExplicitly();
                    
                    y.Filter(f => f.Answer)
                        .BindFiltersExplicitly()
                        .AllowEquals().Description("Allows case sensitive equals comparison of answer")
                            .And().AllowNotEquals().Description("Allows case sensitive not equals comparison of answer")
                            .And().AllowStartsWith().Description("Allows case sensitive starts with comparison of answer")
                            .And().AllowNotStartsWith().Description("Allows case sensitive not starts with comparison of answer");
                    
                    y.Filter(f => f.AnswerLowerCase)
                        .BindFiltersExplicitly()
                        .AllowEquals().Description("Allows case insensitive equals comparison of answer")
                        .And().AllowNotEquals().Description("Allows case insensitive not equals comparison of answer")
                        .And().AllowStartsWith().Description("Allows case insensitive starts with comparison of answer")
                        .And().AllowNotStartsWith().Description("Allows case insensitive not starts with comparison of answer");
                    
                    y.Filter(f => f.AnswerCode)
                        .BindFiltersExplicitly()
                        .AllowEquals().And().AllowIn().And().AllowNotEquals().And().AllowNotIn();
                    
                    y.Object(x => x.Question)
                        .AllowObject<QuestionsFilterType>();
                }).Name("identifyingQuestions_some");
        }
    }
}
