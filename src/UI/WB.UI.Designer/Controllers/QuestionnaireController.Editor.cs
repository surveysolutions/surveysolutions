using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code.Vue;
using WB.UI.Designer.Filters;

namespace WB.UI.Designer.Controllers
{
    public partial class QuestionnaireController
    {
        [VuePage]
        [AntiForgeryFilter]
        [Route("q/details/{id}")]
        [Route("q/details/{id}/chapter/{chapterId}/{entityType}/{entityId}")]
        public IActionResult QuestionnaireDetails(QuestionnaireRevision? id, Guid? chapterId, string entityType, Guid? entityid)
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
                    { "id", id.OriginalQuestionnaireId.FormatGuid() }, { "chapterId", chapterId?.FormatGuid() }, { "entityType", entityType }, { "entityid", entityid?.FormatGuid() }
                });
            }

            return (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id.QuestionnaireId))
                ? this.View("~/questionnaire/index.cshtml")
                : this.LackOfPermits();
        }
    }
}
