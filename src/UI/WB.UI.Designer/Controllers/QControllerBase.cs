using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers;

public abstract class QControllerBase : Controller
{
    protected readonly DesignerDbContext dbContext;
    protected readonly IQuestionnaireViewFactory questionnaireViewFactory;

    protected QControllerBase(DesignerDbContext dbContext, IQuestionnaireViewFactory questionnaireViewFactory)
    {
        this.dbContext = dbContext;
        this.questionnaireViewFactory = questionnaireViewFactory;
    }

    protected bool ShouldRedirectToOriginalId(QuestionnaireRevision id)
    {
        if (!id.OriginalQuestionnaireId.HasValue || id.QuestionnaireId == id.OriginalQuestionnaireId)
            return false;

        if (User.Identity?.IsAuthenticated != true)
            return false;

        var userId = User.GetIdOrNull();
        if (!userId.HasValue)
            return false;

        if (User.IsAdmin())
            return true;

        var questionnaireId = id.OriginalQuestionnaireId.Value;
        var questionnaireListItem = this.dbContext.Questionnaires
            .Where(x => x.QuestionnaireId == questionnaireId.FormatGuid())
            .Include(x => x.SharedPersons).FirstOrDefault();

        if (questionnaireListItem == null)
            return false;

        if (questionnaireListItem.OwnerId == userId)
            return true;

        if (questionnaireListItem.IsPublic)
            return true;

        if (questionnaireListItem.SharedPersons.Any(x => x.UserId == userId))
            return true;

        return false;
    }
    
    protected bool UserHasAccessToEditOrViewQuestionnaire(QuestionnaireRevision id)
    {
        return this.questionnaireViewFactory.HasUserAccessToQuestionnaire(id, User.GetIdOrNull());
    }
    
    [Authorize]
    public IActionResult LackOfPermits()
    {
        this.Error(Resources.QuestionnaireController.Forbidden);
        return this.RedirectToAction("Index", "QuestionnaireList");
    }
}
