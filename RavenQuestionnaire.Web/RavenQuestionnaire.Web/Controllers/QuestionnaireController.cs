#region

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Main.Core.View;
using Main.Core.View.Questionnaire;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using Main.Core.Commands.Questionnaire;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Export.csv;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Models;

#endregion

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class QuestionnaireController : Controller
    {
        private readonly IViewRepository viewRepository;

        public QuestionnaireController(IViewRepository viewRepository)
        {
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

        public ViewResult Details(Guid id)
        {
            if (id == Guid.Empty)
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
        public ViewResult Edit(Guid id)
        {
            if (id == Guid.Empty)
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
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                if (model.PublicKey == Guid.Empty)
                {
                    //maybe better move loading defaults to the handler?
                   
                    Guid key = Guid.NewGuid();
                    //new fw
                    
                    commandService.Execute(new CreateQuestionnaireCommand(key, model.Title));
                
                }
                else
                {

                    commandService.Execute(new UpdateQuestionnaireCommand(model.PublicKey, model.Title));
                }
                return RedirectToAction("Index");
            }
            return View("Create", model);
        }

        /*
        //
        // GET: /Questionnaire/Delete/5
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteQuestionnaireCommand(id, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }*/

        #region export

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Export(Guid id)
        {
            if (id== Guid.Empty)
                throw new HttpException(404, "Invalid quesry string parameters");
            var model =
                viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult GetExportedData(Guid id, string type)
        {
            if ((id == null) || (id == Guid.Empty) || string.IsNullOrEmpty(type))
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
                            new CompleteQuestionnaireExportInputModel {PageSize = 100, QuestionnaryId = model.PublicKey});


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