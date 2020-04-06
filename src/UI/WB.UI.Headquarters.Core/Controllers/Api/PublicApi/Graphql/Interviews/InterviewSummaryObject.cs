using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewSummaryObjectType : ObjectType<InterviewSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("Interview");
            
            descriptor.Field(x => x.SummaryId)
                .Name("id")
                .Type<NonNullType<IdType>>();
            
            descriptor.Field(x => x.Status)
                .Type<NonNullType<EnumType<InterviewStatus>>>();
            
            descriptor.Field(x => x.ResponsibleName)
                .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.ResponsibleRole)
                .Type<NonNullType<EnumType<UserRoles>>>();

            descriptor.Field(x => x.AssignmentId)
                .Type<NonNullType<IntType>>();

            descriptor.Field(x => x.CreatedDate)
                .Description("Utc creation date");

            descriptor.Field(x => x.Key)
                .Type<NonNullType<StringType>>();
            
            descriptor.Field(x => x.UpdateDate)
                .Description("Utc last updated date");
            
            descriptor.Field(x => x.ReceivedByInterviewer);
            
            descriptor.Field(x => x.ErrorsCount)
                .Type<NonNullType<IntType>>();

            descriptor.Field(x => x.QuestionnaireId);
            descriptor.Field(x => x.QuestionnaireVersion);
            
            descriptor.Field(x => x.AnswersToFeaturedQuestions)
                .Name("identifyingQuestions")
                .Resolver(context => 
                    context.GroupDataLoader<string, QuestionAnswer>
                        ("answersByInterview", async keys =>
                    {
                        var unitOfWork = context.Service<IUnitOfWork>();
                        var questionAnswers = await unitOfWork.Session.Query<QuestionAnswer>()
                            .Where(a => keys.Contains(a.InterviewSummary.SummaryId))
                            .ToListAsync()
                            .ConfigureAwait(false);
                        var answers = questionAnswers
                            .ToLookup(x => x.InterviewSummary.SummaryId);

                        return answers;
                    }).LoadAsync(context.Parent<InterviewSummary>().SummaryId, default))
                .Type<NonNullType<ListType<NonNullType<AnswerObjectType>>>>();
        }
    }
}
