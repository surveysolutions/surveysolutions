﻿using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.SharedKernels.Enumerator
{
    public class EnumeratorSharedKernelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IInterviewViewModelFactory>().To<InterviewViewModelFactory>().InSingletonScope();
            this.Bind<IStatefulInterviewRepository>().To<StatefulInterviewRepository>().InSingletonScope();
            this.Bind<ISideBarSectionViewModelsFactory>().To<SideBarSectionViewModelFactory>();
            this.Bind<IRosterTitleSubstitutionService>().To<RosterTitleSubstitutionService>();

            this.Bind<IQuestionnaireModelBuilder>().To<QuestionnaireModelBuilder>().InSingletonScope();

            this.Bind<ISubstitutionService>().To<SubstitutionService>();
            this.Bind<IAnswerToStringService>().To<AnswerToStringService>();

            this.Bind<IInterviewExpressionStateUpgrader>().To<InterviewExpressionStateUpgrader>().InSingletonScope();
            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();

            this.Bind<IFriendlyErrorMessageService>().To<FriendlyErrorMessageService>().InSingletonScope();

            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterviewFromSynchronizationMetadata>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSynchronizationMetadata(command.Id, command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.Comments, command.RejectedDateTime, command.Valid, command.CreatedOnClient))
                .InitializesWith<CreateInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command.QuestionnaireId, command.QuestionnaireVersion, command.SupervisorId, command.AnswersToFeaturedQuestions, command.AnswersTime, command.UserId))
                .InitializesWith<CreateInterviewCreatedOnClientCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewCreatedOnClient(command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.IsValid, command.UserId))
                .InitializesWith<CreateInterviewForTestingCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewForTesting(command.QuestionnaireId, command.AnswersToFeaturedQuestions, command.AnswersTime, command.UserId))
                .InitializesWith<CreateInterviewOnClientCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewOnClient(command.QuestionnaireId, command.QuestionnaireVersion, command.SupervisorId, command.AnswersTime, command.UserId))
                .InitializesWith<CreateInterviewWithPreloadedData>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewWithPreloadedData(command.QuestionnaireId, command.Version, command.PreloadedData, command.SupervisorId, command.AnswersTime, command.UserId))
                .InitializesWith<SynchronizeInterviewFromHeadquarters>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterviewFromHeadquarters(command.Id, command.UserId, command.SupervisorId, command.InterviewDto, command.SynchronizationTime))
                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValues))
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericIntegerQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.PictureFileName))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.RemoveTime))
               
                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.ApproveTime))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.AssignTime))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId, command.SupervisorId))
                .Handles<CancelInterviewByHqSynchronizationCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CancelByHQSynchronization(command.UserId))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.CommentTime, command.Comment))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Complete(command.UserId, command.Comment, command.CompleteTime))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId))
                .Handles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment))
                .Handles<MarkInterviewAsSentToHeadquarters>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsSentToHeadquarters(command.UserId))
                .Handles<ReevaluateSynchronizedInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateSynchronizedInterview())
                .Handles<RejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Reject(command.UserId, command.Comment, command.RejectTime))
                .Handles<RejectInterviewFromHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectInterviewFromHeadquarters(command.UserId, command.SupervisorId, command.InterviewerId, command.InterviewDto, command.SynchronizationTime))
                .Handles<RemoveFlagFromAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveFlagFromAnswer(command.UserId, command.QuestionId, command.RosterVector))
                .Handles<RestartInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restart(command.UserId, command.Comment, command.RestartTime))
                .Handles<RestoreInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restore(command.UserId))
                .Handles<SetFlagToAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SetFlagToAnswer(command.UserId, command.QuestionId, command.RosterVector))
                .Handles<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RestoreInterviewStateFromSyncPackage(command.UserId, command.SynchronizedInterview));
        }
    }
}
