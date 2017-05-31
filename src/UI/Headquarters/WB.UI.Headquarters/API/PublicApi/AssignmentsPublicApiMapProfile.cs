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
                .BeforeMap(this.PrepareQuestionnaire)
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id.ToString()))
                .ForMember(x => x.Capacity, opts => opts.MapFrom(x => x.Capacity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count))
                .ForMember(x => x.ResponsibleName, opts => opts.MapFrom(x => x.Responsible.Name));

            this.CreateMap<IdentifyingAnswer, AssignmentIdentifyingDataItem>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Variable, opts => opts.ResolveUsing(this.VariableNameResolver))
                .ForMember(x => x.QuestionId, opts => opts.MapFrom(x => x.QuestionId));

            this.CreateMap<List<AssignmentIdentifyingDataItem>, Assignment>()
                .BeforeMap((list, assignment, context) => this.PrepareQuestionnaire<string>(assignment, null, context))
                .AfterMap((list, assignment, context) =>
                {
                    var data = context.Mapper.Map<List<IdentifyingAnswer>>(list);
                    assignment.SetAnswers(data);
                });

            this.CreateMap<AssignmentIdentifyingDataItem, IdentifyingAnswer>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.VariableName, opts =>
                {
                    opts.Condition(c => !string.IsNullOrWhiteSpace(c.Variable));
                    opts.MapFrom(c => c.Variable);
                })
                .ForMember(x => x.QuestionId, opts =>
                {
                    opts.Condition(c => c.QuestionId.HasValue);
                    opts.MapFrom(x => x.QuestionId);
                })
                .ForMember(x => x.VariableName, opts =>
                {
                    // map variable name from QuestionId if source do not have variable name
                    opts.Condition(c => string.IsNullOrWhiteSpace(c.Variable) && c.QuestionId != null);
                    opts.ResolveUsing(
                        (item, answer, value, ctx) => this.VariableNameResolver<string>(answer, null, value, ctx));
                })
                .ForMember(x => x.QuestionId, opts =>
                {
                    opts.Condition(c => !c.QuestionId.HasValue && !string.IsNullOrWhiteSpace(c.Variable));
                    opts.ResolveUsing((item, answer, value, context) =>
                    {
                        var qd = this.GetFromScope<QuestionnaireDocument>(context);
                        var question = qd.Find<AbstractQuestion>(q => q.StataExportCaption == item.Variable);
                        Guid? id = question.FirstOrDefault()?.PublicKey;

                        answer.QuestionId = id.Value;
                        return id.Value;
                    });
                });
            
            this.CreateMap<Assignment, PreloadedDataByFile>()
                .ConstructUsing((assignment, context) =>
                {
                    var questionnaire = this.GetQuestionnaireDocument(context, assignment.QuestionnaireId);

                    var id = $"Assignment_{assignment.Id}_{questionnaire.Title}";

                    var headers = assignment.IdentifyingData.Select(data =>
                    {
                        return string.IsNullOrWhiteSpace(data.VariableName)
                            ? questionnaire.Find<AbstractQuestion>(data.QuestionId).StataExportCaption
                            : data.VariableName;
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

        private void PrepareQuestionnaire<TDest>(Assignment assignment, TDest details, ResolutionContext context)
        {
            var document = this.GetQuestionnaireDocument(context, assignment.QuestionnaireId);
            this.SetInScope(context, document);
        }

        private QuestionnaireDocument GetQuestionnaireDocument(ResolutionContext context, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaierStorage = this.GetService<IQuestionnaireStorage>(context);
            return questionnaierStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
        }

        private string VariableNameResolver<TDest>(IdentifyingAnswer answer, TDest item, string value, ResolutionContext context)
        {
            var questionnaier = this.GetFromScope<QuestionnaireDocument>(context);
            return questionnaier?.Find<AbstractQuestion>(answer.QuestionId)?.StataExportCaption;
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