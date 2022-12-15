using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.Enumerator.Native.WebInterview.Controllers
{
    [HandleCommandError]
    public abstract class CommandsController : ControllerBase
    {
        protected readonly ICommandService commandService;
        protected readonly IImageFileStorage imageFileStorage;
        protected readonly IAudioFileStorage audioFileStorage;
        protected readonly IQuestionnaireStorage questionnaireRepository;
        protected readonly IStatefulInterviewRepository statefulInterviewRepository;
        protected readonly IWebInterviewNotificationService webInterviewNotificationService;

        public CommandsController(ICommandService commandService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage,
            IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewNotificationService webInterviewNotificationService)
        {
            this.commandService = commandService;
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
            this.questionnaireRepository = questionnaireRepository;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewNotificationService = webInterviewNotificationService;
        }

        protected virtual Guid GetCommandResponsibleId(Guid interviewId)
        {
            var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            return interview.CurrentResponsibleId;
        }

        public virtual IActionResult ChangeLanguage(Guid interviewId, ChangeLanguageRequest request)
        {
            this.commandService.Execute(new SwitchTranslation(interviewId, request.Language, this.GetCommandResponsibleId(interviewId)));
            return Ok();
        }

        public class AnswerRequest
        {
            public string Identity { get; set; }
        }

        public class AnswerRequest<T> : AnswerRequest
        {
            public T Answer { get; set; }
        }

        public virtual IActionResult AnswerTextQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerTextQuestionCommand(interviewId,
                this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public virtual IActionResult AnswerTextListQuestion(Guid interviewId, [FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerTextListQuestionCommand(interviewId,
                this.GetCommandResponsibleId(interviewId), 
                identity.Id, identity.RosterVector, 
                answerRequest.Answer.Select(row => new Tuple<decimal, string>(row.Value, row.Text)).ToArray()));
            return Ok();
        }

        public virtual IActionResult AnswerGpsQuestion(Guid interviewId, [FromBody] AnswerRequest<GpsAnswer> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerGeoLocationQuestionCommand(interviewId,
                this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, answerRequest.Answer.Latitude, answerRequest.Answer.Longitude,
                answerRequest.Answer.Accuracy ?? 0, answerRequest.Answer.Altitude ?? 0, DateTimeOffset.FromUnixTimeMilliseconds(answerRequest.Answer.Timestamp ?? 0)));
            return Ok();
        }

        public virtual IActionResult AnswerDateQuestion(Guid interviewId, [FromBody] AnswerRequest<DateTime> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerDateTimeQuestionCommand(interviewId,
                this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public virtual IActionResult AnswerSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerSingleOptionQuestionCommand(interviewId, GetCommandResponsibleId(interviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerSingleOptionLinkedQuestionCommand(interviewId, GetCommandResponsibleId(interviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IActionResult AnswerLinkedMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[][]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerMultipleOptionsLinkedQuestionCommand(interviewId, GetCommandResponsibleId(interviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer.Select(x => new RosterVector(x)).ToArray()));
            return Ok();
        }

        public virtual IActionResult AnswerMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerMultipleOptionsQuestionCommand(interviewId, GetCommandResponsibleId(interviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public virtual IActionResult AnswerYesNoQuestion(Guid interviewId, [FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            var answer = answerRequest.Answer.Select(a => new AnsweredYesNoOption(a.Value, a.Yes)).ToArray();
            this.ExecuteQuestionCommand(new AnswerYesNoQuestion(interviewId, GetCommandResponsibleId(interviewId),
                identity.Id, identity.RosterVector, answer));
            return Ok();
        }

        public virtual IActionResult AnswerIntegerQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerNumericIntegerQuestionCommand(interviewId, this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IActionResult AnswerDoubleQuestion(Guid interviewId, [FromBody] AnswerRequest<double> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerNumericRealQuestionCommand(interviewId, this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IActionResult AnswerQRBarcodeQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerQRBarcodeQuestionCommand(interviewId,
                this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public class RemoveAnswerRequest : AnswerRequest
        {
        }

        [ObservingNotAllowed]
        public virtual IActionResult RemoveAnswer(Guid interviewId, RemoveAnswerRequest request)
        {
            Identity identity = Identity.Parse(request.Identity);

            try
            {
                var interview = statefulInterviewRepository.Get(interviewId.FormatGuid());
                var questionnaire = questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, null);
                var questionType = questionnaire.GetQuestionType(identity.Id);

                if (questionType == QuestionType.Multimedia)
                {
                    var fileName = $@"{questionnaire.GetQuestionVariableName(identity.Id)}{string.Join(@"-", identity.RosterVector.Select(rv => rv))}.jpg";
                    this.imageFileStorage.RemoveInterviewBinaryData(interviewId, fileName);
                }
                else if (questionType == QuestionType.Audio)
                {
                    var fileName = $@"{questionnaire.GetQuestionVariableName(identity.Id)}__{identity.RosterVector}.m4a";
                    this.audioFileStorage.RemoveInterviewBinaryData(interviewId, fileName);
                }
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, identity, e);
            }

            this.ExecuteQuestionCommand(new RemoveAnswerCommand(interviewId, GetCommandResponsibleId(interviewId), identity));
            return Ok();
        }

        [ObservingNotAllowed]
        public abstract IActionResult CompleteInterview(Guid interviewId, CompleteInterviewRequest completeInterviewRequest);

        public class NewCommentRequest : AnswerRequest
        {
            public string Comment { get; set; }
        }

        [ObservingNotAllowed]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IActionResult SendNewComment(Guid interviewId, NewCommentRequest request)
        {
            var identity = Identity.Parse(request.Identity);
            var command = new CommentAnswerCommand(interviewId, this.GetCommandResponsibleId(interviewId), identity.Id, identity.RosterVector, request.Comment);

            this.commandService.Execute(command);
            return Ok();
        }

        [ObservingNotAllowed]
        protected void ExecuteQuestionCommand(QuestionCommand command)
        {
            try
            {
                commandService.Execute(command);
            }
            catch (InterviewException ie) when (ie.ExceptionType == InterviewDomainExceptionType.AssignmentLimitReached)
            {
                webInterviewNotificationService.ReloadInterview(command.InterviewId);
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(command.InterviewId, command.Question, e);
            }
        }
    }
}
