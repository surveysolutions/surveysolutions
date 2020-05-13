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
    public class QuestionItemObjectType : ObjectType<QuestionnaireCompositeItem>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionnaireCompositeItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("Question");
            
            descriptor.Field(x => x.QuestionText)
                .Description("Question text. May contain html tags.")
                .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.StataExportCaption)
                .Name("variable")
                .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.QuestionScope)
                .Name("scope")
                .Type<EnumType<QuestionScope>>();

            descriptor.Field(x => x.VariableLabel)
                .Name("label")
                .Type<StringType>();

            descriptor.Field(x => x.QuestionType)
                .Name("type")
                .Type<NonNullType<QuestionTypeObjectType>>();

            descriptor.Field(x => x.Featured)
                .Name("identifying")
                .Type<BooleanType>();

            descriptor.Field("options")
                .Name("options")
                .Resolver(context =>
                    context.BatchDataLoader<int, List<CategoricalOption>>
                    ("optionsByQuestion", async keys =>
                    {
                        var unitOfWork = context.Service<IUnitOfWork>();
                        var questions = await unitOfWork.Session.Query<QuestionnaireCompositeItem>()
                            .Where(q => keys.Contains(q.Id)).ToListAsync();

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
                                    var questionType = q.Item3.GetQuestionType(q.EntityId);
                                    if(questionType == QuestionType.SingleOption)
                                        return q.Item3.GetOptionsForQuestion(q.EntityId, null, null, null).ToList();
                                    return new List<CategoricalOption>();
                                });
                      
                    }).LoadAsync(context.Parent<QuestionnaireCompositeItem>().Id, default))
                .Type<NonNullType<ListType<NonNullType<CategoricalOptionType>>>>();
        }
    }
}
