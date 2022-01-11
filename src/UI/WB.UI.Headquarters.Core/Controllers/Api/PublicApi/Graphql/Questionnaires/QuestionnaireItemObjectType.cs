#nullable enable
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnaireItemObjectType : ObjectType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("QuestionnaireItem");
            
            descriptor.Field(x => x.EntityType)
                .Type<EntityTypeObjectType>();

            descriptor.Field(x => x.QuestionText)
                .Name("title")
                .Description("Question text or Variable label. May contain html tags.")
                .Type<StringType>();

            descriptor.Field(x => x.StataExportCaption)
                .Name("variable")
                .Type<StringType>();

            descriptor.Field(x => x.QuestionScope)
                .Name("scope")
                .Type<EnumType<QuestionScope>>();

            descriptor.Field(x => x.VariableLabel)
                .Name("label")
                .Type<StringType>();

            descriptor.Field(x => x.QuestionType)
                .Name("type")
                .Type<QuestionTypeObjectType>();

            descriptor.Field(x => x.VariableType)
                .Name("variableType")
                .Type<VariableTypeObjectType>();

            descriptor.Field(x => x.Featured)
                .Name("identifying")
                .Type<BooleanType>();

            descriptor.Field(x => x.IncludedInReportingAtUtc)
                .Name("includedInReportingAtUtc")
                .Type<DateTimeType>();

            descriptor.Field("options")
                .Name("options")
                .Resolve(async context => await
                    context.BatchDataLoader<int, List<CategoricalOption>>(async (keys, token) =>
                    {
                        var unitOfWork = context.Service<IUnitOfWork>();
                        var questions = await unitOfWork.Session.Query<QuestionnaireCompositeItem>()
                            .Where(q => Enumerable.Contains(keys, q.Id)).ToListAsync();

                        if(!questions.Any()) return new Dictionary<int, List<CategoricalOption>>();
                        
                        var questionnaireStorage = context.Service<IQuestionnaireStorage>();
                        string? language = null;
                        if (context.ScopedContextData.ContainsKey("language"))
                        {
                            language = context.ScopedContextData["language"]?.ToString();
                        }
                        
                        var questionnaires =  questions.Select(q
                            => (q.Id, q.EntityId,
                                questionnaireStorage.GetQuestionnaire(
                                    QuestionnaireIdentity.Parse(q.QuestionnaireIdentity), language))).ToList();

                        return questionnaires.Where(q => q.Item3 != null).ToDictionary(
                            q => q.Id, 
                            q =>
                            {
                                if (q.Item3!.IsQuestion(q.EntityId))
                                {
                                    var questionType = q.Item3!.GetQuestionType(q.EntityId);
                                    if (questionType == QuestionType.SingleOption)
                                        return q.Item3!.GetOptionsForQuestion(q.EntityId, null, null, null).ToList();
                                }
                                return new List<CategoricalOption>();
                            });
                      
                    },"optionsByQuestion").LoadAsync(context.Parent<QuestionnaireCompositeItem>().Id, default).ConfigureAwait(false))
                .Type<NonNullType<ListType<NonNullType<CategoricalOptionType>>>>();
        }
    }
}
