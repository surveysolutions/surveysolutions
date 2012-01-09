using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Export.csv;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Models;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class QuestionnaireController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
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
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(new QuestionnaireBrowseInputModel());
            return View(model);
        }
       /* public ActionResult Index()
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return View(model);
        }*/
        //
        // GET: /Questionnaire/Details/5

        public ViewResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid quesry string parameters");
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View(model);
        }


        public ViewResult Flow(string id)
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
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
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
                    commandInvoker.Execute(new CreateNewQuestionnaireCommand(model.Title, GlobalInfo.GetCurrentUser()));
                }
                else
                {
                    commandInvoker.Execute(new UpdateQuestionnaireCommand(model.Id, model.Title, GlobalInfo.GetCurrentUser()));
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
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
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
                    ExportManager manager = new ExportManager(provider);

                    CompleteQuestionnaireExportView records =
                        viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(
                            new CompleteQuestionnaireExportInputModel() {PageSize = 100});

                    Dictionary<Guid, string> header = new Dictionary<Guid, string>();

                    foreach (var q in model.Questions)
                    {
                        header.Add(q.PublicKey, string.IsNullOrEmpty(q.StataExportCaption) ? q.QuestionText : q.StataExportCaption);
                    }

                    var stream = manager.ExportToStream(header, records);

                    FileStreamResult fsr = new FileStreamResult(stream, "text/csv") {FileDownloadName = fileName};

                    return fsr;
                }


            }
            return null;

        }

        #endregion
    }
}
