#nullable enable
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Humanizer;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
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
                .Type<IdType>();
            
            descriptor.Field(x => x.Status)
                .Type<EnumType<InterviewStatus>>();
            
            descriptor.Field(x => x.ResponsibleName).Type<StringType>()
                .Description("Login of current responsible user");
            
            descriptor.Field(x => x.ResponsibleNameLowerCase).Type<StringType>()
                .Description($"Lower cased version of {nameof(InterviewSummary.ResponsibleName).Camelize()} field");

            descriptor.Field(x => x.ResponsibleId).Type<UuidType>();
            descriptor.Field(x => x.ResponsibleRole).Type<EnumType<UserRoles>>();

            descriptor.Field(x => x.TeamLeadName).Type<StringType>()
                .Description("Supervisor login who is responsible for interview");
            descriptor.Field(x => x.ReceivedByInterviewer)
                .Type<StringType>();
            descriptor.Field(x => x.WasCompleted)
                .Description("Indicates if interview was ever completed by interviewer")
                .Type<StringType>();
            descriptor.Field(x => x.TeamLeadNameLowerCase)
                .Description("Lowercased version of team lead name")
                .Type<StringType>();

            descriptor.Field(x => x.AssignmentId).Type<IntType>()
                  .Description("Identifier for the assignment to which this interview belongs");

            descriptor.Field(x => x.CreatedDate)
                .Description("Utc creation date");

            descriptor.Field(x => x.Key).Type<StringType>()  
                .Description("Short case identifier that appears throughout the system - in Headquarters, Supervisor, and Interviewer.");
            
            descriptor.Field(x => x.UpdateDate)
                .Description("Represents date (UTC) when interview was changed last time");
            
            descriptor.Field(x => x.ReceivedByInterviewer).Type<BooleanType>()
                .Description("Indicator for whether the interview is on the interviewer’s tablet now");
            
            descriptor.Field(x => x.ErrorsCount)
                .Description("Shows total number of invalid questions and static texts in the interview. Multiple failed validation conditions on same entity are counted as 1.")
                .Type<IntType>();

            descriptor.Field(x => x.QuestionnaireId);
            descriptor.Field(x => x.QuestionnaireVersion);
            
            descriptor.Field(x => x.AnswersToFeaturedQuestions)
                .Name("identifyingQuestions")
                .Description("Information that identifies each assignment. These are the answers to questions marked as identifying in Designer.")
                .Resolver(context => 
                    context.GroupDataLoader<string, QuestionAnswer>
                        ("answersByInterview", async keys =>
                    {
                        var unitOfWork = context.Service<IUnitOfWork>();
                        var questionAnswers = await unitOfWork.Session.Query<QuestionAnswer>()
                            .Where(a => keys.Contains(a.InterviewSummary.SummaryId))
                            .OrderBy(a => a.Position)
                            .ToListAsync()
                            .ConfigureAwait(false);
                        
                        var answers = questionAnswers
                            .ToLookup(x => x.InterviewSummary.SummaryId);

                        return answers;
                    }).LoadAsync(context.Parent<InterviewSummary>().SummaryId, default))
                .Type<ListType<AnswerObjectType>>();

            descriptor.Field<InterviewActionFlagsResolver>(f => f.GetActionFlags(default, default))
                .Description("List of actions that can be applied to interview")
                .Name("actionFlags");
        }
    }
}
