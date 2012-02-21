using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Views.FlowGraph;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Models;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class FlowGraphController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public FlowGraphController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        public ActionResult Index()
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(new QuestionnaireBrowseInputModel());
            return View(model);
        }

        public ViewResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid quesry string parameters");

            var model = viewRepository.Load<FlowGraphViewInputModel, FlowGraphView>(new FlowGraphViewInputModel(id));

            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _SaveFlow(string questionnaireId, List<FlowGraphClient> graphs)
        {
            var blocks = graphs.SelectMany(graph => graph.Blocks).Select(b => b.Convert()).ToList();
            var connections = graphs.SelectMany(graph => graph.Connections).Select(c => c.Convert()).ToList();
            try
            {
                commandInvoker.Execute(new UpdateQuestionnaireFlowCommand(questionnaireId, blocks, connections, GlobalInfo.GetCurrentUser()));
            }
            catch (Exception e)
            {
                return Json(new { status = "not saved" });
            }

            var conditions = GetConditions(graphs);

            if (conditions.Count > 0)
            {
                commandInvoker.Execute(new UpdateConditionsCommand(questionnaireId, conditions, GlobalInfo.GetCurrentUser()));
            }

            return Json(new { status = "flow saved" });
        }

        public static Dictionary<Guid, string> GetConditions(List<FlowGraphClient> graphs)
        {
            var blocks = graphs.SelectMany(graph => graph.Blocks).ToList();

            foreach (var graph in graphs)
            {
                if (graph.ParentPublicKey.HasValue)
                {
                    var block = blocks.SingleOrDefault(b => b.PublicKey == graph.ParentPublicKey.Value);
                    if (block != null)
                        block.Graphs.Add(graph);
                }
            }
            foreach (var graph in graphs.Where(g => !g.ParentPublicKey.HasValue))
            {
                graph.Calc(string.Empty);
            }

            return blocks.Where(b => b.IsQuestion).ToDictionary(b => b.PublicKey, b => b.Condition);
        }
    }
}
