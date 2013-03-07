using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.View;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Utils;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class WillBeApiController : Controller
    {
        protected internal new JsonResult Json(object data)
        {
            return new JsonWithTypeResult { Data = data };
        }

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WillBeApiController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public WillBeApiController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        //
        // GET: /WillBeApi/Questionnaire/id
        public JsonResult Questionnaire(Guid id)
        {
            var model = this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));

            return Json(new List<object>(){ model }, JsonRequestBehavior.AllowGet);
        }
    }
}
