using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.PublicApi.Models;

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
                .ForMember(x => x.Capacity, opts => opts.MapFrom(x => x.Capacity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId.ToString()))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible.Name));

            this.CreateMap<IdentifyingAnswer, AssignmentIdentifyingDataItem>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Variable, opts => opts.ResolveUsing(
                    (answer, dest, value, ctx) => GetVariableName(ctx, answer.QuestionId)))
                .ForMember(x => x.QuestionId, opts => opts.MapFrom(x => x.QuestionId));

            this.CreateMap<List<AssignmentIdentifyingDataItem>, Assignment>()
                .BeforeMap((list, assignment, context) => this.PrepareQuestionnaire(context, assignment.QuestionnaireId))
                .AfterMap((list, assignment, context) =>
                {
                    var data = context.Mapper.Map<List<IdentifyingAnswer>>(list);
                    assignment.SetAnswers(data);
                });

            this.CreateMap<AssignmentIdentifyingDataItem, IdentifyingAnswer>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.VariableName, opts =>
                    opts.ResolveUsing((item, answer, value, ctx) =>
                        {
                            if (string.IsNullOrWhiteSpace(item.Variable))
                                return this.GetVariableName(ctx, item.QuestionId.Value);

                            return item.Variable;
                        }))
                .ForMember(x => x.QuestionId, opts =>
                    opts.ResolveUsing((item, answer, value, context) =>
                    {
                        if (item.QuestionId.HasValue) return item.QuestionId.Value;

                        var qd = this.GetFromScope<QuestionnaireDocument>(context);
                        var question = qd.Find<AbstractQuestion>(q => q.StataExportCaption == item.Variable);
                        Guid? id = question.FirstOrDefault()?.PublicKey;

                        return id.Value;
                    })
                );

            this.CreateMap<Assignment, PreloadedDataByFile>()
                .ConstructUsing((assignment, context) =>
                {
                    var questionnaire = this.GetQuestionnaireDocument(context, assignment.QuestionnaireId);

                    var id = $"Assignment_{assignment.Id}_{questionnaire.Title}";

                    var headers = assignment.IdentifyingData.Select(data =>
                    {
                        if (string.IsNullOrWhiteSpace(data.VariableName))
                            return questionnaire.Find<AbstractQuestion>(data.QuestionId).StataExportCaption;

                        return data.VariableName;
                    }).ToArray();

                    var content = new[] { assignment.IdentifyingData.Select(data => data.Answer).ToArray() };

                    return new PreloadedDataByFile(id, id, headers, content);
                });

            this.CreateMap<AssignmentRow, AssignmentViewItem>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId))
                .ForMember(x => x.Capacity, opts => opts.MapFrom(x => x.Capacity))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewsCount))
                .ForMember(x => x.ResponsibleId, opts => opts.MapFrom(x => x.ResponsibleId))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible))
                .ForMember(x => x.CreatedAtUtc, opts => opts.MapFrom(x => x.CreatedAtUtc))
                .ForMember(x => x.UpdatedAtUtc, opts => opts.MapFrom(x => x.UpdatedAtUtc))
                .ForMember(x => x.Archived, opts => opts.MapFrom(x => x.Archived));
        }

        private T GetService<T>(ResolutionContext context) where T : class
        {
            return context.Mapper.ServiceCtor(typeof(T)) as T;
        }

        private void PrepareQuestionnaire(ResolutionContext context, QuestionnaireIdentity questionnaireIdentity)
        {
            var document = this.GetQuestionnaireDocument(context, questionnaireIdentity);
            this.SetInScope(context, document);
        }

        private QuestionnaireDocument GetQuestionnaireDocument(ResolutionContext context, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaierStorage = this.GetService<IQuestionnaireStorage>(context);
            return questionnaierStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
        }

        private string GetVariableName(ResolutionContext ctx, Guid questionId)
        {
            var questionnaier = this.GetFromScope<QuestionnaireDocument>(ctx);
            return questionnaier?.Find<AbstractQuestion>(questionId)?.StataExportCaption;
        }

        private void SetInScope<T>(ResolutionContext ctx, T value)
        {
            ctx.Items[typeof(T).Name] = value;
        }

        private T GetFromScope<T>(ResolutionContext ctx) where T : class
        {
            return ctx.Items[typeof(T).Name] as T;
        }
    }
}