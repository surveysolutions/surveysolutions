using System.Linq;
using GreenDonut;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class AnswerObjectType : ObjectType<IdentifyEntityValue>
    {
        protected override void Configure(IObjectTypeDescriptor<IdentifyEntityValue> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            
            descriptor.Name("IdentifyingEntity");
            
            descriptor.Field(x => x.Value)
                .Name("value")
                .Type<StringType>();

            descriptor.Field(x => x.ValueLowerCase)
                .Description("Lower cased version of answer")
                .Type<StringType>();
            
            descriptor.Field(x => x.AnswerCode)
                .Type<IntType>()
                .Name("answerValue")
                .Description("Answer value for categorical questions");

            descriptor.Field(x => x.Entity)
                .Name("entity")
                .Resolver(context => {
                        var parent = context.Parent<IdentifyEntityValue>();

                        return context.BatchDataLoader<int, QuestionnaireCompositeItem>("questionByAnswer", async keys =>
                        {
                            var unitOfWork = context.Service<IUnitOfWork>();
                            var items = await unitOfWork.Session.Query<QuestionnaireCompositeItem>()
                                .Where(q => keys.Contains(q.Id))
                                .ToListAsync()
                                .ConfigureAwait(false);
                            return items.ToDictionary(x => x.Id);
                        }).LoadAsync(parent.Entity.Id);
                    }
                )
                .Type<NonNullType<EntityItemObjectType>>();
        }
    }
}
