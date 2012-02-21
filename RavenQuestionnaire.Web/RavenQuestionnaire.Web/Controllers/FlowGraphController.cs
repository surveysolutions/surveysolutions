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
        public ActionResult _SaveFlow(string questionnaireId, List<FlowGraph> graphs)
        {
            var blocks = graphs.SelectMany(graph => graph.Blocks).ToList();
            var connections = graphs.SelectMany(graph => graph.Connections).ToList();
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

        public static Dictionary<Guid, string> GetConditions(List<FlowGraph> graphs)
        {
            var conditions = new Dictionary<Guid, string>();
            var parents = new List<Guid>();

            foreach (var graph in graphs)
            {
                foreach (var block in graph.Blocks)
                {
                    var inputs = graph.Connections.Where(c => c.Target == block.PublicKey).ToList();
                    var condition = string.Empty;
                    if (graph.ParentPublicKey.HasValue && !parents.Contains(graph.ParentPublicKey.Value))
                    {
                        parents.Add(graph.ParentPublicKey.Value);
                    }
                    if (inputs.Count == 0 && graph.ParentPublicKey.HasValue)
                    {
                        condition = conditions[graph.ParentPublicKey.Value];
                    }
                    if (inputs.Count == 1)
                    {
                        var andList = new List<string>();
                        var c = inputs[0].Condition;
                        if (!string.IsNullOrWhiteSpace(c)) andList.Add(c);
                        c = conditions[inputs[0].Source];
                        if (!string.IsNullOrWhiteSpace(c)) andList.Add(c);
                        if (andList.Count == 1)
                            condition = andList[0];
                        else if (andList.Count > 1)
                            condition = "(" + string.Join(") and (", andList) + ")";
                    }
                    else if (inputs.Count > 1)
                    {
                        var orList = new List<string>();
                        foreach (var input in inputs)
                        {
                            var andList = new List<string>();
                            var c = input.Condition;
                            if (!string.IsNullOrWhiteSpace(c)) andList.Add(c);
                            c = conditions[input.Source];
                            if (!string.IsNullOrWhiteSpace(c)) andList.Add(c);
                            if (andList.Count > 1)
                                orList.Add("(" + string.Join(") and (", andList) + ")");
                            else if (andList.Count == 1)
                                orList.Add(andList[0]);
                        }
                        if (orList.Count > 1)
                            condition = "(" + string.Join(") or (", orList) + ")";
                        else if (orList.Count == 1)
                            condition = orList[0];
                    }

                    conditions.Add(block.PublicKey, condition);
                }
            }
            return conditions.Where(condition => !parents.Contains(condition.Key)).ToDictionary(condition => condition.Key, condition => condition.Value);
        }
    }
}
