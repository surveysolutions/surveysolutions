using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Web.Models;

namespace RavenQuestionnaire.Web.Controllers
{
    public class CompleteQuestionnaireAsync1Controller : AsyncController
    {
        private readonly IBagManager _bagManager;
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly ICommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public CompleteQuestionnaireAsync1Controller(ICommandInvoker commandInvoker, IViewRepository viewRepository,
                                                    IBagManager bagManager, IGlobalInfoProvider globalProvider)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            _bagManager = bagManager;
            _globalProvider = globalProvider;
        }

        public void IndexAsync(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            AsyncManager.OutstandingOperations.Increment();
            var user = _globalProvider.GetCurrentUser();
            AsyncQuestionnaireUpdater.Update(() =>
                                                 {
                                                     AsyncManager.Parameters["result"] = SaveSingleResultM(/*commandInvoker,viewRepository, */settings,
                                                                                                           questions, user);
                                                     AsyncManager.OutstandingOperations.Decrement();
                                                 });
        }

        public ActionResult IndexCompleted(CompleteQuestionnaireMobileView result)
        {
            return PartialView("~/Views/Group/_ScreenHtml5.cshtml", result);
        }

        protected CompleteQuestionnaireMobileView SaveSingleResultM(/*ICommandInvoker commandInvokerCurrent, IViewRepository viewRepositoryCurrent,**/ CompleteQuestionSettings[] settings,
                                                                    CompleteQuestionView[] questions, UserLight user)
        {

            var question = questions[0];
            try
            {
                commandInvoker.Execute(
                    new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                   question.Answers,
                                                                   settings[0].PropogationPublicKey,user
                                                                   ));
            }
            catch (Exception e)
            {
                ModelState.AddModelError(
                    "questions[" + question.PublicKey +
                    (settings[0].PropogationPublicKey.HasValue
                         ? string.Format("_{0}", settings[0].PropogationPublicKey.Value)
                         : "") + "].AnswerValue", e.Message);
            }


            return viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId)
                    {CurrentGroupPublicKey = settings[0].ParentGroupPublicKey});


        }
    }
}
