using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Code.Vue;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        [HttpGet]
        [VuePage]
        //[AntiForgeryFilter]
        [Route("q/details/{id}")]
        [Route("q/details/{id}/chapter/{chapterId}")]
        [Route("q/details/{id}/chapter/{chapterId}/{entityType}/{entityId}")]
        public IActionResult QuestionnaireDetails(QuestionnaireRevision? id, Guid? chapterId, string entityType, Guid? entityId)
        {
            if(id == null)
                return this.RedirectToAction("Index", "QuestionnaireList");

            var questionnaire = questionnaireViewFactory.Load(id);
            if (questionnaire == null || questionnaire.Source.IsDeleted)
                return NotFound();

            if (ShouldRedirectToOriginalId(id))
            {
                return RedirectToAction("Details", new RouteValueDictionary
                {
                    { "id", id.OriginalQuestionnaireId.FormatGuid() }, { "chapterId", chapterId?.FormatGuid() }, { "entityType", entityType }, { "entityid", entityId?.FormatGuid() }
                });
            }

            return (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id.QuestionnaireId))
                ? this.Ok()
                : this.LackOfPermits();
        }
    }
}
