using System.Threading.Tasks;
using Utils.Commands;
using Utils.CustomInfrastructure;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;

namespace Utils.Setup
{
    public class CoreDebugModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            // TODO: Find a generic place for each of the dependencies below
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();
            registry.BindAsSingleton<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();

            registry.Unbind<ICommandService>();
            registry.BindAsSingleton<ICommandService, CustomCommandService>();

            registry.Bind<IAssemblyService, AssemblyService>();

            registry.BindToMethod<IInterviewAnswerSerializer>(() => new NewtonInterviewAnswerJsonSerializer());

            registry.Bind<IQuestionnaireBrowseViewFactory, QuestionnaireBrowseViewFactory>();

            registry.BindAsSingleton<IQuestionnaireStorage, CustomQuestionnaireStorage>();

            registry.Bind<CoreTestRunner>();
            registry.Bind<DebugInformationDumper>();
            registry.Bind<CoreDebugger>();

            registry.Bind<ISerializer, NewtonJsonSerializer>();
            registry.BindAsSingleton<IQuestionnaireAssemblyAccessor, CustomQuestionnaireAssemblyAccessor>();



            //Designer
            registry.Bind<IExpressionProcessorGenerator, QuestionnaireExpressionProcessorGenerator>();
            registry.Bind<IExpressionsGraphProvider, ExpressionsGraphProvider>();
            registry.Bind<IExpressionsPlayOrderProvider, ExpressionsPlayOrderProvider>();
            registry.Bind<IMacrosSubstitutionService, MacrosSubstitutionService>();

            registry.BindAsSingleton<IExpressionProcessor, RoslynExpressionProcessor>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>(aggregate => aggregate.ImportFromDesigner, config => config.ValidatedBy<QuestionnaireNameValidator>())
                .InitializesWith<RegisterPlainQuestionnaire>(aggregate => aggregate.RegisterPlainQuestionnaire)
                .InitializesWith<DeleteQuestionnaire>(aggregate => aggregate.DeleteQuestionnaire)
                .InitializesWith<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire)
                .InitializesWith<CloneQuestionnaire>(aggregate => aggregate.CloneQuestionnaire);

            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterviewFromSynchronizationMetadata>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSynchronizationMetadata(command.Id, command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.Comments, command.RejectedDateTime, command.InterviewerAssignedDateTime, command.Valid, command.CreatedOnClient, command.OriginDate))
                .InitializesWith<SynchronizeInterviewEventsCommand>(command => command.InterviewId, aggregate => aggregate.SynchronizeInterviewEvents)
                .InitializesWith<CreateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command))

                .StatelessHandles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId, command.OriginDate))

                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerYesNoQuestion(command))
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, aggregate => aggregate.AnswerNumericIntegerQuestion)
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.PictureFileName))
                .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.FileName, command.Length))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.OriginDate))

                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.OriginDate))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.OriginDate))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId,command.SupervisorId,command.OriginDate))
                .Handles<MoveInterviewToTeam>(command => command.InterviewId, (command, aggregate) => aggregate.MoveInterviewToTeam(command))
                .Handles<AssignResponsibleCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignResponsible(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Comment))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId, command.OriginDate))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment, command.OriginDate))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment, command.OriginDate))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment, command.OriginDate))
                .Handles<MarkInterviewAsReceivedByInterviewer>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsReceivedByInterviwer(command.UserId, command.OriginDate))
                .Handles<ReevaluateSynchronizedInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateSynchronizedInterview(command.UserId))
                .Handles<RepeatLastInterviewStatus>(command => command.InterviewId, aggregate => aggregate.RepeatLastInterviewStatus)
                .Handles<RejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Reject(command.UserId, command.Comment, command.OriginDate))
                .Handles<RejectInterviewToInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectToInterviewer(command.UserId, command.InterviewerId, command.Comment, command.OriginDate))
                .Handles<RestartInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restart(command.UserId, command.Comment, command.RestartTime))
                .Handles<RestoreInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restore(command.UserId, command.OriginDate))
                //.Handles<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterview(command.UserId, command.SynchronizedInterview))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CompleteWithoutFirePassiveEvents(command.UserId, command.Comment, command.OriginDate))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation)
                .Handles<PauseInterviewCommand>(cmd => cmd.InterviewId, a => a.Pause)
                .Handles<ResumeInterviewCommand>(cmd => cmd.InterviewId, a => a.Resume)
                .Handles<OpenInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.OpenBySupevisor)
                .Handles<CloseInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.CloseBySupevisor);
            
            CommandRegistry.Configure<StatefulInterview, InterviewCommand>(configuration => 
                configuration
                .PostProcessBy<InterviewSummaryErrorsCountPostProcessor>()
                    .SkipPostProcessFor<HardDeleteInterview>()
                    .SkipPostProcessFor<DeleteInterviewCommand>()
                    .SkipPostProcessFor<MarkInterviewAsReceivedByInterviewer>()
                    .SkipPostProcessFor<AssignInterviewerCommand>()
                    .SkipPostProcessFor<AssignSupervisorCommand>()
                .ValidatedBy<InterviewReceivedByInterviewerCommandValidator>()
                    .SkipValidationFor<SynchronizeInterviewEventsCommand>()
                    .SkipValidationFor<MarkInterviewAsReceivedByInterviewer>()
                    .SkipValidationFor<AssignInterviewerCommand>()
                    .SkipValidationFor<AssignResponsibleCommand>()
                    .SkipValidationFor<AssignSupervisorCommand>()
                    .SkipValidationFor<HardDeleteInterview>()
                    .SkipValidationFor<DeleteInterviewCommand>()
                    .SkipValidationFor<MoveInterviewToTeam>()
            );

            CommandRegistry.Configure<StatefulInterview, SynchronizeInterviewEventsCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<StatefulInterview, CreateInterview>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());

            return Task.CompletedTask;
        }
    }
}
