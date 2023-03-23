using System;
using System.ComponentModel;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.UI.Headquarters.API.PublicApi.Models;
using AggregateInterviewAnswer = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.InterviewAnswer;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Localizable(false)]
    public class AssignmentsPublicApiMapProfile : Profile
    {
        public AssignmentsPublicApiMapProfile()
        {
            this.CreateMap<Assignment, FullAssignmentDetails>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId.ToString()))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible.Name))
                .ForMember(x => x.IsAudioRecordingEnabled, opts => opts.MapFrom(x => x.AudioRecording))
                .ForMember(x => x.Answers, opts => opts.MapFrom(x => x.Answers));

            this.CreateMap<AggregateInterviewAnswer, InterviewAnswer>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Identity, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Variable, opts => opts.UseDestinationValue())
                .AfterMap((row, item, ctx) =>
                    item.Variable = GetVariableName(ctx, item.Variable, item.Identity.Id.ToString()));

            this.CreateMap<Assignment, AssignmentDetails>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId.ToString()))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count))
                .ForMember(x => x.IsAudioRecordingEnabled, opts => opts.MapFrom(x => x.AudioRecording))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible.Name));

            this.CreateMap<IdentifyingAnswer, AssignmentIdentifyingDataItem>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Variable, opts => opts.MapFrom(x => x.VariableName))
                .AfterMap((row, item, ctx) => item.Variable = GetVariableName(ctx, item.Variable, item.Identity))
                .ForMember(x => x.Identity, opts => opts.MapFrom(x => x.Identity.ToString()));

            this.CreateMap<AssignmentIdentifyingQuestionRow, AssignmentIdentifyingDataItem>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Variable, opts => opts.MapFrom(x => x.Variable))
                    .AfterMap((row, item, ctx) => item.Variable = GetVariableName(ctx, item.Variable, item.Identity))
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
                .ForMember(x => x.ReceivedByTabletAtUtc, opts => opts.MapFrom(x => x.ReceivedByTabletAtUtc))
                .ForMember(x => x.Archived, opts => opts.MapFrom(x => x.Archived))
                .ForMember(x => x.IsAudioRecordingEnabled, opts => opts.MapFrom(x => x.IsAudioRecordingEnabled));
        }

        private string GetVariableName(ResolutionContext ctx, string value, string identity)
        {
            if (string.IsNullOrWhiteSpace(value) 
                //&& ctx != ctx.Mapper.DefaultContext
                && (ctx.TryGetItems(out var items) && items.TryGetValue("questionnaire", out var ctxItem)) 
                && ctxItem is IQuestionnaire questionnaire)
            {
                return questionnaire.GetEntityVariableOrThrow(Guid.Parse(identity));
            }

            return value;
        }
    }
}
