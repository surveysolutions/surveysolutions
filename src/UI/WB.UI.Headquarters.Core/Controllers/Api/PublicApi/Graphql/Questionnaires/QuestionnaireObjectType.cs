#nullable enable
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class Questionnaire : ObjectType<QuestionnaireBrowseItem>
    {
        protected override void Configure(IObjectTypeDescriptor<QuestionnaireBrowseItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Name("Questionnaire");
            descriptor.Field(x => x.Variable).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.QuestionnaireId);
            descriptor.Field(x => x.Version);
            descriptor.Field(x => x.Id)
                .Type<IdType>();
            descriptor.Field(x => x.Title).Type<NonNullType<StringType>>();
            
            descriptor.Field("translations")
                .Resolver(ctx =>
                {
                    var questionnaireStorage = ctx.Service<IQuestionnaireStorage>();
                    var browseItem = ctx.Parent<QuestionnaireBrowseItem>();
                    var questionnaire = questionnaireStorage.GetQuestionnaire(browseItem.Identity(), null);
                    return questionnaire.Translations;
                });
        }
    }
}
