using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Helpers
{
    public class GlobalInfoProvider: IGlobalInfoProvider
    {
        public UserLight GetCurrentUser()
        {
            return GlobalInfo.GetCurrentUser();
        }
    }
}
