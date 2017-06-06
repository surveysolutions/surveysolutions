using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
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
                    (answer, dest, value, ctx) => GetVariableName(ctx, answer.QuestionId)))
                .ForMember(x => x.QuestionId, opts => opts.MapFrom(x => x.QuestionId));

            this.CreateMap<Assignment, PreloadedDataByFile>()
                .ConstructUsing((assignment, context) =>
                {
                    var questionnaire = this.GetQuestionnaire(context, assignment.QuestionnaireId);

                    var id = $"Assignment_{assignment.Id}_{questionnaire.Title}";

                    var headers = assignment.IdentifyingData.Select(data =>
                    {
                        if (string.IsNullOrWhiteSpace(data.VariableName))
                            return questionnaire.GetQuestionVariableName(data.QuestionId);

                        return data.VariableName;
                    }).ToArray();

                    var content = new[] { assignment.IdentifyingData.Select(data => data.Answer).ToArray() };

                    return new PreloadedDataByFile(id, id, headers, content);
                });

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
            var document = this.GetQuestionnaire(context, questionnaireIdentity);
            context.Set(document);
        }

        private IQuestionnaire GetQuestionnaire(ResolutionContext context, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaierStorage = context.Resolve<IQuestionnaireStorage>();
            return questionnaierStorage.GetQuestionnaire(questionnaireIdentity, null);
        }

        private string GetVariableName(ResolutionContext ctx, Guid? questionId)
        {
            if (questionId == null) return null;

            var questionnaier = ctx.Get<IQuestionnaire>();
            return questionnaier.GetQuestionVariableName(questionId.Value);
        }
    }
}