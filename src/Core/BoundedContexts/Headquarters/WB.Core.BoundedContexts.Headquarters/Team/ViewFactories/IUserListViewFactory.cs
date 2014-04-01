using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Team.Models;

namespace WB.Core.BoundedContexts.Headquarters.Team.ViewFactories
{
    public interface IUserListViewFactory
    {
        List<UserDocument> GetActiveUsers(int pageSize);
    }
}