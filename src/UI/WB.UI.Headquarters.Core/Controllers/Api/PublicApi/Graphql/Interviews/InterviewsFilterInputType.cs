using System;
using System.Collections.Generic;
using HotChocolate.Data.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewsFilterInputType : FilterInputType<InterviewSummary>
    {
        protected override void Configure(IFilterInputTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("InterviewsFilter");
            
            descriptor.Field(x => x.Status);
            descriptor.Field(x => x.InterviewMode);
            descriptor.Field(x => x.QuestionnaireId);
            descriptor.Field(x => x.QuestionnaireVariable);
            descriptor.Field(x => x.QuestionnaireVersion);
            descriptor.Field(x => x.Key);
            descriptor.Field(x => x.NotAnsweredCount);
            descriptor.Field(x => x.ClientKey);
            descriptor.Field(x => x.AssignmentId);
            descriptor.Field(x => x.CreatedDate);
            descriptor.Field(x => x.ResponsibleName);
            descriptor.Field(x => x.ResponsibleNameLowerCase);
            descriptor.Field(x => x.SupervisorName);
            descriptor.Field(x => x.SupervisorNameLowerCase);
            descriptor.Field(x => x.ResponsibleRole);       
            descriptor.Field(x => x.UpdateDate).Name("updateDateUtc");
            descriptor.Field(x => x.ReceivedByInterviewerAtUtc);
            descriptor.Field(x => x.ErrorsCount);
            descriptor.Field(x => x.IdentifyEntitiesValues)
                .Name("identifyingData");
        }
    }
}
