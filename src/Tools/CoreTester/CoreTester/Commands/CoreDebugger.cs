using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreTester.CustomInfrastructure;
using Main.Core.Documents;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace CoreTester.Commands
{
    public class CoreDebugger
    {
        private readonly ISerializer serializer;
        private readonly IQuestionnaireAssemblyAccessor assemblyAccessor;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICommandService commandService;

        public CoreDebugger(
            ISerializer serializer, 
            IQuestionnaireAssemblyAccessor assemblyAccessor, 
            IQuestionnaireStorage questionnaireStorage, 
            ICommandService commandService)
        {
            this.serializer = serializer;
            this.assemblyAccessor = assemblyAccessor;
            this.questionnaireStorage = questionnaireStorage;
            this.commandService = commandService;
        }

        public int Run(string folder)
        {
            int result = 1;

            var directories = Directory.EnumerateDirectories(folder).ToList();
            if (!directories.Any())
                result *= RunForQuestionnire(folder);

            foreach (var directory in directories)
            {
                result *= Run(directory);
            }

            return result;
        }

        public int RunForQuestionnire(string folder)
        {
            Console.WriteLine($"Analize folder {folder}");

            var files = Directory.EnumerateFiles(folder).ToList();

            if (!files.Any())
            {
                Console.WriteLine("Empty folder. Check -f param");
                return 0;
            }

            var questionnairePrefix = "questionnaire-";
            var questionnaireJsonFileName = files.Single(x => Path.GetFileName(x).StartsWith(questionnairePrefix));
            var questionnaireIdentity = QuestionnaireIdentity.Parse(Path.GetFileNameWithoutExtension(questionnaireJsonFileName).Substring(questionnairePrefix.Length));
            var questionnaireDocument = serializer.Deserialize<QuestionnaireDocument>(File.ReadAllText(questionnaireJsonFileName));
            questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            if (questionnaireDocument == null)
            {
                Console.WriteLine("Can't deserialize questionnaire");
                return 0;
            }

            var assemblyDllFileName = files.Single(x => Path.GetFileName(x).StartsWith("assembly-"));
            assemblyAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, File.ReadAllBytes(assemblyDllFileName));

            foreach (var file in files)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
//                if (!fileNameWithoutExtension.Contains("1fcb900d92e74de286eb119df741909d"))
//                    continue;

                if (Guid.TryParse(fileNameWithoutExtension, out Guid interviewId))
                {
                    Console.WriteLine($"Process interviewId {interviewId}, in file {fileNameWithoutExtension}");

                    var events = serializer.Deserialize<List<CommittedEvent>>(File.ReadAllText(file));
                    CreateInterviewAndApplyEvents(interviewId, events);
                }
            }

            return 1;
        }

        private void CreateInterviewAndApplyEvents(Guid interviewId, List<CommittedEvent> committedEvents)
        {
            var userId = Guid.Parse("22222222222222222222222222222222");
            var createCommand = EventsToCommandConverter.GetCreateInterviewCommand(committedEvents, interviewId, userId);
            commandService.Execute(createCommand);

            for (int i = 0; i < committedEvents.Count; i++)
            {
                var committedEvent = committedEvents[i];

                var commands = EventsToCommandConverter.ConvertEventToCommands(interviewId, committedEvent)?.ToList();

                if (commands == null)
                    continue;

                try
                {
                    foreach (var command in commands)
                    {
                        if (command is RemoveAnswerCommand)
                        {
                            try
                            {
                                commandService.Execute(command);
                            }
                            catch (InterviewException exception)
                            {
                                if (!(exception.Message.Contains(
                                          "is disabled and question's answer cannot be changed") ||
                                      exception.Message.Contains("No questions found for roster vector")))
                                {
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            commandService.Execute(command);
                        }
                    }
                }
                catch (InterviewException exception)
                {
                    var message = exception.ExceptionType ==
                                  InterviewDomainExceptionType.ExpessionCalculationError
                        ? $"Calculation error! IN: {interviewId}. Event: {committedEvent.EventSequence} / {committedEvents.Count}"
                        : $"General error! IN: {interviewId}. Event: {committedEvent.EventSequence} / {committedEvents.Count}";

                    Console.WriteLine(message);

                    break;
                }
            }
        }
    }
}
