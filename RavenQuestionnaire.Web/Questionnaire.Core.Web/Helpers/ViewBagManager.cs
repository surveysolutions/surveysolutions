using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.User;

namespace Questionnaire.Core.Web.Helpers
{
    public class ViewBagManager : IBagManager
    {
        public void AddUsersToBag(dynamic bag, IViewRepository viewRepository)
        {
            var users =
                viewRepository.Load<UserBrowseInputModel, UserBrowseView>(new UserBrowseInputModel() { PageSize = 300 }).Items;
            List<UserBrowseItem> list = users.ToList();
            bag.Users = list;
        }
    }
}
