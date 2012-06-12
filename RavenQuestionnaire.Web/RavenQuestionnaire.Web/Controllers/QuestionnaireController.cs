#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Export.csv;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;
using RavenQuestionnaire.Core.Views.File;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Web.Models;

#endregion

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class QuestionnaireController : Controller
    {
        private readonly ICommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public QuestionnaireController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _TableData(GridDataRequest data)
        {
            var input = new QuestionnaireBrowseInputModel
                            {
                                Page = data.Pager.Page,
                                PageSize = data.Pager.PageSize,
                                Orders = data.SortOrder
                            };
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return PartialView("_Table", model);
        }

        public ViewResult ItemList(QuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return View(model);
        }

        public ActionResult Index()
        {
            var model =
                viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                    new QuestionnaireBrowseInputModel());
            return View(model);
        }

        public ViewResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid quesry string parameters");
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            
            return View(model);
        }

        //
        // GET: /Questionnaire/Create
        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor)]
        public ActionResult Create()
        {
            return View(new QuestionnaireView());
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ViewResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters.");
            var model =
                viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View("Create", model);
        }

        //
        // POST: /Questionnaire/Create

        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionnaireView model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    //maybe better move loading defaults to the handler?
                    var statusDefault =
                        viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel("0"));
                    Guid key = Guid.NewGuid();

                    commandInvoker.Execute(new CreateNewQuestionnaireCommand(model.Title, 
                        statusDefault != null ? statusDefault.Id : null,
                        key,
                        GlobalInfo.GetCurrentUser()));

                    //new fw
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(new CreateQuestionnaireCommand(key, model.Title));
                
                }
                else
                {
                    commandInvoker.Execute(new UpdateQuestionnaireCommand(model.Id, model.Title,
                                                                          GlobalInfo.GetCurrentUser()));
                }
                return RedirectToAction("Index");
            }
            return View("Create", model);
        }


        //
        // GET: /Questionnaire/Delete/5
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteQuestionnaireCommand(id, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }

        #region export

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Export(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid quesry string parameters");
            var model =
                viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult GetExportedData(string id, string type)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(type))
                throw new HttpException(404, "Invalid quesry string parameters");
            var model =
                viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));

            if (model != null)
            {
                var fileName = string.Format("exported{0}.csv", DateTime.Now.ToLongTimeString());

                if (type == "csv" || type == "tab")
                {
                    IExportProvider provider = new CSVExporter(type == "csv" ? ',' : '\t');
                    var manager = new ExportManager(provider);

                    var records =
                        viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(
                            new CompleteQuestionnaireExportInputModel {PageSize = 100, QuestionnaryId = model.Id});


                    var header = new Dictionary<Guid, string>();

                    foreach (var q in model.Questions)
                    {
                        header.Add(q.PublicKey,
                                   string.IsNullOrEmpty(q.StataExportCaption) ? q.Title : q.StataExportCaption);
                    }

                    foreach (var group in model.Groups)
                    {
                        foreach (var q in group.Questions)
                        {
                            header.Add(q.PublicKey,
                                       string.IsNullOrEmpty(q.StataExportCaption)
                                           ? q.Title
                                           : q.StataExportCaption);
                        }
                    }

                    var stream = manager.ExportToStream(header, records);

                    var fsr = new FileStreamResult(stream, "text/csv") {FileDownloadName = fileName};

                    return fsr;
                }
            }
            return null;
        }

        #endregion
    }
}