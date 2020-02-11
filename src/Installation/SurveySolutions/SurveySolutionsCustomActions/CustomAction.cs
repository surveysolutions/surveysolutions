using Microsoft.Deployment.WindowsInstaller;

namespace SurveySolutionsCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult WriteDataToFile(Session session)
        {
            session.Log("Begin WriteDataToFile action");

            string content = session.CustomActionData["FILECONTENT"];

            session.Log("Content: " + content);
            return ActionResult.Success;
        }
    }
}
