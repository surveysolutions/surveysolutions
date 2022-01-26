using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    public class HQControllerBase : ControllerBase
    {
        protected bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.CreatedBy == User.GetId())
                return true;

            return questionnaireView.SharedPersons.Any(x => x.UserId == User.GetId());
        }
        
        protected bool ValidateAccessPermissionsOrAdmin(QuestionnaireView questionnaireView)
        {
            if (ValidateAccessPermissions(questionnaireView))
                return true;

            if (User.IsAdmin())
                return true;
            
            return false;
        }

    }
}
