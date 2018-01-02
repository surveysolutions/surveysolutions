using System.Collections.Generic;
using Newtonsoft.Json;
using Refit;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Storage;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;
using RestService = Refit.RestService;

namespace WB.UI.WebTester
{
    public class WebTesterModule : IModule
    {
        private static string DesignerAddress()
        {
            var baseAddress = System.Configuration.ConfigurationManager.AppSettings["DesignerAddress"];
            return $"{baseAddress.TrimEnd('/')}";
        }

        public void Load(IIocRegistry registry)
        {
            registry.Bind<IEventSourcedAggregateRootRepository, WebTesterAggregateRootRepository>();
            registry.BindToMethod<IServiceLocator>(() => ServiceLocator.Current);
            registry.BindAsSingleton<IAggregateRootCacheCleaner, DummyAggregateRootCacheCleaner>();
            registry.BindToMethod<IDesignerWebTesterApi>(() => RestService.For<IDesignerWebTesterApi>(DesignerAddress(),
                new RefitSettings
                {
                    JsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
                        Binder = new OldToNewAssemblyRedirectSerializationBinder()
                    }
                }));

            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<ITranslationStorage, TranslationsStorage>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();

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
        }
    }
}