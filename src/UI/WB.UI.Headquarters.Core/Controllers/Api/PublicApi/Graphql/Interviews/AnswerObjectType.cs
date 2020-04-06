using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
    public class AnswerObjectType : ObjectType<QuestionAnswer>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionAnswer> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            
            descriptor.Field(x => x.Answer)
                .Type<StringType>();

            descriptor.Field(x => x.AnswerCode)
                .Type<IntType>()
                .Name("answerValue")
                .Description("Answer value for categorical questions");

            descriptor.Field(x => x.Question)
                .Resolver(context => {
                    var parent = context.Parent<QuestionAnswer>();

                    return context.BatchDataLoader<int, QuestionnaireCompositeItem>("questionByAnswer", async keys =>
                    {
                        var unitOfWork = context.Service<IUnitOfWork>();
                        var items = await unitOfWork.Session.Query<QuestionnaireCompositeItem>()
                            .Where(q => keys.Contains(q.Id))
                            .ToListAsync()
                            .ConfigureAwait(false);
                        return items.ToDictionary(x => x.Id);
                    }).LoadAsync(parent.Question.Id);
                    }
                )
                .Type<NonNullType<QuestionItemObjectType>>();
        }
    }
}
