using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.Enumerator.Native.WebInterview.Controllers
{
    public abstract class CommandsController : ApiController
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

        public virtual IHttpActionResult ChangeLanguage(ChangeLanguageRequest request)
        {
            this.commandService.Execute(new SwitchTranslation(request.InterviewId, request.Language, this.GetCommandResponsibleId(request.InterviewId)));
            return Ok();
        }

        public class AnswerRequest<T>
        {
            public Guid InterviewId { get; set; }
            public string Identity { get; set; }
            public T Answer { get; set; }
        }

        public virtual IHttpActionResult AnswerTextQuestion([FromBody] AnswerRequest<string> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerTextQuestionCommand(answerRequest.InterviewId,
                this.GetCommandResponsibleId(answerRequest.InterviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public virtual IHttpActionResult AnswerTextListQuestion([FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerTextListQuestionCommand(answerRequest.InterviewId,
                this.GetCommandResponsibleId(answerRequest.InterviewId), 
                identity.Id, identity.RosterVector, 
                answerRequest.Answer.Select(row => new Tuple<decimal, string>(row.Value, row.Text)).ToArray()));
            return Ok();
        }

        public virtual IHttpActionResult AnswerGpsQuestion([FromBody] AnswerRequest<GpsAnswer> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerGeoLocationQuestionCommand(answerRequest.InterviewId,
                this.GetCommandResponsibleId(answerRequest.InterviewId), identity.Id, identity.RosterVector, answerRequest.Answer.Latitude, answerRequest.Answer.Longitude,
                answerRequest.Answer.Accuracy ?? 0, answerRequest.Answer.Altitude ?? 0, DateTimeOffset.FromUnixTimeMilliseconds(answerRequest.Answer.Timestamp ?? 0)));
            return Ok();
        }

        public virtual IHttpActionResult AnswerDateQuestion([FromBody] AnswerRequest<DateTime> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerDateTimeQuestionCommand(answerRequest.InterviewId,
                this.GetCommandResponsibleId(answerRequest.InterviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public virtual IHttpActionResult AnswerSingleOptionQuestion([FromBody] AnswerRequest<int> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerSingleOptionQuestionCommand(answerRequest.InterviewId, GetCommandResponsibleId(answerRequest.InterviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IHttpActionResult AnswerLinkedSingleOptionQuestion([FromBody] AnswerRequest<decimal[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerSingleOptionLinkedQuestionCommand(answerRequest.InterviewId, GetCommandResponsibleId(answerRequest.InterviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IHttpActionResult AnswerLinkedMultiOptionQuestion([FromBody] AnswerRequest<decimal[][]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerMultipleOptionsLinkedQuestionCommand(answerRequest.InterviewId, GetCommandResponsibleId(answerRequest.InterviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer.Select(x => new RosterVector(x)).ToArray()));
            return Ok();
        }

        public virtual IHttpActionResult AnswerMultiOptionQuestion([FromBody] AnswerRequest<int[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerMultipleOptionsQuestionCommand(answerRequest.InterviewId, GetCommandResponsibleId(answerRequest.InterviewId),
                identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        public virtual IHttpActionResult AnswerYesNoQuestion([FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            var answer = answerRequest.Answer.Select(a => new AnsweredYesNoOption(a.Value, a.Yes)).ToArray();
            this.ExecuteQuestionCommand(new AnswerYesNoQuestion(answerRequest.InterviewId, GetCommandResponsibleId(answerRequest.InterviewId),
                identity.Id, identity.RosterVector, answer));
            return Ok();
        }

        public virtual IHttpActionResult AnswerIntegerQuestion([FromBody] AnswerRequest<int> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerNumericIntegerQuestionCommand(answerRequest.InterviewId, this.GetCommandResponsibleId(answerRequest.InterviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IHttpActionResult AnswerDoubleQuestion([FromBody] AnswerRequest<double> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerNumericRealQuestionCommand(answerRequest.InterviewId, this.GetCommandResponsibleId(answerRequest.InterviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IHttpActionResult AnswerQRBarcodeQuestion([FromBody] AnswerRequest<string> answerRequest)
        {
            var identity = Identity.Parse(answerRequest.Identity);
            this.ExecuteQuestionCommand(new AnswerQRBarcodeQuestionCommand(answerRequest.InterviewId,
                this.GetCommandResponsibleId(answerRequest.InterviewId), identity.Id, identity.RosterVector, answerRequest.Answer));
            return Ok();
        }

        [ObserverNotAllowed]
        public virtual IHttpActionResult RemoveAnswer(Guid interviewId, string questionId)
        {
            Identity identity = Identity.Parse(questionId);

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

        [ObserverNotAllowed]
        public abstract IHttpActionResult CompleteInterview(CompleteInterviewRequest completeInterviewRequest);

        public class NewCommentRequest
        {
            public Guid InterviewId { get; set; }
            public string QuestionId { get; set; }
            public string Comment { get; set; }
        }

        [ObserverNotAllowed]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        public virtual IHttpActionResult SendNewComment(NewCommentRequest request)
        {
            var identity = Identity.Parse(request.QuestionId);
            var command = new CommentAnswerCommand(request.InterviewId, this.GetCommandResponsibleId(request.InterviewId), identity.Id, identity.RosterVector, request.Comment);

            this.commandService.Execute(command);
            return Ok();
        }

        [ObserverNotAllowed]
        protected void ExecuteQuestionCommand(QuestionCommand command)
        {
            try
            {
                InScopeExecutor.Current.Execute(sl =>
                {
                    sl.GetInstance<ICommandService>().Execute(command);
                });
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(command.InterviewId, command.Question, e);
            }
        }
    }
}
