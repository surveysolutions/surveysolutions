using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RestSharp;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.QuestionnaireTester.Authentication;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.QuestionnaireTester.Services
{
    public class DesignerService
    {
        public bool Login(string userName, string password, CancellationToken cancellationToken)
        {
            var webExecutor = new AndroidRestUrils(CapiTesterApplication.GetPathToDesigner());
            try
            {
                webExecutor.ExcecuteRestRequestAsync<bool>(
                    "Authorize",cancellationToken,null,
                    new HttpBasicAuthenticator(userName, password), "POST");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public QuestionnaireListSyncPackage  GetQuestionnaireListForCurrentUser(CancellationToken cancellationToken)
        {
            if (!CapiTesterApplication.Membership.IsLoggedIn)
                return null;
            var webExecutor = new AndroidRestUrils(CapiTesterApplication.GetPathToDesigner());
            try
            {
                return webExecutor.ExcecuteRestRequestAsync<QuestionnaireListSyncPackage>(
                    "GetAllTemplates", cancellationToken, null,
                    new HttpBasicAuthenticator(CapiTesterApplication.Membership.RemoteUser.UserName,
                        CapiTesterApplication.Membership.RemoteUser.Password), "GET");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public QuestionnaireSyncPackage GetTemplateForCurrentUser(Guid id, CancellationToken cancellationToken)
        {
            if (!CapiTesterApplication.Membership.IsLoggedIn)
                return null;
            var webExecutor = new AndroidRestUrils(CapiTesterApplication.GetPathToDesigner());
            try
            {
                return webExecutor.ExcecuteRestRequestAsync<QuestionnaireSyncPackage>(
                    "GetTemplate", cancellationToken, null,
                    new HttpBasicAuthenticator(CapiTesterApplication.Membership.RemoteUser.UserName,
                        CapiTesterApplication.Membership.RemoteUser.Password), "GET", new KeyValuePair<string, string>("id", id.ToString()));
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}