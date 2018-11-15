using System;
using System.ComponentModel;
using System.Net.Http;
using System.Web;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    [Localizable(false)]
    public class AssignmentsPublicApiMapProfile : Profile
    {
        public AssignmentsPublicApiMapProfile()
        {
            this.CreateMap<Assignment, FullAssignmentDetails>()
                .BeforeMap((assignment, details, ctx) => this.PrepareQuestionnaire(ctx, assignment.QuestionnaireId))
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId.ToString()))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible.Name))
                .ForMember(x => x.Answers, opts => opts.MapFrom(x => x.Answers));

            this.CreateMap<Assignment, AssignmentDetails>()
                .BeforeMap((assignment, details, ctx) => this.PrepareQuestionnaire(ctx, assignment.QuestionnaireId))
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId.ToString()))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible.Name));

            this.CreateMap<IdentifyingAnswer, AssignmentIdentifyingDataItem>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Variable, opts => opts.ResolveUsing(
                    (answer, dest, value, ctx) => GetVariableName(ctx, answer.Identity.Id)))
                .ForMember(x => x.Identity, opts => opts.MapFrom(x => x.Identity.ToString()));

            this.CreateMap<AssignmentRow, AssignmentViewItem>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewsCount))
                .ForMember(x => x.ResponsibleId, opts => opts.MapFrom(x => x.ResponsibleId))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible))
                .ForMember(x => x.CreatedAtUtc, opts => opts.MapFrom(x => x.CreatedAtUtc))
                .ForMember(x => x.UpdatedAtUtc, opts => opts.MapFrom(x => x.UpdatedAtUtc))
                .ForMember(x => x.Archived, opts => opts.MapFrom(x => x.Archived));
        }

        private void PrepareQuestionnaire(ResolutionContext context, QuestionnaireIdentity questionnaireIdentity)
        {
            var document = this.GetQuestionnaire(questionnaireIdentity);
            context.Set(document);
        }

        private IQuestionnaire GetQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            // https://stackoverflow.com/a/24509451/72174
            var httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            var currentDependencyScope = httpRequestMessage.GetDependencyScope();
            var questionnaireStorage = (IQuestionnaireStorage)currentDependencyScope.GetService(typeof(IQuestionnaireStorage));
            return questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
        }

        private string GetVariableName(ResolutionContext ctx, Guid? questionId)
        {
            if (questionId == null) return null;

            var questionnaire = ctx.Get<IQuestionnaire>();
            return questionnaire.GetQuestionVariableName(questionId.Value);
        }
    }
}
