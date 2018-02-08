using CoreTester.Commands;
using CoreTester.CustomInfrastructure;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
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

namespace CoreTester.Setup
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

            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterview>(command => command.InterviewId,
                    (command, aggregate) => aggregate.CreateInterview(command))
                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Latitude, command.Longitude, command.Accuracy,
                        command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId,
                        command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId,
                    aggregate => aggregate.AnswerYesNoQuestion)
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId,
                    aggregate => aggregate.AnswerNumericIntegerQuestion)
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.PictureFileName))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId,
                        command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.FileName, command.Length))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector,
                        command.UserId, command.RemoveTime))
                .Handles<AnswerAreaQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerAreaQuestion(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId,
                        command.RosterVector, command.CommentTime, command.Comment))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.Complete(command.UserId, command.Comment, command.CompleteTime))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation);

            //Designer
            registry.Bind<IExpressionProcessorGenerator, QuestionnaireExpressionProcessorGenerator>();
            registry.Bind<IExpressionsGraphProvider, ExpressionsGraphProvider>();
            registry.Bind<IExpressionsPlayOrderProvider, ExpressionsPlayOrderProvider>();
            registry.Bind<IMacrosSubstitutionService, MacrosSubstitutionService>();

            registry.BindAsSingleton<IExpressionProcessor, RoslynExpressionProcessor>();
        }
    }
}