#nullable enable
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class Interview : ObjectType<InterviewSummary>
    {
        protected override void Configure(IObjectTypeDescriptor<InterviewSummary> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("Interview");
            
            descriptor.Field<InterviewActionFlagsResolver>(f => 
                    f.GetActionFlags(default, default))
                .Type<NonNullType<ListType<NonNullType<EnumType<InterviewActionFlags>>>>>()
                .Description("List of actions that can be applied to interview")
                .Name("actionFlags");
            
            descriptor.Field(x => x.AssignmentId)
                .Type<NonNullType<IntType>>()
                .Description("Identifier for the assignment to which this interview belongs");
            
            descriptor.Field(x => x.SummaryId)
                .Name("id")
                .Type<NonNullType<IdType>>();
            
            descriptor.Field(x => x.Status)
                .Type<NonNullType<EnumType<InterviewStatus>>>();

            descriptor.Field(x => x.InterviewMode)
                .Type<NonNullType<EnumType<InterviewMode>>>();

            descriptor.Field(x => x.ResponsibleName).Type<NonNullType<StringType>>()
                .Description("Login of current responsible user");
            
            descriptor.Field(x => x.ResponsibleId).Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.ResponsibleRole).Type<NonNullType<EnumType<UserRoles>>>();

            descriptor.Field(x => x.SupervisorName).Type<StringType>()
                .Description("Supervisor login who is responsible for interview");
            
            descriptor.Field(x => x.WasCompleted)
                .Description("Indicates if interview was ever completed by interviewer")
                .Type<NonNullType<BooleanType>>();
            
            descriptor.Field(x => x.CreatedDate)
                .Type<NonNullType<DateTimeType>>()
                .Description("Date when interview was created");

            descriptor.Field(x => x.Key).Type<NonNullType<StringType>>()  
                .Description("Short case identifier that appears throughout the system - in Headquarters, Supervisor, and Interviewer");

            descriptor.Field(x => x.ClientKey).Type<NonNullType<StringType>>()  
                .Description("Key that was generated on interviewer tablet when interview was created for the first time");

            descriptor.Field(x => x.UpdateDate)
                .Name("updateDateUtc")
                .Type<NonNullType<DateTimeType>>()
                .Description("Represents date (UTC) when interview was changed last time");
            
            /*descriptor
                .Field(x => x.ReceivedByInterviewer)
                .Type<NonNullType<BooleanType>>()
                .Description("Indicator for whether the interview is on the interviewer’s tablet now");*/
                
            descriptor.Field(x => x.ReceivedByInterviewerAtUtc)
                .Type<DateTimeType>()
                .Description("Represents date (UTC) when the interview was received by the interviewer’s tablet");
            
            descriptor.Field(x => x.ErrorsCount)
                .Description("Shows total number of invalid questions and static texts in the interview. Multiple failed validation conditions on same entity are counted as 1")
                .Type<NonNullType<IntType>>();

            descriptor.Field(x => x.QuestionnaireId).Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.QuestionnaireVariable).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.QuestionnaireVersion).Type<NonNullType<LongType>>();
            

            descriptor.Field(x => x.IdentifyEntitiesValues)
                .Name("identifyingData")
                .Description("Information that identifies each assignment. These are the answers to questions marked as identifying in Designer")
                .Resolve(async context => await
                    context.GroupDataLoader<string, IdentifyEntityValue>
                        (async (keys, token) =>
                    {
                        var unitOfWork = context.Service<IUnitOfWork>(); 
                        var questionAnswers = await unitOfWork.Session.Query<IdentifyEntityValue>()
                            .Where(a => keys.Contains(a.InterviewSummary.SummaryId) && a.Identifying)
                            .OrderBy(a => a.Position)
                            .ToListAsync(cancellationToken: token)
                            .ConfigureAwait(false);
                        
                        var answers = questionAnswers
                            .ToLookup(x => x.InterviewSummary.SummaryId);

                        return answers;
                    },"answersByInterview")
                        .LoadAsync(context.Parent<InterviewSummary>().SummaryId, default))
                .Type<ListType<AnswerObjectType>>();

            
            descriptor.Field(x => x.NotAnsweredCount)
                .Description(
                    "Number of questions without answer. Includes supervisor, identifying and interviewer questions. Can contain nulls for interviews that were completed prior to 20.09 release");
            
            descriptor.Field("calendarEvent")
                .Description("Active Calendar Event associated with interview")
                .Type<CalendarEventObjectType>()
                .Resolve(context =>
                {
                    var interviewId = context.Parent<InterviewSummary>().InterviewId;
                    var unitOfWork = context.Service<IUnitOfWork>();
                  
                    var calendarEvent = unitOfWork.Session
                        .Query<CalendarEvent>()
                        .FirstOrDefault(x => x.InterviewId == interviewId 
                                             && x.CompletedAtUtc == null
                                             && x.DeletedAtUtc == null);
                       return calendarEvent;
                });

            descriptor.Field(x => x.InterviewMode)
                .Description("Current mode of interview")
                .Type<NonNullType<EnumType<InterviewMode>>>();
        }
    }
}
